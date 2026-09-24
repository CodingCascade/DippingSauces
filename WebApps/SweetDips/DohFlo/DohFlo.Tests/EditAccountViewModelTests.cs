using DohFlo.Models;

namespace DohFlo.Tests
{
    public class EditAccountViewModelTests
    {
        [Fact]
        public void ValidModel_HasNoValidationErrors()
        {
            var model = ValidModel();
            var results = ValidationTestHelper.Validate(model);

            Assert.Empty(results);
        }

        [Fact]
        public void MissingName_HasNameValidationError()
        {
            var model = ValidModel();
            model.Name = "";

            var results = ValidationTestHelper.Validate(model);

            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(model.Name)));
        }

        [Theory]
        [InlineData("")]
        [InlineData("Wallet")]
        [InlineData("Cryptocurrency")]
        public void InvalidType_HasTypeValidationError(string type)
        {
            var model = ValidModel();
            model.Type = type;

            var results = ValidationTestHelper.Validate(model);

            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(model.Type)));
        }

        [Theory]
        [InlineData("")]
        [InlineData("US")]
        [InlineData("USDD")]
        [InlineData("12$")]
        public void InvalidCurrency_HasCurrencyValidationError(string currency)
        {
            var model = ValidModel();
            model.CurrencyCode = currency;

            var results = ValidationTestHelper.Validate(model);

            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(model.CurrencyCode)));
        }

        [Fact]
        public void NameLongerThan120Characters_HasNameValidationError()
        {
            var model = ValidModel();
            model.Name = new string('A', 121);

            var results = ValidationTestHelper.Validate(model);

            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(model.Name)));
        }

        private static EditAccountViewModel ValidModel() => new()
        {
            // Return a basic instance. EditAccountViewModel has required properties,
            // set them here to produce a valid model for the test.
            Id = 1,
            Name = "Primary Checking",
            Type = "Checking",
            Institution = "Example Bank",
            CurrencyCode = "USD"
        };
    }
}
