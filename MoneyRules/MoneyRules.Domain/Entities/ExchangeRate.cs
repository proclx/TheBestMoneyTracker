using System.Text.Json.Serialization;

namespace MoneyRules.Domain.Entities
{
    public class ExchangeRate
    {
        [JsonPropertyName("ccy")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("base_ccy")]
        public string BaseCurrency { get; set; } = string.Empty;

        [JsonPropertyName("buy")]
        public string BuyRate { get; set; } = string.Empty;

        [JsonPropertyName("sale")]
        public string SaleRate { get; set; } = string.Empty;
    }
}