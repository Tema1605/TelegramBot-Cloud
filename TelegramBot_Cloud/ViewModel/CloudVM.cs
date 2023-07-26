using System;
using System.IO;
using System.Linq;
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
            var limitMemory = _subscriptionsProcessing.GetSubscriptionMemoryLimit(userSubName);
            var remainingMemory = RemainingMemory(userId, limitMemory);

            if (userSubName != "Standart")
            {
                dynamic expirationDate;
                expirationDate = await _subscriptionsProcessing.GetExpirationSubscriptionDateAsync(userId);

                var profileInfo = $"Подписка: {userSubName}\nАктивно до {expirationDate}\n\nОблачное хранилище\n{remainingMemory}";
                return profileInfo;
            }
            else
            {
                var profileInfo = $"Подписка: {userSubName}\n\nОблачное хранилище\n{remainingMemory}";
                return profileInfo;
            }

        }
        public async Task UserFiles(CallbackQuery callbackQuery, FileActions.Action action)
        {
            var userId = callbackQuery.Message.Chat.Id;
            var fileList = _cloudProcessing.GetListUserFiles($"{_globalFilePath}\\{userId}");

            if (fileList != null)
            {
                var buttons = _buttonHandler.GenerateInlineKeyboardButtons(fileList, action);
                await BotVM.BotClient.SendTextMessageAsync(userId, "☁️ Выберите файл", replyMarkup: buttons);
            }
            else
                await MessageHandler.SendMessageUser(userId, "У вас нет файлов");
            
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
                if (await _cloudProcessing.DeleteFile(path)) 
                    await MessageHandler.SendMessageUser(userId, "✅ Файл успешно удален");
                else 
                    await MessageHandler.SendMessageUser(userId, "Ошибка при удалении файла");
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Ошибка при удалении файла пользователя: " + ex.Message);
            }
        }

        public async Task DeleteUserFile(long userId, double sizeLimitBytes)
        {
            string folderPath = $"{_globalFilePath}\\{userId}";

            var directoryInfo = new DirectoryInfo(folderPath);

            if (!directoryInfo.Exists)
            {
                Console.WriteLine("Папка не найдена.");
                return;
            }
            var directorySize = _cloudProcessing.CalculateFolderWeight(folderPath);
            var files = directoryInfo.GetFiles().OrderByDescending(f => f.Length);
            await MessageHandler.SendMessageUser(userId,"Некоторые файлы которые пришлось удалить");
            while (directorySize > sizeLimitBytes)
            {
                var largestFile = files.FirstOrDefault();

                if (largestFile == null)
                {
                    Console.WriteLine("Все файлы удалены или папка пуста.");
                    break;
                }
                await GetUserFile(userId, largestFile.Name);
                await _cloudProcessing.DeleteFile($"{folderPath}\\{largestFile.Name}");
                Console.WriteLine($"Удален файл: {largestFile.Name}");
                directorySize -= (double)largestFile.Length / 1048576;

                files = directoryInfo.GetFiles().OrderByDescending(f => f.Length);
            }

            Console.WriteLine("Удаление файлов завершено.");
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
