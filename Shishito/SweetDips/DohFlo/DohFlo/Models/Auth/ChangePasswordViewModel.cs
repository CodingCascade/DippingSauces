using System.ComponentModel.DataAnnotations;

namespace DohFlo.Models.Auth
{
    public sealed class ChangePasswordViewModel
    {
        [Required, DataType(DataType.Password), Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = "";

        [Required, DataType(DataType.Password), Display(Name = "New Password")]
        public string NewPassword { get; set; } = "";

        [Required, DataType(DataType.Password), Compare(nameof(NewPassword)), Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = "";
    }
}
