using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TelegramBot_Cloud.Model;
using TelegramBot_Cloud.Model.Payment;

namespace TelegramBot_Cloud.ViewModel
{
    internal class PaymentVM
    {
        #region Private_Fields
        private DataProcessing _dataProcessing { get; set; }
        private readonly string _tokenBot;
        private readonly string _tokenPay;
        #endregion Private_Fields

        #region Constructor
        public PaymentVM(DataProcessing dataProcessing, string tokenBot, string tokenPay)
        {
            _dataProcessing = dataProcessing;
            _tokenBot = tokenBot;
            _tokenPay = tokenPay;
        }
        #endregion Constructor

        #region Private_Methods
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
        #endregion Private_Methods

        #region Public_Methods
        /// <summary>
        /// Создает платежный запрос.
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        /// <param name="subName">Название подписки</param>
        /// <returns>True, если платеж прошел успешно. False, в противном случае.</returns>
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
                await _dataProcessing.UpdateUserSubscribe(userId, subName); //Обновление подписки пользователя в БД
                return true;
            }
            else
                return false;
        }
        #endregion Public_Methods
    }
}
