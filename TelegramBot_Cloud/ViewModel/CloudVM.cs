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
        private CloudProcessing _cloudProcessing { get; set; }
        private DataProcessing _dataProcessing { get; set; }
        private ButtonHandler _buttonHandler { get; set; }
        private SubscriptionsProcessing _subscriptionsProcessing { get; set; }        
        #endregion Private_Fields

        #region Public_Fileds
        public static readonly string _globalFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\TelegramBotCloud";
        #endregion Public_Fileds

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
        /// <summary>
        /// Отпрака информации о занимаемой, оставшейся памяти.
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        /// <param name="limitMemory">Лимит памяти в Мегабайтах</param>
        /// <returns>Информация о памяти пользователя</returns>
        private string RemainingMemory(long userId, double limitMemory)
        {
            var usedMemory = _cloudProcessing.CalculateFolderWeight($"{_globalFilePath}\\{userId}"); // Получение занимаемого пространсва пользователем
            var remainMemory = limitMemory - usedMemory; // Вычисление оставшейся памяти
            var title = $"☁️ Используется {Math.Round(usedMemory,2)}MB | {Math.Round(limitMemory,2)}MB\n" +
                $"☁️ Остаток {Math.Round(remainMemory,2)}MB";
            return title;
        }

        /// <summary>
        /// Информация о пользователе.
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        /// <returns>Информация о пользователе.</returns>
        public async Task<string> UserInfoAsync(long userId)
        {
            var userSubName = await _subscriptionsProcessing.GetSubscriptionUserLevelAsync(userId); // Получение текущей подписки
            var limitMemory = _subscriptionsProcessing.GetSubscriptionMemoryLimit(userSubName); // Получение лимита памяти подписки пользователя
            var remainingMemory = RemainingMemory(userId, limitMemory); // Получение информации о памяти пользователя в облаке

            if (userSubName != "Standart")
            {
                dynamic expirationDate;
                expirationDate = await _subscriptionsProcessing.GetExpirationSubscriptionDateAsync(userId); // Получение даты окончания подписки
                if(expirationDate != null)
                {
                    var profileInfo = $"Подписка: {userSubName}\nАктивно до {expirationDate}\n\nОблачное хранилище\n{remainingMemory}";
                    return profileInfo;
                }
                else
                {
                    var profileInfo = $"Облачное хранилище\n{remainingMemory}";
                    return profileInfo;
                }               
            }
            else
            {
                var profileInfo = $"Подписка: {userSubName}\n\nОблачное хранилище\n{remainingMemory}";
                return profileInfo;
            }

        }

        /// <summary>
        /// Отправка пользователю его файлов из облака.
        /// </summary>
        /// <param name="callbackQuery"></param>
        /// <param name="messageId">Id сообщения.</param>
        /// <param name="action">Действие над. файлом</param>
        public async Task UserFilesAsync(CallbackQuery callbackQuery, int messageId, FileActions.Action action)
        {
            var userId = callbackQuery.Message.Chat.Id; // Id пользователя
            var fileList = _cloudProcessing.GetListUserFiles($"{_globalFilePath}\\{userId}"); // Список файлов пользователя

            if (fileList != null)
            {
                var buttons = _buttonHandler.GenerateFileListButtons(fileList, action); // Создание динамических кнопок
                await MessageHandler.EditKeyboardMessageAsync(userId, messageId, "☁️ Выберите файл", buttons);
            }
            else
                await MessageHandler.SendMessageUserAsync(userId, "У вас нет файлов");
            
        }

        /// <summary>
        /// Отправка файла пользователю.
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        /// <param name="fileName">Имя файла.</param>
        public async Task GetUserFileAsync(long userId, string fileName)
        {
            string path = $"{_globalFilePath}\\{userId}"; //Путь у папке пользователя
            try
            {
                await _cloudProcessing.GetFile(userId, path, fileName); //Отправка файла пользователю
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Ошибка при отправке файла пользователю: " + ex.Message);
            }

        }

        /// <summary>
        /// Удаление файла пользователя.
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        /// <param name="fileName">Имя файла.</param>
        public async Task DeleteUserFileAsync(long userId, string fileName)
        {
            string path = $"{_globalFilePath}\\{userId}\\{fileName}"; // Путь к файлу

            try
            {
                if (await _cloudProcessing.DeleteFile(path)) 
                    await MessageHandler.SendMessageUserAsync(userId, "✅ Файл успешно удален");
                else 
                    await MessageHandler.SendMessageUserAsync(userId, "Ошибка при удалении файла");
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Ошибка при удалении файла пользователя: " + ex.Message);
            }
        }

        /// <summary>
        /// Удаление файлов пользователя до определенного лимита веса папки.
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        /// <param name="sizeLimitBytes">Лимит памяти папки пользователя.</param>
        public async Task DeleteUserFileAsync(long userId, double sizeLimitBytes)
        {
            string folderPath = $"{_globalFilePath}\\{userId}"; // Путь до папки пользователя

            var directoryInfo = new DirectoryInfo(folderPath);

            if (!directoryInfo.Exists)
            {
                Console.WriteLine("Папка не найдена.");
                return;
            }
            var directorySize = _cloudProcessing.CalculateFolderWeight(folderPath); // Получение веса папки
            var files = directoryInfo.GetFiles().OrderByDescending(f => f.Length); // Сотировка полученного списка файлов по убыванию

            await MessageHandler.SendMessageUserAsync(userId,"Некоторые файлы которые пришлось удалить");

            while (directorySize > sizeLimitBytes)
            {
                var largestFile = files.FirstOrDefault();

                if (largestFile == null)
                {
                    Console.WriteLine("Все файлы удалены или папка пуста.");
                    break;
                }
                await GetUserFileAsync(userId, largestFile.Name); // Отправка удаленного файла пользователю

                await _cloudProcessing.DeleteFile($"{folderPath}\\{largestFile.Name}"); // Удаление файла

                directorySize -= (double)largestFile.Length / 1048576;

                files = directoryInfo.GetFiles().OrderByDescending(f => f.Length);
            }
        }

        /// <summary>
        /// Процедура сохранения файла пользователя.
        /// </summary>
        /// <param name="update"></param>
        public async Task SavingProcedureAsync(Update update)
        {
            await CheckIfUserExists(update); // Проверка на существования или создание папки пользователя.

            var userId = update.Message.Chat.Id; // Id Пользователя
            var fileId = update.Message.Document.FileId; // Id Файла 
            var fileName = update.Message.Document.FileName; // Название файла

            var userSub = await _dataProcessing.GetUserSubscribeAsync(userId); // Получение подписки пользователя
            var limitMemory = Math.Round(_subscriptionsProcessing.GetSubscriptionMemoryLimit(userSub), 2); // Получение лимита памяти подписки

            var file = BotVM.BotClient.GetFileAsync(fileId).Result;
            string path = $"{_globalFilePath}\\{userId}"; // Путь к папке пользователя
            var usedMemory = Math.Round(_cloudProcessing.CalculateFolderWeight(path),2); // Получение занимаемого пространства
            double fileWeight = Math.Round(Converter.BytesToMegabytes(update.Message.Document.FileSize.Value), 2); // Вес файла

            if (usedMemory+fileWeight <= limitMemory) // Если хватает места
            {
                if (await _cloudProcessing.FileSaving(file, fileName, path))
                {
                    await MessageHandler.SendMessageUserAsync(userId, $"✅ Файл успешно сохранен");
                    await _dataProcessing.SavingFileDataAsync(userId, fileName, fileWeight);
                }
                else await MessageHandler.SendMessageUserAsync(userId, "Ошибка при сохранении файла");
            }
            else 
            { 
                await MessageHandler.SendMessageUserAsync(
                userId, 
                $"❌ Не хватает места\n" +
                $"☁️ Использовано памяти - {usedMemory}\n" +
                $"☁️ Вес файла - {fileWeight}\n" +
                $"☁️ Лимит - {limitMemory}");
            }
        }

        /// <summary>
        /// Проверка на наличие данных о пользователе.
        /// </summary>
        /// <param name="update"></param>
        public async Task CheckIfUserExists(Update update)
        {
            var userId = update.Message.Chat.Id; // Id Пользователя
            var username = update.Message.Chat.Username; // Имя пользователя
            var firstName = update.Message.Chat.FirstName; // Имя

            if( !(await _dataProcessing.UserVerificationAsync(userId)) )
                await _dataProcessing.RegistrationUserAsync(userId, username, firstName);
        }
        #endregion Public_Methods
    }
}
