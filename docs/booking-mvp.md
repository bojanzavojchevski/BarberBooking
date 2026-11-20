# BarberBooking booking MVP

This branch extends the existing Week 1–3 backend and basic React catalog.

## Implemented

- Customer registration and JWT login/refresh using the existing identity infrastructure.
- Public shop discovery, services, barbers and per-day availability.
- Owner management of barbers, services, weekly shifts, optional time off and appointments.
- Customer booking, listing and cancellation.
- Local scheduling based on `Europe/Skopje`. All stored appointment instants are UTC.
- PostgreSQL partial exclusion constraint over confirmed appointment intervals. Two simultaneous requests cannot reserve overlapping time ranges for the same barber.
- Price and duration are based on the service when the appointment is created; price is snapshotted in the appointment.
- Integration test for the concurrent booking conflict.

## Local Docker quick start

Prerequisite: Docker Desktop running.

1. Copy `.env.example` to `.env.compose` and replace the sample secret values with **unique strong random values**.
2. From the repository root run:

   ```powershell
   docker compose --env-file .env.compose up --build
   ```

3. Open `http://localhost:3000`. The frontend proxies `/api/` to the backend at port 8080.
4. To provision a **development-only** owner account, supply the exact `DEV_SEED_KEY` from `.env.compose` as `X-Dev-Seed-Key`:

   ```powershell
   $secret = "<DEV_SEED_KEY_FROM_ENV_FILE>"
   $body = @{ email = "owner@example.test"; password = "<YOUR_STRONG_OWNER_PASSWORD>" } | ConvertTo-Json
   Invoke-RestMethod -Method Post -Uri "http://localhost:8080/api/dev/seed/owner" -Headers @{ "X-Dev-Seed-Key" = $secret } -ContentType "application/json" -Body $body
   ```

5. Sign in with that owner account, create the shop, add services, add barbers and set working hours. Time slots do not appear until a weekly schedule is configured.
6. Register as a customer in a separate browser session, sign in, and book an available appointment.

To shut down while keeping data: `docker compose down`. **Do not** add `-v` unless you intend to delete the database volume.

## API

| Method | Endpoint | Access |
| --- | --- | --- |
| POST | `/api/auth/register` | Public customer registration |
| POST | `/api/auth/login` | Public |
| POST | `/api/auth/refresh` | Public |
| GET | `/api/public/shops` | Public |
| GET | `/api/public/shops/{slug}` | Public |
| GET | `/api/public/availability?shopId=&barberId=&serviceId=&date=YYYY-MM-DD` | Public |
| POST | `/api/bookings` | Customer |
| GET | `/api/bookings/mine` | Customer |
| DELETE | `/api/bookings/{id}` | Customer |
| GET | `/api/owner/bookings` | Owner |
| DELETE | `/api/owner/bookings/{id}` | Owner |
| GET/PUT | `/api/owner/barbers/{barberId}/schedule` | Owner |
| POST | `/api/owner/barbers/{barberId}/time-off` | Owner |

Owner schedule example:

```json
{
  "shifts": [
    { "dayOfWeek": 1, "start": "09:00", "end": "17:00" },
    { "dayOfWeek": 2, "start": "09:00", "end": "17:00" }
  ]
}
```

Days: Sunday=0 through Saturday=6. Shifts have 15-minute resolution. Availability is limited to the next 30 days and includes a 15-minute lead time. Customers can cancel until two hours before the scheduled start; owners can cancel at any time.

## Known limitations before production

- This is an MVP, not a hardened production release. Require HTTPS, secure deployment configuration and a full threat review before accepting real customer data.
- No online payments, SMS/email booking notifications, automated reminders or self-service barber identity linking.
- Owner schedule edits and time-off creation do not share a cross-request lock with booking writes, so a booking/schedule-edit race may need additional database transaction coordination.
- The demo login uses sessionStorage tokens. Review your browser token strategy, CSRF model, and account recovery UX before public launch.
- The email sender is the existing development log sender; connect a real email delivery service and revisit the confirmed-email policy.
- The migration contains PostgreSQL-specific SQL. Review it before running on non-disposable databases and take backups.
- Run `dotnet test`, `npm run build` and the full GitHub Actions CI before merging.

