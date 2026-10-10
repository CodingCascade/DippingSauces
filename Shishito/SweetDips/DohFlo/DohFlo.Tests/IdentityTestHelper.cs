using DohFlo.Security;
using System.Security.Claims;
using DohFlo.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DohFlo.Tests
{
    internal static class IdentityTestHelper
    {
        public static async Task SeedAdminAsync(DohFloContext db)
        {
            if (!await db.Users.AnyAsync(u => u.Id ==1))
            {
                db.Users.Add(new User
                {
                    Id = 1,
                    UserName = "test-admin",
                    NormalizedUserName = "TEST-ADMIN",
                    Email = "admin@example.invalid",
                    NormalizedEmail = "ADMIN@EXAMPLE.INVALID",
                    SecurityStamp = "test-security-stamp",
                    ConcurrencyStamp = "test-concurrency-stamp"
                });

                await db.SaveChangesAsync();
            }
        }

        public static void SignIn(ControllerBase controller, int userId = 1, string role = FinanceAccess.AdminRole)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                        new Claim(ClaimTypes.Role, role)
                    }, "Test"))
                }
            };
        }
    }
}
