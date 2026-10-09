namespace DohFlo.Models.Admin
{
    public sealed class AccountUserRowViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsAdmin { get; set; }
        public bool IsBlocked { get; set; }
    }
}
