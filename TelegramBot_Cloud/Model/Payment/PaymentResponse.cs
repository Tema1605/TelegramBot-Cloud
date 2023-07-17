using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TelegramBot_Cloud.Model.Payment
{
    public class PaymentResponse
    {
        [JsonProperty("ok")]
        public bool IsSuccessful { get; set; }

        [JsonProperty("result")]
        public PaymentResult Result { get; set; }
    }
}
