using DohFlo.Models;
using Microsoft.Identity.Client;

namespace DohFlo.Tests
{
    public class CreateTransactionViewModelTests
    {
        [Fact]
        public void ValidModel_HasNoValidationErrors()
        {
            var model = ValidModel();
            var results = ValidationTestHelper.Validate(model);

            Assert.Empty(results);
        }

        [Fact]
        public void MissingAccount_HasAccountValidationError()
        {
            var model = ValidModel();
            model.AccountId = 0;

            var results = ValidationTestHelper.Validate(model);

            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(model.AccountId)));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("1000000.01")]
        public void AmountOutsideAllowedRange_HasAmountValidationEror(string amountText)
        {
            var model = ValidModel();
            model.Amount = decimal.Parse(amountText);

            var results = ValidationTestHelper.Validate(model);

            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(model.Amount)));
        }

        [Theory]
        [InlineData("")]
        [InlineData("US")]
        [InlineData("USDD")]
        [InlineData("123")]
        public void InvalidCurrency_HasCurrencyValidationEror(string currency)
        {
            var model = ValidModel();
            model.CurrencyCode = currency;

            var results = ValidationTestHelper.Validate(model);

            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(model.CurrencyCode)));
        }

        [Fact]
        public void NotesLongerThan1000Characters_HasNotesValidationError()
        {
            var model = ValidModel();
            model.Notes = new string('N', 1001);

            var results = ValidationTestHelper.Validate(model);

            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(model.Notes)));
        }

        [Fact]
        public void NewModel_DefaultsDateToToday()
        {
            var model = new CreateTransactionViewModel();

            Assert.Equal(DateTime.Today, model.Date);
        }

        private static CreateTransactionViewModel ValidModel() => new()
        {
            AccountId = 1,
            Amount = 125.50m,
            Date = new DateTime(2026, 9, 22),
            CurrencyCode = "USD"
        };

    }
}
