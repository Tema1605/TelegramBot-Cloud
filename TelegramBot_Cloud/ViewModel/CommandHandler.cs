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
        private SubscriptionsProcessing _subscriptionsProcessing { get; set; }
        #endregion Private_Fields

        #region Constructor
        internal CommandHandler(ButtonHandler buttonHandler, CloudVM cloudVM, SubscriptionsProcessing subscriptionsProcessing)
        {
            _buttonHandler = buttonHandler;
            _cloudVM = cloudVM;
            _subscriptionsProcessing = subscriptionsProcessing;

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
                    break;
                case "/menu":
                    await _buttonHandler.ShowMenu(userId);
                    break;
                case "/profileinfo":
                    await MessageHandler.SendMessageUser(userId, $"{await _cloudVM.UserInfo(userId)}");
                    break;
                case "/pay":
                    await _buttonHandler.DonationMenu(userId);
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

            await HandlerCallbackAsync(callbackQuery);
        }
        public async Task HandlerCallbackAsync(CallbackQuery callbackQuery)
        {
            var action = callbackQuery.Data;
            var userId = callbackQuery.Message.Chat.Id;

            if (action.Contains("GetFile_"))
            {
                await _cloudVM.GetUserFile(userId, action.Replace("GetFile_", ""));
                return;
            }
            else if (action.Contains("DelFile_"))
            {
                await _cloudVM.DeleteUserFile(userId, action.Replace("DelFile_", ""));
                return;                
            }
            else if (action.Contains("RSUB_"))
            {
                var _action = action.Replace("RSUB_", "");
                if (_action == "NotRenewal")
                {
                    await _subscriptionsProcessing.UnsubscribeProcedureAsync(userId);
                }
                else
                {
                    await _subscriptionsProcessing.RegistrationSubscriptionAsync(userId, _action);
                }
                return;
            }
            else if (action.Contains("SUB_"))
            {
                await _subscriptionsProcessing.RegistrationSubscriptionAsync(userId, action.Replace("SUB_", ""));
                return;
            }
            

            switch (action)
            {
                case "ProfileInfo":
                    await MessageHandler.SendMessageUser(callbackQuery.Message.Chat.Id, $"{await _cloudVM.UserInfo(callbackQuery.Message.Chat.Id)}");
                    break;
                case "GetFile":
                    await _cloudVM.UserFiles(callbackQuery, FileActions.Action.GetFile);
                    break;
                case "DeleteFile":
                    await _cloudVM.UserFiles(callbackQuery, FileActions.Action.DeleteFile);
                    break;
                case "Subscribe":
                    await _buttonHandler.DonationMenu(callbackQuery.Message.Chat.Id);
                    break;

                default : break;
            }
            if (callbackQuery.Id != null)
                await BotVM.BotClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }
        #endregion Public_Methods
    }
}
