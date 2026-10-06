using DohFlo.Controllers.Api;
using DohFlo.Data;
using DohFlo.Models;
using DohFlo.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DohFlo.Tests
{
    public class AccountsApiControllerIntegrationTests
    {
        [Fact]
        public async Task GetById_OwnedAccount_returnsSafeDto()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
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
                Institution = "Example Bank",
                CurrencyCode = "USD",
                IsClosed = false
            };

            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            var controller = new AccountsApiController(db, NullLogger<AccountsApiController>.Instance);

            var actionResult = await controller.GetById(account.Id);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var dto = Assert.IsType<AccountDto>(okResult.Value);

            Assert.Equal(account.Id, dto.Id);
            Assert.Equal("Primary Checking", dto.Name);
            Assert.False(dto.IsClosed);
        }

        [Fact]
        public async Task UpdateStatus_OwnedAccount_ClosesWithoutDeleting()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
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
                Institution = "Example bank",
                CurrencyCode = "USD",
                IsClosed = false
            };

            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            var controller = new AccountsApiController(db, NullLogger<AccountsApiController>.Instance);

            var actionResult = await controller.UpdateStatus(account.Id, new AccountStatusRequest { IsClosed = true });

            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var dto = Assert.IsType<AccountDto>(okResult.Value);

            Assert.True(dto.IsClosed);
            Assert.Equal(1, await db.Accounts.CountAsync());
            Assert.True((await db.Accounts.SingleAsync()).IsClosed);
        } 
        
    }
}
