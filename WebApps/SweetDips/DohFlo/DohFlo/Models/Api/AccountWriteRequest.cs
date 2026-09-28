using System.ComponentModel.DataAnnotations;

namespace DohFlo.Models.Api
{
    public sealed class AccountWriteRequest
    {
        [Required(ErrorMessage = "Please enter an account name.")]
        [StringLength(120)]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Please select an account type.")]
        [RegularExpression("^(Checking|Savings|Credit Card|Cash|Investment|Loan|Other)$",
            ErrorMessage = "Please select a valid account type.")]
        public string Type { get; set; }

        [StringLength(200)]
        public string? Institution { get; set; }

        [Required]
        [RegularExpression("^[A-Za-z]{3}$",
            ErrorMessage = "Use a three-letter currency code, such as a USD.")]
        public string CurrencyCode { get; set; }
    }
}
