using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using DohFlo.Data;

namespace DohFlo.Models
{
    public class CreateTransactionViewModel
    {
        // Fields user will fill
        [Range(1, int.MaxValue, ErrorMessage = "Please select an account.")]
        public int AccountId { get; set; }

        public int? PayeeId { get; set; }
        public int? CategoryId { get; set; }

        [Required]
        [Range(typeof(decimal), "0.01", "1000000.00", ErrorMessage = "The amount must be between 0.01 and 1,000,000.00.")]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [StringLength(1000)]
        public string? Notes { get; set; }

        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        [Required(ErrorMessage = "Please enter a currency code.")]
        [RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "Use a three-letter currency code, such as USD.")]
        public string CurrencyCode { get; set; } = "USD";


        // Dropdown data
        public List<SelectListItem> Accounts { get; set; } = new();
        public List<SelectListItem> Payees { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();

    }
}
