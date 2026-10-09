using System.Security.Claims;

namespace DohFlo.Security
{
    public static class FinanceAccess
    {
        public const string ReadPolicy = "CanReadFinanceData";
        public const string WritePolicy = "CanWriteFinanceData";
        public const string AdminPolicy = "AdminOnly";

        public const string AdminRole = "Admin";
        public const string StandardUserRole = "StandardUser";

        public static int GetUserId(ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (principal.Identity?.IsAuthenticated != true || !int.TryParse(value, out var id) || id <= 0)
            {
                throw new InvalidOperationException("A valid authenticated user ID is required.");
            }

            return id;
        }
    }
}