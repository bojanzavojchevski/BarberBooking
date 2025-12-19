using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Infrastructure.Identity;

public static class RoleSeeder
{
    public static readonly string[] Roles =
        [
            "Customer",
            "Barber",
            "Owner",
            "Admin"
        ];

    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken ct = default)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach(var roleName in Roles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Failed to seed role '{roleName}': {errors}");
            }
        }
    }
}
