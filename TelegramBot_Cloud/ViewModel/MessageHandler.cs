using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot_Cloud.ViewModel
{
    internal class MessageHandler
    {
        #region Private_Fields
        private CloudVM _cloudVM { get; set; }
        private CommandHandler _commandHandler { get; set; }
        #endregion Private_Fields

        #region Constructor
        public MessageHandler(CommandHandler commandHandler, CloudVM cloudVM)
        {
            _commandHandler = commandHandler;
            _cloudVM = cloudVM;
        }
        #endregion Constructor

        #region Public_Methods
        /// <summary>
        /// Получение формата сообщения от пользователя.
        /// </summary>
        /// <param name="update"></param>
        public async Task GetMessageFormatAsync(Update update)
        {
            try
            {
                if (BotVM.BotClient != null && update != null)
                {
                    var userId = update.Message.Chat.Id;

                    if (CheckTextMessage(update)) // Проверка на текстовое сообщение
                        await _commandHandler.CommandHandlerAsync(update);
                    if (CheckPhotoMessage(update)) // Проверка на фотографию
                        await SendMessageUserAsync(userId, "Чтобы сохранить изображение - нужно отправить документом");
                    if (CheckDocumentMessage(update)) // Проверка на документ
                        await _cloudVM.SavingProcedureAsync(update);
                }
                else return;
            }
            catch(Exception ex)
            {
                await Console.Out.WriteLineAsync($"Ошибка при получении сообщения пользователя: {ex.Message}");
            }            
        }

        /// <summary>
        /// Отправка текстового сообщения пользователю.
        /// </summary>
        /// <param name="userId">Id Пользователя</param>
        /// <param name="textMessage">Текст сообщения</param>
        public static async Task SendMessageUserAsync(long userId, string message)
        {
            await BotVM.BotClient.SendTextMessageAsync(userId, message);
        }

        /// <summary>
        /// Отправка текстового сообщения с кнопочной формой пользователю.
        /// </summary>
        /// <param name="userId">Id Пользователя</param>
        /// <param name="message">Текст сообщения</param>
        /// <param name="buttons">Кнопочная форма</param>
        public static async Task SendMessageUserAsync(long userId, string message, InlineKeyboardMarkup buttons)
        {
            await BotVM.BotClient.SendTextMessageAsync(userId, message, replyMarkup: buttons);
        }

        /// <summary>
        /// Удаление сообщения.
        /// </summary>
        /// <param name="userId">Id Пользователя</param>
        /// <param name="messageId">Id Сообщения</param>
        public static async Task RemoveMessageAsync(long userId, int messageId)
        {
            await BotVM.BotClient.DeleteMessageAsync(userId, messageId);
        }

        /// <summary>
        /// Изменение сообщения.
        /// </summary>
        /// <param name="userId">Id Пользователя</param>
        /// <param name="messageId">Id Сообщения</param>
        /// <param name="message">Текст сообщения</param>
        /// <param name="replyMarkup">Кнопочная форма</param>
        public static async Task EditKeyboardMessageAsync(long userId, int messageId, string message, InlineKeyboardMarkup replyMarkup)
        {
            await BotVM.BotClient.EditMessageTextAsync(userId, messageId, message, replyMarkup: replyMarkup);
        }
        #endregion Public_Methods

        #region Check_Func
        /// <summary>
        /// Проверка формата сообщения(Фото)
        /// </summary>
        private Func<Update, bool> CheckPhotoMessage = update => update.Message.Type == MessageType.Photo;
        /// <summary>
        /// Проверка формата сообщения(Документ)
        /// </summary>
        private Func<Update, bool> CheckDocumentMessage = update => update.Message.Type == MessageType.Document;
        /// <summary>
        /// Проверка формата сообщения(Текст)
        /// </summary>
        private Func<Update, bool> CheckTextMessage = update => update.Message.Type == MessageType.Text;
        #endregion Check_Func
    }
}
