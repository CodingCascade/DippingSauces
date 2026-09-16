using Microsoft.Identity.Client;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DohFlo.Models
{
    public class EditAccountViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter an account name.")]
        [StringLength(120)]
        [Display(Name = "Account Name")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Please select an account type.")]
        [RegularExpression("^(Checking|Savings|Credit Card|Cash|Investment|Loan|Other)$", ErrorMessage = "Please select a valid accout type.")]
        [Display(Name = "Account Type")]
        public string Type { get; set; } = "";

        [StringLength(200)]
        public string? Institution { get; set; }

        [Required(ErrorMessage = "Use a three-letter currency code, such as USD, EUR, GBP, and so on.")]
        [RegularExpression("^[a-zA-Z]{3}$", ErrorMessage = "Use a three-letter currency code, such as USD.")]
        [Display(Name= "Currency")]
        public string CurrencyCode { get; set; } = "USD";
    }
}
