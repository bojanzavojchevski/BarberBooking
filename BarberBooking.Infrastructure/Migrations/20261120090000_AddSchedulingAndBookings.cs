using BarberBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BarberBooking.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261120090000_AddSchedulingAndBookings")]
public sealed class AddSchedulingAndBookings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Database-level exclusion constraint prevents overlapping confirmed bookings,
        // including simultaneous requests handled by different API instances.
        migrationBuilder.Sql("""
            CREATE EXTENSION IF NOT EXISTS btree_gist;

            CREATE TABLE appointments (
                "Id" uuid NOT NULL PRIMARY KEY,
                "ShopId" uuid NOT NULL REFERENCES shops("Id"),
                "BarberId" uuid NOT NULL REFERENCES barbers("Id"),
                "ServiceId" uuid NOT NULL REFERENCES services("Id"),
                "CustomerId" uuid NOT NULL REFERENCES "AspNetUsers"("Id"),
                "StartUtc" timestamptz NOT NULL,
                "EndUtc" timestamptz NOT NULL,
                "PriceAmount" numeric(12,2) NOT NULL,
                "Status" integer NOT NULL DEFAULT 0,
                "CreatedAtUtc" timestamptz NOT NULL,
                "CancelledAtUtc" timestamptz NULL,
                CONSTRAINT appointments_time_valid CHECK ("EndUtc" > "StartUtc"),
                CONSTRAINT appointments_status_valid CHECK ("Status" IN (0, 1))
            );
            CREATE INDEX ix_appointments_customer_time ON appointments ("CustomerId", "StartUtc");
            CREATE INDEX ix_appointments_barber_time ON appointments ("BarberId", "StartUtc");
            ALTER TABLE appointments ADD CONSTRAINT appointments_no_overlap
                EXCLUDE USING gist (
                    "BarberId" WITH =,
                    tstzrange("StartUtc", "EndUtc", '[)') WITH &&
                ) WHERE ("Status" = 0);

            CREATE TABLE weekly_shifts (
                "Id" uuid NOT NULL PRIMARY KEY,
                "BarberId" uuid NOT NULL REFERENCES barbers("Id") ON DELETE CASCADE,
                "DayOfWeek" integer NOT NULL,
                "StartMinute" integer NOT NULL,
                "EndMinute" integer NOT NULL,
                CONSTRAINT weekly_shifts_valid CHECK (
                    "DayOfWeek" BETWEEN 0 AND 6 AND
                    "StartMinute" >= 0 AND "EndMinute" <= 1440 AND
                    "EndMinute" > "StartMinute" AND
                    "StartMinute" % 15 = 0 AND "EndMinute" % 15 = 0)
            );
            CREATE INDEX ix_weekly_shifts_barber_day ON weekly_shifts ("BarberId", "DayOfWeek");

            CREATE TABLE barber_time_off (
                "Id" uuid NOT NULL PRIMARY KEY,
                "BarberId" uuid NOT NULL REFERENCES barbers("Id") ON DELETE CASCADE,
                "StartUtc" timestamptz NOT NULL,
                "EndUtc" timestamptz NOT NULL,
                "Note" varchar(250),
                CONSTRAINT barber_time_off_valid CHECK ("EndUtc" > "StartUtc")
            );
            CREATE INDEX ix_barber_time_off_barber_start ON barber_time_off ("BarberId", "StartUtc");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE barber_time_off;
            DROP TABLE weekly_shifts;
            DROP TABLE appointments;
            """);
    }
}
