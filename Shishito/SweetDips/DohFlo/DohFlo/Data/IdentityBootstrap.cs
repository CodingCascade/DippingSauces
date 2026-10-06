using DohFlo.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DohFlo.Data
{
    public static class IdentityBootstrap
    {
        public static async Task InitializeAsync(IServiceProvider services, IConfiguration config)
        {
            var users = services.GetRequiredService<UserManager<User>>();
            var roles = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var db = services.GetRequiredService<DohFloContext>();

            foreach (var name in new[] { FinanceAccess.OwnerRole, FinanceAccess.DemoRole })

                    if (!await roles.RoleExistsAsync(name))
                    {
                        Check(await roles.CreateAsync(new IdentityRole<int>(name)));
                    }

            // Explicitly claim the backed-up local devlopment owner row.
            var ownerId = config.GetValue<int>("Bootstrap:LegacyOwnerId");
            var owner = await users.FindByIdAsync(ownerId.ToString()) ?? throw new InvalidOperationException("The configured legacy owner row is missing.");

            if (await users.IsInRoleAsync(owner, FinanceAccess.DemoRole))
                throw new InvalidOperationException("The owner must not have the demo role.");

            if (!await users.HasPasswordAsync(owner))
            {
                var legacyMatch = string.Equals(owner.Email, Required(config, "LegacyOwnerEmail"), StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(owner.UserName);

                var retryMatch = string.Equals(owner.UserName, Required(config, "OwnerUserName"), StringComparison.OrdinalIgnoreCase) && string.Equals(owner.Email, Required(config, "OwnerEmail"), StringComparison.OrdinalIgnoreCase);

                if (!legacyMatch && !retryMatch)
                    throw new InvalidOperationException("Legacy owner identity did not match. Review the database.");

                owner.UserName = Required(config, "OwnerUserName");
                owner.Email = Required(config, "OwnerEmail");
                owner.SecurityStamp = Guid.NewGuid().ToString();
                owner.ConcurrencyStamp = Guid.NewGuid().ToString();
                owner.LockoutEnabled = true;
                Check(await users.UpdateAsync(owner));
                Check(await users.AddPasswordAsync(owner, Required(config, "OwnerPassword")));

                if (!await users.IsInRoleAsync(owner, FinanceAccess.OwnerRole))
                    Check(await users.AddToRoleAsync(owner, FinanceAccess.OwnerRole));

                var demoName = Required(config, "DemoUserName");
                var demo = await users.FindByNameAsync(demoName);

                if (demo is null)
                {
                    demo = new User
                    {
                        UserName = demoName,
                        Email = "demo@example.invalid",
                        DisplayName = "Demo Viewer",
                        LockoutEnabled = true
                    };

                    Check(await users.CreateAsync(demo, Required(config, "DemoPassword")));
                }

                if (demo.Id == owner.Id || await users.IsInRoleAsync(demo, FinanceAccess.OwnerRole))
                    throw new InvalidOperationException("The demo must be separate from the owner.");

                if (!await users.IsInRoleAsync(demo, FinanceAccess.DemoRole))
                    Check(await users.AddToRoleAsync(demo, FinanceAccess.DemoRole));

                if (!await db.Accounts.AnyAsync(a => a.UserId == demo.Id))
                {
                    var account = new Account
                    {
                        UserId = demo.Id,
                        Name = "Sample checking",
                        Type = "Checking",
                        Institution = "Fictional Demo Bank",
                        CurrencyCode = "USD"
                    };

                    var payee = new Payee
                    {
                        UserId = demo.Id,
                        Name = "Sample Grocery",
                        NormalizedName = "SAMPLE GRCERY"
                    };

                    var category = new Category
                    {
                        UserId = demo.Id,
                        Name = "Sample Groceries",
                        CatType = "Expense"
                    };

                    db.AddRange(account, payee, category);
                    db.Transactions.Add(new Transaction
                    {
                        UserId = demo.Id,
                        Account = account,
                        Payee = payee,
                        Category = category,
                        Amount = 25m, 
                        Date = new DateTime(2026, 10, 1),
                        CurrencyCode = "USD",
                        Status = TransactionStatus.Pending,
                        Notes = "Fictional sample transaction"
                    });

                    await db.SaveChangesAsync();
                }
            }
        }

        private static string Required(IConfiguration config, string name) =>
        !string.IsNullOrWhiteSpace(config[$"Bootstrap:{name}"])
            ? config[$"Bootstrap:{name}"]!
            : throw new InvalidOperationException($"Missing bootstrap setting {name}.");

        private static void Check(IdentityResult result)
        {
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
