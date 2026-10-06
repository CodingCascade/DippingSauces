using System.ComponentModel.DataAnnotations;

namespace DohFlo.Tests
{
    internal static class ValidationTestHelper
    {
        internal static IReadOnlyList<ValidationResult> Validate(object model)
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
