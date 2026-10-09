using System.ComponentModel.DataAnnotations;

namespace DohFlo.Models.Admin
{
    public sealed class CreateAccountUserViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MaxLength(150), Display(Name = "Display Name")]
        public string DisplayName { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = "";
    }
}
