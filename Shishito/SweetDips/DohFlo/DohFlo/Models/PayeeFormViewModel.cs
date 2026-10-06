using System.ComponentModel.DataAnnotations;

namespace DohFlo.Models
{
    public class PayeeFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a payee name.")]
        [StringLength(160)]
        [Display(Name = "Payee Name")]
        public string Name { get; set; } = "";
    }
}
