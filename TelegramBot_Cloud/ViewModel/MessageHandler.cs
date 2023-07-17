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
        public async Task GetMessageFormat(Update update)
        {
            try
            {
                if (BotVM.BotClient != null && update != null)
                {
                    var userId = update.Message.Chat.Id;

                    if (CheckTextMessage(update))
                        await _commandHandler.ProcessCommand(update);
                    if (CheckPhotoMessage(update))
                        await SendMessageUser(userId, "Чтобы сохранить изображение - нужно отправить документом");
                    if (CheckDocumentMessage(update))
                        await _cloudVM.SavingProcedure(update);
                }
                else return;
            }
            catch(Exception ex)
            {
                await Console.Out.WriteLineAsync($"Ошибка при получении сообщения пользователя: {ex.Message}");
            }            
        }
        public static async Task SendMessageUser(long userId, string textMessage)
        {
            await BotVM.BotClient.SendTextMessageAsync(userId, textMessage);
        }
        public static async Task SendMessageUser(long userId, string message, InlineKeyboardMarkup buttons)
        {
            await BotVM.BotClient.SendTextMessageAsync(userId, message, replyMarkup: buttons);
        }
        #endregion Public_Methods

        #region Check_Func
        private Func<Update, bool> CheckPhotoMessage = update => update.Message.Type == MessageType.Photo;
        private Func<Update, bool> CheckDocumentMessage = update => update.Message.Type == MessageType.Document;
        private Func<Update, bool> CheckTextMessage = update => update.Message.Type == MessageType.Text;
        #endregion Check_Func
    }
}
