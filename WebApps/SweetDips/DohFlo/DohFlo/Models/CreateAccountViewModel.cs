using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DohFlo.Models
{
    public class CreateAccountViewModel
    {
        [Required(ErrorMessage = "Please enter an account name")]
        [StringLength(120)]
        [Display(Name = "Account Name")]
        public string Name { get; set; } = "";

        [RegularExpression("^(Checking|Savings|Credit Card|Cash|Investment|Loan|Other)$", ErrorMessage = "Please select a valid account type")]
        [DisplayName("Account Type")]
        public string Type { get; set; } = "";

        [StringLength(200)]
        public string? Institution { get; set; }

        [Required]
        [RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "Use a three-letter currency code, such as USD")]
        [DisplayName("CurrencyCode")]
        public string CurrencyCode { get; set; } = "USD";
    }
}
