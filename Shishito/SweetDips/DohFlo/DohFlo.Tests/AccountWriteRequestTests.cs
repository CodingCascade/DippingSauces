using System.ComponentModel.DataAnnotations;
using DohFlo.Models.Api;

namespace DohFlo.Tests
{
    public class AccountWriteRequestTests
    {
        [Fact]
        public void InvalidFields_HaveValidationErrors()
        {
            var request = new AccountWriteRequest
            {
                Name = "",
                Type = "Banana",
                CurrencyCode = "US"
            };

            var results = Validate(request);

            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(request.Name)));
            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(request.Type)));
            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(request.CurrencyCode)));
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
