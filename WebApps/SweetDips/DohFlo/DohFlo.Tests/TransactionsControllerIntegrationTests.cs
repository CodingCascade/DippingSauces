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
                Status = TransactionStatus.Pending
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
                Status = TransactionStatus.Cleared
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
            Assert.Equal(TransactionStatus.Cleared, savedTransaction.Status);
            Assert.NotNull(savedTransaction.ClearedDate);
            Assert.Null(savedTransaction.ReconciledDate);
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

        private static async Task<(SqliteConnection Connection, DohFloContext Db, int Id)>
            CreateStatusFixtureAsync(TransactionStatus status, DateTime? clearedDate = null, DateTime? reconciledDate = null)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<DohFloContext>()
                .UseSqlite(connection).Options;

            var db = new DohFloContext(options);
            await db.Database.EnsureCreatedAsync();

            var account = new Account
            {
                UserId = 1, Name = "Primary Checking",
                Type = "Checking", CurrencyCode = "USD"
            };

            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            var transaction = new Transaction
            {
                UserId = 1,
                AccountId = account.Id,
                Amount = 25m,
                CurrencyCode = "USD",
                Date = new DateTime(2026, 9, 24),
                Status = status, 
                ClearedDate  = clearedDate,
                ReconciledDate = reconciledDate
            };

            db.Transactions.Add(transaction);
            await db.SaveChangesAsync();

            return (connection, db, transaction.Id);

        }

        [Fact]
        public async Task UpdateStatus_ClearedToReconciled_PreservesClearedDate()
        {
            var clearedAt = new DateTime(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc);
            var fixture = await CreateStatusFixtureAsync(TransactionStatus.Cleared, clearedDate: clearedAt);

            await using var db = fixture.Db;
            using var connection = fixture.Connection;

            var controller = CreateController(db);
            var before = DateTime.UtcNow;

            var result = await controller.UpdateStatus(fixture.Id, TransactionStatus.Reconciled);

            var after = DateTime.UtcNow;

            Assert.Equal(nameof(TransactionsController.Index),
                Assert.IsType<RedirectToActionResult>(result).ActionName);

            db.ChangeTracker.Clear();
            var saved = await db.Transactions.SingleAsync(t => t.Id == fixture.Id);

            Assert.Equal(TransactionStatus.Reconciled, saved.Status);
            Assert.Equal(clearedAt, saved.ClearedDate);
            Assert.NotNull(saved.ReconciledDate);
            Assert.InRange(saved.ReconciledDate.Value, before, after);
        }

        // Test Reconciled to pending
        [Fact]
        public async Task UpdateStatus_ReconciledToPending_ClearsBothDates()
        {
            var clearedAt = new DateTime(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc);
            var reconciledAt = clearedAt.AddDays(1);

            var fixture = await CreateStatusFixtureAsync(TransactionStatus.Reconciled, clearedAt, reconciledAt);

            await using var db = fixture.Db;
            using var connection = fixture.Connection;

            var result = await CreateController(db).UpdateStatus(
                fixture.Id, TransactionStatus.Pending);

            Assert.IsType<RedirectToActionResult>(result);

            db.ChangeTracker.Clear();
            var saved = await db.Transactions.SingleAsync(t => t.Id == fixture.Id);

            Assert.Equal(TransactionStatus.Pending, saved.Status);
            Assert.Null(saved.ClearedDate);
            Assert.Null(saved.ReconciledDate);
        }

        // Test that selecting the same status preserves dates to catch accidental overwrites when a status action is repeated - use a known date so the assertion is exact.
        [Fact]
        public async Task UpdateStatus_ReconciledAgain_PreservesDates()
        {
            var clearedAt = new DateTime(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc);
            var reconciledAt = clearedAt.AddDays(1);
            var fixture = await CreateStatusFixtureAsync(TransactionStatus.Reconciled, clearedAt, reconciledAt);

            await using var db = fixture.Db;
            using var connection = fixture.Connection;

            await CreateController(db).UpdateStatus(
                fixture.Id, TransactionStatus.Reconciled);
            db.ChangeTracker.Clear();

            var saved = await db.Transactions.SingleAsync(t => t.Id == fixture.Id);

            Assert.Equal(clearedAt, saved.ClearedDate);
            Assert.Equal(reconciledAt, saved.ReconciledDate);
        }

        // Test an invalid status value - Enum biding can accept a number that has no defined name. The controller should return BadRequest before changing the database.
        [Fact]
        public async Task UpdateStatus_InvalidStatus_DoesNotChangeTransaction()
        {
            var fixture = await CreateStatusFixtureAsync(TransactionStatus.Pending);
            await using var db = fixture.Db;

            using var connection = fixture.Connection;

            var result = await CreateController(db).UpdateStatus(
                fixture.Id, (TransactionStatus)999);

            Assert.IsType<BadRequestResult>(result);
            db.ChangeTracker.Clear();

            var saved = await db.Transactions.SingleAsync(t => t.Id == fixture.Id);

            Assert.Equal(TransactionStatus.Pending, saved.Status);
            Assert.Null(saved.ClearedDate);
            Assert.Null(saved.ReconciledDate);
        }


    }
}
