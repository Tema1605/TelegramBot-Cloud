using System;
using System.Threading.Tasks;
using System.Xml;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot_Cloud.Model;
using TelegramBot_Cloud.View;

namespace TelegramBot_Cloud.ViewModel
{
    internal class CloudVM
    {
        #region Private_Fields
        private long _userId { get; set; }
        private string _fileName { get; set; }
        private string _fileId { get; set; }
        private CloudProcessing _cloudProcessing { get; set; }
        private DataProcessing _dataProcessing { get; set; }
        private ButtonHandler _buttonHandler { get; set; }
        private SubscriptionsProcessing _subscriptionsProcessing { get; set; }
        private long _limitMemoryInBytes { get; set; }
        public static readonly string _globalFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\TelegramBotCloud";
        #endregion Private_Fields

        #region Constructor
        public CloudVM(CloudProcessing cloudProcessing, ButtonHandler buttonHandler, DataProcessing dataProcessing, SubscriptionsProcessing subscriptionsProcessing)
        {
            _cloudProcessing = cloudProcessing;
            _buttonHandler = buttonHandler;
            _dataProcessing = dataProcessing;
            _subscriptionsProcessing = subscriptionsProcessing;
        }
        #endregion Constructor

        #region Private_Func
        private Func<double, double> ConverterBytesToMegabytes = number => number / Math.Pow(1024, 2);
        #endregion Private_Func

        #region Public_Methods
        public string RemainingMemory(long userId, double limitMemory)
        {
            var usedMemory = _cloudProcessing.CalculateFolderWeight($"{_globalFilePath}\\{userId}");
            var remainMemory = limitMemory - usedMemory;
            var title = $"☁️ Используется {Math.Round(usedMemory,2)}MB | {Math.Round(limitMemory,2)}MB\n" +
                $"☁️ Остаток {Math.Round(remainMemory,2)}MB";
            return title;
        }
        public async Task<string> UserInfo(long userId)
        {
            var userSubName = await _subscriptionsProcessing.GetSubscriptionUserLevelAsync(userId);
            var expirationDate = await _subscriptionsProcessing.GetExpirationSubscriptionDateAsync(userId);
            var limitMemory = _subscriptionsProcessing.GetSubscriptionMemoryLimit(userSubName);
            var remainingMemory = RemainingMemory(userId, limitMemory);

            var profileInfo = $"Подписка: {userSubName}\nАктивно до {expirationDate}\n\nОблачное хранилище\n{remainingMemory}";

            return profileInfo;
        }
        public async Task UserFiles(CallbackQuery callbackQuery, FileActions.Action action)
        {
            string list = string.Empty;
            var userId = callbackQuery.Message.Chat.Id;
            var fileList = _cloudProcessing.GetListUserFiles($"{_globalFilePath}\\{userId}");
            foreach (var el in fileList)
                list += $"{el}\n";

            var buttons = _buttonHandler.GenerateInlineKeyboardButtons(fileList, action);
            await BotVM.BotClient.SendTextMessageAsync(userId, "☁️ Выберите файл", replyMarkup: buttons);
        }
        public async Task GetUserFile(long userId, string fileName)
        {
            string path = $"{_globalFilePath}\\{userId}";
            try
            {
                await _cloudProcessing.GetFile(userId, path, fileName);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Ошибка при отправке файла пользователю: " + ex.Message);
            }

        }
        public async Task DeleteUserFile(long userId, string fileName)
        {
            string path = $"{_globalFilePath}\\{userId}\\{fileName}";

            try
            {
                if (await _cloudProcessing.DeleteFile(path)) await MessageHandler.SendMessageUser(_userId, "✅ Файл успешно удален");
                else await MessageHandler.SendMessageUser(_userId, "Ошибка при удалении файла");
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Ошибка при удалении файла пользователя: " + ex.Message);
            }
        }
        public async Task SavingProcedure(Update update)
        {
            await CheckIfUserExists(update);

            _userId = update.Message.Chat.Id;
            _fileId = update.Message.Document.FileId;
            _fileName = update.Message.Document.FileName;

            var userSub = await _dataProcessing.GetUserSubscribe(_userId);
            var limitMemory = Math.Round(_subscriptionsProcessing.GetSubscriptionMemoryLimit(userSub), 2);

            var file = BotVM.BotClient.GetFileAsync(_fileId).Result;
            string path = $"{_globalFilePath}\\{_userId}";
            var usedMemory = Math.Round(_cloudProcessing.CalculateFolderWeight(path),2);
            double fileWeight = Math.Round(Converter.BytesToMegabytes(update.Message.Document.FileSize.Value), 2);

            if (usedMemory+fileWeight <= limitMemory)
            {
                if (await _cloudProcessing.FileSaving(file, _fileName, path))
                {
                    await MessageHandler.SendMessageUser(_userId, $"✅ Файл успешно сохранен");
                    await _dataProcessing.SavingFileDataAsync(_userId, _fileName, Math.Round(ConverterBytesToMegabytes(fileWeight), 2));
                }
                else await MessageHandler.SendMessageUser(_userId, "Ошибка при сохранении файла");
            }
            else 
            { 
                await MessageHandler.SendMessageUser(
                _userId, 
                $"❌ Не хватает места\n" +
                $"☁️ Использовано памяти - {usedMemory}\n" +
                $"☁️ Вес файла - {fileWeight}\n" +
                $"☁️ Лимит - {limitMemory}");
            }
        }
        public async Task CheckIfUserExists(Update update)
        {
            var userId = update.Message.Chat.Id;
            var username = update.Message.Chat.Username;
            var firstName = update.Message.Chat.FirstName;

            if( !(await _dataProcessing.UserVerificationAsync(userId)) )
                await _dataProcessing.RegistrationUserAsync(userId, username, firstName);
        }
        #endregion Public_Methods
    }
}
