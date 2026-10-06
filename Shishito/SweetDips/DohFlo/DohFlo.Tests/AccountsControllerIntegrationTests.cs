using DohFlo.Controllers;
using DohFlo.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DohFlo.Tests
{
    public class AccountsControllerIntegrationTests
    {
        public object Mock { get; private set; } = new object();

        public class FakeTempDataProvider : ITempDataProvider
        {
            public IDictionary<string, object> LoadTempData(HttpContext context)
            {
                return new Dictionary<string, object>();
            }

            public void SaveTempData(HttpContext context, IDictionary<string, object> values)
            {
                // Do nothing because this is only for the test.
            }
        }

        [Fact]
        public async Task ToggleClosed_ExistingOwnedAccount_FlipsStatusWithoutDeleting()
        {
            // Arrange - keep on SQLite connection alive for the whole test.
            using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<DohFloContext>()
                .UseSqlite(connection)
                .Options;

            await using var db = new DohFloContext(options);
            await db.Database.EnsureCreatedAsync();

            var originalName = "Primary Checking";

            var account = new Account
            {
                UserId = 1,
                Name = originalName,
                Institution = "Example Bank",
                CurrencyCode = "USD",
                IsClosed = false
            };

            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            var controller = new AccountsController(db, NullLogger<AccountsController>.Instance);

            // Initialize TempData because the ToggleClosed() uses it.
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new FakeTempDataProvider());

            // Act
            await controller.ToggleClosed(account.Id);

            // Assert
            db.ChangeTracker.Clear();

            var savedAccount = await db.Accounts
                .SingleAsync(saved => saved.Id == account.Id);

            Assert.True(savedAccount.IsClosed);
            Assert.Equal(originalName, savedAccount.Name);
            Assert.Equal(1, await db.Accounts.CountAsync());
        }

        
    }
}
