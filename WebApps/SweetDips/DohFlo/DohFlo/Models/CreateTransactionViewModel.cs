using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DohFlo.Models
{
    public class CreateTransactionViewModel
    {
        // Fields user will fill
        [Required]
        public int AccountId { get; set; }
        public int? PayeeId { get; set; }
        public int? CategoryId { get; set; }

        [Required, Range(typeof(decimal), "0.01", "1000000.00", ErrorMessage = "Amount must be between 0.01 and 1,000,000.00!")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Today;

        public string? Notes { get; set; }
        public bool IsPending { get; set; }

        public string CurrencyCode { get; set; } = "USD";

        // Dropdown data
        public List<SelectListItem> Accounts { get; set; } = new();
        public List<SelectListItem> Payees { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();

    }
}
