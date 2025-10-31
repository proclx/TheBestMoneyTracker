using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;

namespace MoneyRules.Application.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private const string PRIVATBANK_API_URL = "https://api.privatbank.ua/p24api/pubinfo?exchange&coursid=5";

        public CurrencyService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<IEnumerable<ExchangeRate>> GetCurrentExchangeRatesAsync()
        {
            try
            {
                Console.WriteLine($"Fetching exchange rates from {PRIVATBANK_API_URL}");
                var response = await _httpClient.GetStringAsync(PRIVATBANK_API_URL);
                
                Console.WriteLine($"Received response: {response}");
                
                var rates = JsonSerializer.Deserialize<List<ExchangeRate>>(response);
                if (rates == null || !rates.Any())
                {
                    throw new Exception("No exchange rates received from the API");
                }

                foreach (var rate in rates)
                {
                    Console.WriteLine($"Parsed rate: {rate.Currency}/{rate.BaseCurrency} - Buy: {rate.BuyRate}, Sell: {rate.SaleRate}");
                }

                return rates;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error fetching exchange rates: {ex.Message}");
                throw new Exception("Failed to connect to PrivatBank API", ex);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing exchange rates: {ex.Message}");
                throw new Exception("Failed to parse exchange rates from API", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error fetching exchange rates: {ex.Message}");
                throw;
            }
        }
    }
}