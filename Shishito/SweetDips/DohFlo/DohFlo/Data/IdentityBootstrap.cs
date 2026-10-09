using DohFlo.Security;
using Microsoft.AspNetCore.Identity;

namespace DohFlo.Data
{
    public static class IdentityBootstrap
    {
        public static async Task InitializeAsync(
        IServiceProvider services, IConfiguration config)
        {
            var users = services.GetRequiredService<UserManager<User>>();
            var roles = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

            foreach (var roleName in new[]
                     { FinanceAccess.AdminRole, FinanceAccess.StandardUserRole })
            {
                if (!await roles.RoleExistsAsync(roleName))
                {
                    Check(await roles.CreateAsync(
                        new IdentityRole<int>(roleName)));
                }
            }

            var adminId = config.GetValue<int>("Bootstrap:LegacyAdminId");
            var admin = await users.FindByIdAsync(adminId.ToString())
                ?? throw new InvalidOperationException(
                    "The configured legacy Admin row is missing.");

            if (!await users.HasPasswordAsync(admin))
            {
                var legacyEmail = Required(config, "LegacyAdminEmail");
                if (!string.Equals(admin.Email, legacyEmail,
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "Legacy Admin identity did not match the database.");
                }

                admin.UserName = Required(config, "AdminEmail");
                admin.Email = Required(config, "AdminEmail");
                admin.EmailConfirmed = true;
                admin.IsBlocked = false;
                admin.SecurityStamp = Guid.NewGuid().ToString();
                admin.ConcurrencyStamp = Guid.NewGuid().ToString();
                admin.LockoutEnabled = true;

                Check(await users.UpdateAsync(admin));
                Check(await users.AddPasswordAsync(
                    admin, Required(config, "AdminPassword")));
            }

            if (!await users.IsInRoleAsync(admin, FinanceAccess.AdminRole))
            {
                Check(await users.AddToRoleAsync(admin, FinanceAccess.AdminRole));
            }
        }

        private static string Required(IConfiguration config, string name) =>
            !string.IsNullOrWhiteSpace(config[$"Bootstrap:{name}"])
                ? config[$"Bootstrap:{name}"]!
                : throw new InvalidOperationException(
                    $"Missing bootstrap setting {name}.");

        private static void Check(IdentityResult result)
        {
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
} 
