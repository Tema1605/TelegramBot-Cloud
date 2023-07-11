using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot_Cloud.Model;
using TelegramBot_Cloud.View;

namespace TelegramBot_Cloud.ViewModel
{
    internal class CommandHandler
    {
        #region Private_Fields
        private ButtonHandler _buttonHandler { get; set; }
        private CloudVM _cloudVM { get; set; }
        #endregion Private_Fields

        #region Constructor
        internal CommandHandler(ButtonHandler buttonHandler, CloudVM cloudVM)
        {
            _buttonHandler = buttonHandler;
            _cloudVM = cloudVM;
        }
        #endregion Constructor

        #region Public_Methods
        public async Task ProcessCommand(Update update)
        {
            string command = update.Message.Text;
            long userId = update.Message.Chat.Id;
            switch (command.ToLower().Trim())
            {
                case "/start":
                    await _buttonHandler.Greeting(userId);
                    _cloudVM.CheckIfUserExists(update);
                    break;
                case "/menu":
                    await _buttonHandler.ShowMenu(userId);
                    break;
                case "/profileinfo":
                    await MessageHandler.SendMessageUser(userId, $"{_cloudVM.RemainingMemory(userId)}MB");
                    break;
                default:
                    break;
            }
        }
        public async Task ProcessCallbackQuery(string callbackId, long chatId)
        {
            var callbackQuery = new CallbackQuery
            {
                Message = new Message
                {
                    Chat = new Chat
                    {
                        Id = chatId
                    }
                },
                Data = callbackId
            };

            await HandleCallback(callbackQuery);
        }
        public async Task HandleCallback(CallbackQuery callbackQuery)
        {
            var action = callbackQuery.Data;

            if (action.Contains("GetFile_"))
            {
                await _cloudVM.GetUserFile(callbackQuery.Message.Chat.Id, action.Replace("GetFile_", ""));
                return;
            }
            else if (action.Contains("DelFile_"))
            {
                await _cloudVM.DeleteUserFile(callbackQuery.Message.Chat.Id, action.Replace("DelFile_", ""));
                return;                
            }

            switch (action)
            {
                case "ProfileInfo":
                    await MessageHandler.SendMessageUser(callbackQuery.Message.Chat.Id, $"{_cloudVM.RemainingMemory(callbackQuery.Message.Chat.Id)}MB");
                    break;
                case "GetFile":
                    await _cloudVM.UserFiles(callbackQuery, FileActions.Action.GetFile);
                    break;
                case "DeleteFile":
                    await _cloudVM.UserFiles(callbackQuery, FileActions.Action.DeleteFile);
                    break;
                default:
                    break;
            }
            if (callbackQuery.Id != null)
                await BotVM.BotClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
        #endregion Public_Methods
    }
}
