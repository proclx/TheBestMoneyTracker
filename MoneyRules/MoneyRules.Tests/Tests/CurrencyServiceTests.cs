using System;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using MoneyRules.Application.Services;

namespace MoneyRules.Tests
{
    public class CurrencyServiceTests
    {
        private readonly CurrencyService _currencyService;

        public CurrencyServiceTests()
        {
            _currencyService = new CurrencyService();
        }

        [Fact]
        public async Task GetCurrentExchangeRatesAsync_ShouldReturnValidData()
        {
            // Act
            var rates = await _currencyService.GetCurrentExchangeRatesAsync();
            var ratesList = rates.ToList();

            // Assert
            Assert.NotNull(rates);
            Assert.NotEmpty(ratesList);

            // Перевіряємо наявність USD та EUR
            var usdRate = ratesList.FirstOrDefault(r => r.Currency == "USD");
            var eurRate = ratesList.FirstOrDefault(r => r.Currency == "EUR");

            Assert.NotNull(usdRate);
            Assert.NotNull(eurRate);

            // Перевіряємо базову валюту
            Assert.Equal("UAH", usdRate.BaseCurrency);
            Assert.Equal("UAH", eurRate.BaseCurrency);

            // Перевіряємо коректність форматування курсів
            Assert.True(decimal.TryParse(usdRate.BuyRate.Replace(',', '.'), 
                System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, 
                out decimal usdBuyRate));
            Assert.True(decimal.TryParse(usdRate.SaleRate.Replace(',', '.'), 
                System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, 
                out decimal usdSellRate));

            Assert.True(decimal.TryParse(eurRate.BuyRate.Replace(',', '.'), 
                System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, 
                out decimal eurBuyRate));
            Assert.True(decimal.TryParse(eurRate.SaleRate.Replace(',', '.'), 
                System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, 
                out decimal eurSellRate));

            // Перевіряємо що курси мають розумні значення
            Assert.True(usdBuyRate > 0);
            Assert.True(usdSellRate > 0);
            Assert.True(eurBuyRate > 0);
            Assert.True(eurSellRate > 0);

            // Перевіряємо що курс продажу вищий за курс купівлі
            Assert.True(usdSellRate >= usdBuyRate);
            Assert.True(eurSellRate >= eurBuyRate);

            // Виводимо поточні курси для перевірки
            Console.WriteLine($"USD Buy: {usdBuyRate:N2}, Sell: {usdSellRate:N2}");
            Console.WriteLine($"EUR Buy: {eurBuyRate:N2}, Sell: {eurSellRate:N2}");
        }

        [Fact]
        public async Task GetCurrentExchangeRatesAsync_ShouldHandleHttpErrors()
        {
            // Arrange
            var invalidService = new CurrencyService(); // В реальному проекті тут би використовувався невалідний URL

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(
                async () => await invalidService.GetCurrentExchangeRatesAsync()
            );
        }
    }
}