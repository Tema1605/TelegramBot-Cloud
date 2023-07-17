using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TelegramBot_Cloud.Model.Payment;
using TelegramBot_Cloud.Model;

namespace TelegramBot_Cloud.ViewModel
{
    internal class PaymentVM
    {
        private DataProcessing _dataProcessing {  get; set; }
        private readonly string _tokenBot;
        private readonly string _tokenPay;
        public PaymentVM(DataProcessing dataProcessing, string tokenBot, string tokenPay)
        {
            _dataProcessing = dataProcessing;
            _tokenBot = tokenBot;
            _tokenPay = tokenPay;
        }
        private async Task<bool> Payment(PaymentRequest paymentRequest)
        {
            // Преобразуем платежный запрос в JSON
            var json = JsonConvert.SerializeObject(paymentRequest);

            // Отправляем запрос на оплату через Telegram API
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"https://api.telegram.org/bot{_tokenBot}/sendInvoice",
                    new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    // Распаковка и обработка ответа
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var paymentResponse = JsonConvert.DeserializeObject<PaymentResponse>(responseContent);

                    if (paymentResponse.IsSuccessful)
                    {
                        Console.WriteLine("Платеж прошел успешно\n" + 
                            "Payment Message ID: " + 
                            paymentResponse.Result.PaymentMessageId);
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Оплата не прошла");
                        return false;
                    }
                }
                else return false;
            }
        }

        public async Task<bool> CreatePaymanRequest(long userId, string subName)
        {
            int amount = 0;
            switch (subName)
            {
                case "Basic":
                    amount = 25;
                    break;
                case "Premium":
                    amount = 50;
                    break;
                case "Professional":
                    amount = 100;
                    break;

                default: break;
            }

            // Создаем платежный запрос
            var paymentRequest = new PaymentRequest
            {
                ChatId = userId,
                Title = "🟢 Платёж создан. Ожидаем оплаты.",
                Description = $"Подписка {subName}",
                Payload = "custom_payload",
                ProviderToken = _tokenPay,
                StartParameter = "start_parameter",
                Currency = "USD",
                Prices = new[] { new Price { Label = "Test Item", Amount = amount } }
            };

            if (await Payment(paymentRequest))
            {
                await _dataProcessing.UpdateUserSubscribe(userId, subName);                
                return true;
            }
            else
                return false;
        }
    }
}
