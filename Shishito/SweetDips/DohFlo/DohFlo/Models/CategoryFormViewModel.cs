using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DohFlo.Models
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a category name.")]
        [StringLength(140)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Please select a category type.")]
        [RegularExpression("^(Expense|Income|Transfer)$",ErrorMessage = "Please select a valid category type.")]
        [Display(Name = "Category Type")]
        public string CatType { get; set; } = "Expense";

        [Display(Name = "Parent Category")]
        public int? ParentCategoryId { get; set; }

        public List<SelectListItem> ParentCategories { get; set; } = new();
    }
}
