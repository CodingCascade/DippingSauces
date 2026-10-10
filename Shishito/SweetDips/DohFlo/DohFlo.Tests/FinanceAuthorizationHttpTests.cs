using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using DohFlo.Data;
using DohFlo.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DohFlo.Tests
{
    public sealed class FinanceAuthorizationHttpTests : IClassFixture<FinanceTestFactory>
    {
        private readonly FinanceTestFactory _factory;

        public FinanceAuthorizationHttpTests(FinanceTestFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/Accounts")]
        [InlineData("/Transactions")]
        [InlineData("/Categories")]
        [InlineData("/Payees")]
        [InlineData("/AccountsLab")]
        [InlineData("/api/accounts")]
        public async Task Anonymous_CannotReadFinanceData(string path)
        {
            using var client = _factory.NewClient();
            using var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("/Accounts")]
        [InlineData("/Transactions")]
        [InlineData("/Categories")]
        [InlineData("/Payees")]
        [InlineData("/api/accounts")]
        [InlineData("/Accounts/Create")]
        [InlineData("/Transactions/Create")]
        [InlineData("/Categories/Create")]
        [InlineData("/Payees/Create")]
        public async Task StandardUser_CanReadFinancePagesAndCreateForms(string path)
        {
            using var client = _factory.NewClient("standard");
            using var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("/AccountsLab")]
        [InlineData("/AccountUsers")]
        [InlineData("/Auth/ChangePassword")]
        public async Task StandardUser_CannotReadAdminPages(string path)
        {
            using var client = _factory.NewClient("standard");
            using var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData("/AccountsLab")]
        [InlineData("/AccountUsers")]
        [InlineData("/Auth/ChangePassword")]
        public async Task Admin_CanReadAdminPages(string path)
        {
            using var client = _factory.NewClient("admin");
            using var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("admin", 2)]
        [InlineData("standard", 1)]
        public async Task User_CannotReadAnotherUserAccount(string user, int id)
        {
            using var client = _factory.NewClient(user);
            using var response = await client.GetAsync($"/api/accounts/{id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Home/PrivacyPolicy")]
        [InlineData("/Home/CookiePolicy")]
        [InlineData("/Home/SiteDisclaimer")]
        [InlineData("/Home/SiteMap")]
        [InlineData("/Home/ContactUs")]
        public async Task Anonymous_CanReadPublicPages(string path)
        {
            using var client = _factory.NewClient();
            using var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("admin", 1, 2)]
        [InlineData("standard", 2, 1)]
        public async Task User_ApiWriteWithToken_ChangesOnlyOwnedAccount(
            string user, int ownedId, int otherId)
        {
            using var client = _factory.NewClient(user);
            var token = await GetTokenAsync(client);

            using var request = CreateStatusRequest(ownedId, true, token);
            using var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DohFloContext>();
            Assert.True((await db.Accounts.SingleAsync(a => a.Id == ownedId)).IsClosed);

            // Restore this user's row so fixture state does not depend on test order.
            using var reset = CreateStatusRequest(ownedId, false, token);
            using var resetResponse = await client.SendAsync(reset);
            Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);
            db.ChangeTracker.Clear();
            Assert.False((await db.Accounts.SingleAsync(a => a.Id == otherId)).IsClosed);
        }

        [Theory]
        [InlineData("admin")]
        [InlineData("standard")]
        public async Task User_ApiWriteWithoutAntiforgeryToken_IsRejected(string user)
        {
            using var client = _factory.NewClient(user);
            using var request = CreateStatusRequest(user == "admin" ? 1 : 2, true);
            using var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("admin", 2)]
        [InlineData("standard", 1)]
        public async Task User_CannotUpdateAnotherUserAccount(string user, int otherId)
        {
            using var client = _factory.NewClient(user);
            var token = await GetTokenAsync(client);
            using var request = CreateStatusRequest(otherId, true, token);
            using var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DohFloContext>();
            Assert.False((await db.Accounts.SingleAsync(a => a.Id == otherId)).IsClosed);
        }

        private static async Task<string> GetTokenAsync(HttpClient client)
        {
            using var response = await client.GetAsync("/api/security/antiforgery");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var json = System.Text.Json.JsonDocument.Parse(
                await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("requestToken").GetString()
                ?? throw new InvalidOperationException("The antiforgery token is missing.");
        }

        private static HttpRequestMessage CreateStatusRequest(
            int id, bool isClosed, string? token = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/accounts/{id}/status")
            {
                Content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(new { isClosed }),
                    System.Text.Encoding.UTF8, "application/json")
            };

            if (token is not null)
            {
                request.Headers.Add("X-CSRF-TOKEN", token);
            }

            return request;
        }
    }

    public sealed class FinanceTestFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");
        private bool _seeded;
        public FinanceTestFactory()
        {
            _connection.Open();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _connection.Dispose();
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
                new Dictionary<string, string?> { ["Bootstrap:Enabled"] = "false" }));

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DohFloContext>();
                services.RemoveAll<DbContextOptions<DohFloContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<DohFloContext>>();
                services.AddDbContext<DohFloContext>(options => options.UseSqlite(_connection));

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                    options.DefaultForbidScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, FinanceTestAuthHandler>("Test", _ => { });
            });
        }

        public HttpClient NewClient(string? user = null)
        {
            if (!_seeded)
            {
                using var scope = Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DohFloContext>();
                db.Database.EnsureCreated();

                db.Users.AddRange(
                    new User { Id = 1, UserName = "admin", NormalizedUserName = "ADMIN", Email = "admin@example.invalid", NormalizedEmail = "ADMIN@EXAMPLE.INVALID", SecurityStamp = "admin-stamp" },
                    new User { Id = 2, UserName = "standard", NormalizedUserName = "STANDARD", Email = "standard@example.invalid", NormalizedEmail = "STANDARD@EXAMPLE.INVALID", SecurityStamp = "standard-stamp" });

                db.Accounts.AddRange(
                    new Account { Id = 1, UserId = 1, Name = "Admin account", Type = "Checking" },
                    new Account { Id = 2, UserId = 2, Name = "Standard account", Type = "Checking" });

                db.SaveChanges();
                _seeded = true;
            }

            var client = CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });

            if (user is not null)
            {
                client.DefaultRequestHeaders.Add("X-Test-User", user);
            }

            return client;
        }

        public sealed class FinanceTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public FinanceTestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder) { }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                var user = Request.Headers["X-Test-User"].ToString();

                if (user != "admin" && user != "standard")
                {
                    return Task.FromResult(AuthenticateResult.NoResult());
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user == "admin" ? "1" : "2"),
                    new Claim(ClaimTypes.Role, user == "admin" ? FinanceAccess.AdminRole : FinanceAccess.StandardUserRole)
                };

                var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));

                return Task.FromResult(AuthenticateResult.Success(
                    new AuthenticationTicket(principal, Scheme.Name)));
            }
        }
    }
}