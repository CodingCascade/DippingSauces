using DohFlo.Controllers;
using DohFlo.Data;
using DohFlo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DohFlo.Tests
{
    public class TransactionsControllerIntegrationTests
    {
        [Fact]
        public async Task Edit_ValidOwnedTransaction_UpdatesTransaction()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");

            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<DohFloContext>()
                .UseSqlite(connection)
                .Options;

            await using var db = new DohFloContext(options);
            await db.Database.EnsureCreatedAsync();

            var account = new Account
            {
                UserId = 1,
                Name = "Primary Checking",
                Type = "Checking",
                CurrencyCode = "USD"
            };

            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            var transaction = new Transaction
            {
                UserId = 1,
                AccountId = account.Id,
                Amount = 25.00m,
                CurrencyCode = "USD",
                Date = new DateTime(2026, 9, 22),
                Notes = "Original notes",
                IsPending = true
            };

            db.Transactions.Add(transaction);
            await db.SaveChangesAsync();

            var controller = CreateController(db);

            var viewModel = new EditTransactionViewModel
            {
                Id = transaction.Id,
                AccountId = account.Id,
                Amount = 45.50m,
                CurrencyCode = "usd",
                Date = new DateTime(2026, 9, 23),
                Notes = " Updated notes ",
                IsPending = false
            };

            // Act
            var result = await controller.Edit(viewModel);

            // Assert 
            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal(nameof(TransactionsController.Index),
                redirect.ActionName);

            db.ChangeTracker.Clear();

            var savedTransaction = await db.Transactions
                .SingleAsync(saved => saved.Id == transaction.Id);

            Assert.Equal(45.50m, savedTransaction.Amount);
            Assert.Equal("USD", savedTransaction.CurrencyCode);
            Assert.Equal("Updated notes", savedTransaction.Notes);
            Assert.False(savedTransaction.IsPending);
            Assert.NotNull(savedTransaction.ClearedDate);
            Assert.False(savedTransaction.IsDeleted);
        }

        [Fact]
        public async Task Delete_ExistingOwnedTransaction_SoftDeletesTransaction()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");

            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<DohFloContext>()
                .UseSqlite(connection)
                .Options;

            await using var db = new DohFloContext(options);
            await db.Database.EnsureCreatedAsync();

            var account = new Account
            {
                UserId = 1,
                Name = "Primary Checking",
                Type = "Checking",
                CurrencyCode = "USD"
            };

            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            var transaction = new Transaction
            {
                UserId = 1,
                AccountId = account.Id,
                Amount = 25.00m,
                CurrencyCode = "USD",
                Date = new DateTime(2026, 9, 22),
                Notes = "Keep this database record"
            };

            db.Transactions.Add(transaction);
            await db.SaveChangesAsync();

            var controller = CreateController(db);

            // Act
            var result = await controller.Delete(transaction.Id);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal(nameof(TransactionsController.Index), redirect.ActionName);

            db.ChangeTracker.Clear();

            var savedTransaction = await db.Transactions
                .SingleAsync(saved => saved.Id == transaction.Id);

            Assert.True(savedTransaction.IsDeleted);
            Assert.NotNull(savedTransaction.DeletedAt);

            // Soft delete means the row still exists.
            Assert.Equal(1, await db.Transactions.CountAsync());
        }

        private static TransactionsController CreateController(DohFloContext db)
        {
            var controller = new TransactionsController(db);

            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                new FakeTempDataProvider());

            return controller;
        }

        private sealed class FakeTempDataProvider : ITempDataProvider
        {
            public IDictionary<string, object> LoadTempData(HttpContext context)
            {
                return new Dictionary<string, object>();
            }

            public void SaveTempData(HttpContext context, IDictionary<string, object> values)
            {
                // For testing here
            }
        }
    }
}
