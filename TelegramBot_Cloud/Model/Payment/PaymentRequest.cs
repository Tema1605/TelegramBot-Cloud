using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TelegramBot_Cloud.Model.Payment
{
    public class PaymentRequest
    {
        [JsonProperty("chat_id")]
        public long ChatId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("provider_token")]
        public string ProviderToken { get; set; }

        [JsonProperty("start_parameter")]
        public string StartParameter { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("prices")]
        public Price[] Prices { get; set; }
    }
}
