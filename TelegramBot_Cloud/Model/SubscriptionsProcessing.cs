using System;
using System.Threading.Tasks;
using TelegramBot_Cloud.View;
using TelegramBot_Cloud.ViewModel;

namespace TelegramBot_Cloud.Model
{
    internal class SubscriptionsProcessing
    {
        #region Private_Fields
        private DataProcessing _dataProcessing { get; set; }
        private PaymentVM _paymentVM { get; set; }
        private ButtonHandler _buttonHandler { get; set; }
        private CloudProcessing _cloudProcessing { get; set; }
        #endregion Private_Fields

        #region Public_Fields
        public CloudVM _cloudVM { get; set; }
        #endregion Public_Fields

        #region Constructor
        public SubscriptionsProcessing(DataProcessing dataProcessing, PaymentVM paymentVM, ButtonHandler buttonHandler, CloudProcessing cloudProcessing)
        {
            _dataProcessing = dataProcessing;
            _paymentVM = paymentVM;
            _buttonHandler = buttonHandler;
            _cloudProcessing = cloudProcessing;
        }
        #endregion Constructor

        #region Private_Methods
        /// <summary>
        /// Проверка актуальности подписки пользователя.
        /// </summary>
        /// <param name="userId">Id Пользователя</param>
        /// <returns>True, если подписка актуальна, False в противном случае</returns>
        private async Task<bool> CheckRelevanceSubscribe(long userId)
        {
            var dateSub = await GetExpirationSubscriptionDateAsync(userId, false); // Получение даты окончания текущей подписки

            if (dateSub != null)
            {
                if ((DateTime)dateSub > DateTime.Now.AddDays(3))
                    return false; // Если с момента окончания подписки прошло более 3 дней
                else
                    return true;
            }
            return false;

        }
        #endregion Private_Methods

        #region Public_Methods
        /// <summary>
        /// Получение текущей подписки пользователя.
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        /// <returns>Название подписки пользователя</returns>
        public async Task<string> GetSubscriptionUserLevelAsync(long userId)
        {
            return await _dataProcessing.GetUserSubscribeAsync(userId);
        }

        /// <summary>
        /// Получение лимита памяти подписки.
        /// </summary>
        /// <param name="subName">Название подписки.</param>
        /// <returns>Лимит памяти подписки в Мегабайтах</returns>
        public double GetSubscriptionMemoryLimit(string subName)
        {
            var limitBytes = SubscribeLevelsRepository.GetMemorySubscribe(subName);
            var memoryLimit = Converter.BytesToMegabytes(limitBytes);
            return memoryLimit;
        }

        /// <summary>
        /// Получение даты окончания подписки пользователя.
        /// </summary>
        /// <param name="userId">Id Пользователя</param>
        /// <param name="convertToString">Конвертировать DateTime в string</param>
        /// <returns>Дата окончания подписки</returns>
        public async Task<dynamic> GetExpirationSubscriptionDateAsync(long userId, bool convertToString = true)
        {
            var date = await _dataProcessing.GetExpirationDateSubscribeAsync(userId);

            if (date != DateTime.MinValue)
            {
                if (convertToString)
                    return date.ToString("D");
                else
                    return date;
            }
            return null;

        }

        /// <summary>
        /// Оформление подписки пользователя.
        /// </summary>
        /// <param name="userId">Id Пользователя</param>
        /// <param name="subName">Название подписки</param>
        /// <param name="relevance">Актуальность текущей подписки</param>
        public async Task RegistrationSubscriptionAsync(long userId, string subName, bool relevance = true)
        {
            var subActive = await GetSubscriptionUserLevelAsync(userId); // Получение активной подписки пользователя
            var limitSubActive = GetSubscriptionMemoryLimit(subActive); // Получение лимита памяти активной подписки
            var limitSubCreate = GetSubscriptionMemoryLimit(subName); // Получение лимита памяти оформляемой подписки

            if (limitSubActive >= limitSubCreate && relevance == true)
            {
                await MessageHandler.SendMessageUserAsync(userId, $"У вас уже оформлена подписка {subActive}");
                return;
            }

            var result = await _paymentVM.CreatePaymanRequest(userId, subName); // Создание платежа

            if (result)
                await MessageHandler.SendMessageUserAsync(userId, $"✅ Подписка {subName} успешно оформлена");
            else
                await MessageHandler.SendMessageUserAsync(userId, $"❌ Произошла ошибка во время оплаты");
        }

        /// <summary>
        /// Процедура продления подписки пользователя.
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        public async Task SubscriptionRenewalProcedureAsync(long userId)
        {
            var subName = await GetSubscriptionUserLevelAsync(userId); // Получение текущей подписки пользователя
            var isRelevance = await CheckRelevanceSubscribe(userId); //Получение актуальности подписки

            if (!isRelevance)
                await UnsubscribeProcedureAsync(userId); // Процедура отписки
            else
                await _buttonHandler.SubscriptionRenewalButtonsAsync(userId, subName);
        }

        /// <summary>
        /// Процедура отписки.
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        public async Task UnsubscribeProcedureAsync(long userId)
        {
            await _dataProcessing.UpdateUserSubscribeAsync(userId, "Standart"); // Обновление подписки пользователя на Стандартную

            var path = $"{CloudVM._globalFilePath}\\{userId}"; // Путь к папке пользователя
            var limit = GetSubscriptionMemoryLimit("Standart"); // Лимит памяти Стандратной подписки
            var occupiedUserSpace = _cloudProcessing.CalculateFolderWeight(path); //Получение занимаемого пространства пользователем

            if (occupiedUserSpace > limit) // Если занимаемое пространсво больше лимита памята подписки
            {                
                await _cloudVM.DeleteUserFileAsync(userId, limit); // Процедура удаления файлов
            }
        }

        /// <summary>
        /// Проверка актуальности подписки у пользователей.
        /// </summary>
        public async Task CheckingUserSubscriptionsAsync()
        {
            var listUsers = await _dataProcessing.GetExpiredSubscriptionsAsync(); // Получение списка пользователей у которых кончилась подписка

            if (listUsers != null && listUsers.Count != 0)
            {
                foreach (var user in listUsers)
                {
                    await SubscriptionRenewalProcedureAsync(user); // Процедура продления подписки
                }
            }
            await Task.Delay(TimeSpan.FromDays(1)); // Задержка 1 день
            await CheckingUserSubscriptionsAsync();
        }
        #endregion Public_Methods
    }
}
