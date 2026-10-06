using System.ComponentModel.DataAnnotations;
using DohFlo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DohFlo.Tests
{
    public class CategoryFormViewModelTests
    {
        [Theory]
        [InlineData("Budget")]
        [InlineData("Debit")]
        [InlineData("")]
        public void InvalidCategoryType_HasTypeValidationError(string catType)
        {
            // Arrange
            var model = new CategoryFormViewModel
            {
                Name = "Test Category",
                CatType = catType
            };

            // Act
            var results = Validate(model);

            // Assert
            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(model.CatType)));
        }

        private static List<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);

            Validator.TryValidateObject(
                model, context, results, validateAllProperties: true);

            return results;
        }
    }
}
