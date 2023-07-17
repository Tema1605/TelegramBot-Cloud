using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TelegramBot_Cloud.Model.Payment
{
    public class Price
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }
    }
}
