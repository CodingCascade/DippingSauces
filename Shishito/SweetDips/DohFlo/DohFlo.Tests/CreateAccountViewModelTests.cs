using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DohFlo.Models;
using Microsoft.AspNetCore.Mvc;

namespace DohFlo.Tests
{
    public class CreateAccountViewModelTests
    {
        [Fact]
        public void ValidModel_HasNoValidationErrors()
        {
            // Arrange
            var model = new CreateAccountViewModel
            {
                Name = "Primary Checking",
                Type = "Checking",
                Institution = "Example Bank",
                CurrencyCode = "USD"
            };

            // Act
            var results = Validate(model);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void MissingName_HasNameValidationError()
        {
            // Arrange
            var model = new CreateAccountViewModel {
                Name = "",
                Type = "Checking",
                CurrencyCode = "USD",
            };

            // Act
            var results = Validate(model);

            // Assert
            Assert.Contains(results, result => result.MemberNames.Contains(nameof(model.Name)));
        }

        [Theory]
        [InlineData("Wallet")]
        [InlineData("Crytocurrency")]
        [InlineData("")]
        public void InvalidAccountType_HasTypeValidationError(string accountType)
        {
            // Arrange
            var model = new CreateAccountViewModel
            {
                Name = "Test Account",
                Type = accountType,
                CurrencyCode = "USD"
            };

            // Act
            var results = Validate(model);

            // Assert
            Assert.Contains(results, result => result.MemberNames.Contains(nameof(model.Type)));
        }

        private static List<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);

            Validator.TryValidateObject(
                model,
                context,
                results,
                validateAllProperties: true);

            return results;
        }
    }
}
