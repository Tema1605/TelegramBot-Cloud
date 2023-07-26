using System;
using System.Threading.Tasks;
using TelegramBot_Cloud.View;
using TelegramBot_Cloud.ViewModel;

namespace TelegramBot_Cloud.Model
{
    internal class SubscriptionsProcessing
    {
        private DataProcessing _dataProcessing { get; set; }
        private PaymentVM _paymentVM { get; set; }
        private ButtonHandler _buttonHandler {  get; set; } 
        private CloudProcessing _cloudProcessing { get; set; }
        public CloudVM _cloudVM { get; set; }
        public SubscriptionsProcessing(DataProcessing dataProcessing, PaymentVM paymentVM, ButtonHandler buttonHandler, CloudProcessing cloudProcessing)
        {
            _dataProcessing = dataProcessing;   
            _paymentVM = paymentVM;
            _buttonHandler = buttonHandler;
            _cloudProcessing = cloudProcessing;
        }

        public async Task<string> GetSubscriptionUserLevelAsync(long userId)
        {
            return await _dataProcessing.GetUserSubscribe(userId);
        }
        public double GetSubscriptionMemoryLimit(string subName)
        {
            var limitBytes = SubscribeLevelsRepository.GetMemorySubscribe(subName);
            var memoryLimit = Converter.BytesToMegabytes(limitBytes);
            return memoryLimit;
        }
        public async Task<dynamic> GetExpirationSubscriptionDateAsync(long userId, bool convertToString = true)
        {
            var result = await _dataProcessing.GetExpirationDateSubscribe(userId);
            DateTime date = (DateTime)result;
            if (convertToString)
                return date.ToString("D");
            else
                return date;
        }
        public async Task RegistrationSubscriptionAsync(long userId, string subName)
        {
            var subActive = await GetSubscriptionUserLevelAsync(userId);
            var limitSubActive = GetSubscriptionMemoryLimit(subActive);
            var limitSubCreate = GetSubscriptionMemoryLimit(subName);

            if (limitSubActive >= limitSubCreate)
            {
                await MessageHandler.SendMessageUser(userId, $"У вас уже оформлена подписка {subActive}");
                return;
            }

            var result = await _paymentVM.CreatePaymanRequest(userId, subName);

            if (result)
                await MessageHandler.SendMessageUser(userId, $"✅ Подписка {subName} успешно оформлена");
            else
                await MessageHandler.SendMessageUser(userId, $"❌ Произошла ошибка во время оплаты");
        }
        public async Task SubscriptionRenewalAsync(long userId)
        {
            var subName = await GetSubscriptionUserLevelAsync(userId);
            var isRelevance = await CheckRelevanceSubscribe(userId);

            if(!isRelevance)            
                await UnsubscribeProcedureAsync(userId);
            else
                await _buttonHandler.SubscriptionRenewalButtonsMenu(userId, subName);
        }
        public async Task UnsubscribeProcedureAsync(long userId)
        {
            await _dataProcessing.UpdateUserSubscribe(userId, "Standart");

            var path = $"{CloudVM._globalFilePath}\\{userId}";
            var limit = GetSubscriptionMemoryLimit("Standart");
            var occupiedUserSpace = _cloudProcessing.CalculateFolderWeight(path);
            if (occupiedUserSpace > limit)
            {
                //Процедура удаления файлов
                await _cloudVM.DeleteUserFile(userId, limit);
            }     
        }
        public async Task<bool> CheckRelevanceSubscribe(long userId)
        {
            var dateSub = await GetExpirationSubscriptionDateAsync(userId, false);
            if ((DateTime)dateSub > DateTime.Now.AddDays(3)) return false;
            return true;
        }
        public async Task CheckingUserSubscriptionsAsync()
        {
            var listUsers = await _dataProcessing.GetExpiredSubscriptions();
            if (listUsers != null && listUsers.Count != 0)
            {
                foreach (var user in listUsers)
                {
                    await SubscriptionRenewalAsync(user);
                }
            }
            await Task.Delay(TimeSpan.FromDays(1));
            await CheckingUserSubscriptionsAsync();
        }
    }
}
