using DohFlo.Data;
using DohFlo.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DohFlo.Controllers.Api
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountsApiController : ControllerBase
    {
        private readonly DohFloContext _db;
        private readonly ILogger<AccountsApiController> _logger;
        private const int BoolieUserId = 1;

        public AccountsApiController(
            DohFloContext db,
            ILogger<AccountsApiController> logger)
        {
            _db = db;
            _logger = logger;
        }

        private static AccountDto ToDto(Account account) => new(
            account.Id,
            account.Name,
            account.Type,
            account.Institution,
            account.CurrencyCode,
            account.IsClosed);

        // GET all accounts
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AccountDto>>> GetAll()
        {
            var accounts = await _db.Accounts
                .AsNoTracking()
                .Where(account => account.UserId == BoolieUserId)
                .OrderBy(account => account.IsClosed)
                .ThenBy(account => account.Name)
                .Select(account => new AccountDto(
                    account.Id,
                    account.Name,
                    account.Type,
                    account.Institution,
                    account.CurrencyCode,
                    account.IsClosed))
                .ToListAsync();

            return Ok(accounts);
        }

        // Get one account
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AccountDto>> GetById(int id)
        {
            var account = await _db.Accounts
                .AsNoTracking()
                .SingleOrDefaultAsync(account =>
                account.Id == id &&
                account.UserId == BoolieUserId);

            if (account is null)
            {
                _logger.LogWarning("API account #{AccountId} was not found for the current user.", id);

                return NotFound();
            }

            return Ok(ToDto(account));
        }

        // POST Create
        [HttpPost]
        public async Task<ActionResult<AccountDto>> Create(AccountWriteRequest request)
        {
            var name = request.Name.Trim();

            var duplicateExists = await _db.Accounts.AnyAsync(account =>
                account.UserId == BoolieUserId &&
                account.Name == name);

            if (duplicateExists)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Duplicate account name",
                    Detail = "You already have an account with this name.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            var account = new Account
            {
                UserId = BoolieUserId,
                Name = name,
                Type = request.Type,
                Institution = request.Institution?.Trim() ?? "",
                CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
                IsClosed = false
            };

            _db.Accounts.Add(account);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "API created account #{AccountId} for the current user.", account.Id);

            var response = ToDto(account);

            return CreatedAtAction(
                nameof(GetById),
                new { id = account.Id },
                response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, AccountWriteRequest request)
        {
            var account = await _db.Accounts.SingleOrDefaultAsync(account =>
                account.Id == id &&
                account.UserId == BoolieUserId);

            if (account is null)
            {
                return NotFound();
            }

            var name = request.Name.Trim();

            var duplicateExists = await _db.Accounts.AnyAsync(other =>
                other.UserId == BoolieUserId &&
                other.Id != id &&
                other.Name == name);

            if (duplicateExists)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Duplicate account name",
                    Detail = "You already have another account with this name.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            account.Name = name;
            account.Type = request.Type;
            account.Institution = request.Institution?.Trim() ?? "";
            account.CurrencyCode = request.CurrencyCode
                .Trim()
                .ToUpperInvariant();

            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "API updated account #{AccountId} for the current user.", id);

            return NoContent();
        }

        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<AccountDto>> UpdateStatus(int id, AccountStatusRequest request)
        {
            var account = await _db.Accounts.SingleOrDefaultAsync(account =>
            account.Id == id &&
            account.UserId == BoolieUserId);

            if (account is null)
            {
                return NotFound();
            }

            account.IsClosed = request.IsClosed;
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "API changed account #{AccountId} status to {IsClosed}.",
                id,
                account.IsClosed);

            return Ok(ToDto(account));
        }
    }
}
