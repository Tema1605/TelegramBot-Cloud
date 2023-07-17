using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TelegramBot_Cloud.Model.Payment
{
    public class PaymentResult
    {
        [JsonProperty("payment_message_id")]
        public int PaymentMessageId { get; set; }
    }
}
