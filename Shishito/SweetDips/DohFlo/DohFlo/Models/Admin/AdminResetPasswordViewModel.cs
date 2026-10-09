using System.ComponentModel.DataAnnotations;

namespace DohFlo.Models.Admin
{
    public sealed class AdminResetPasswordViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password), Display(Name = "New Password")]
        public string NewPassword { get; set; } = "";

        [Required, DataType(DataType.Password), Compare(nameof(NewPassword)), Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = "";
    }
}
