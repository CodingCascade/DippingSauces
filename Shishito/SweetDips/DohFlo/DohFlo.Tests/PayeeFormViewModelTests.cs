using System.ComponentModel.DataAnnotations;
using DohFlo.Models;
using Microsoft.AspNetCore.Mvc;

namespace DohFlo.Tests
{
    public class PayeeFormViewModelTests
    {
        [Fact]
        public void MissingName_HasNameValidationError()
        {
            // Arrange
            var model = new PayeeFormViewModel { Name = "" };

            // Act
            var results = Validate(model);

            // Asert
            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(model.Name)));
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
