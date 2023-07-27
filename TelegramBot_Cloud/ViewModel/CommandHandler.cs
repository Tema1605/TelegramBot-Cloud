using System;
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
        /// <summary>
        /// Обработчик команд.
        /// </summary>
        /// <param name="update"></param>
        public async Task CommandHandlerAsync(Update update)
        {
            await _cloudVM.CheckIfUserExists(update); // Проверка наличия данных о пользователе

            string command = update.Message.Text; // Команда
            long userId = update.Message.Chat.Id; // Id Пользователя

            //Обработка команды
            switch (command.ToLower().Trim())
            {
                case "/start":
                    await _buttonHandler.StartBotMessage(userId);
                    break;
                case "/menu":
                    await _buttonHandler.SendingButtonMenuAsync(userId);
                    break;
                case "/pay":
                    await _buttonHandler.SendingDonationMenuAsync(userId);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Обработчик Callback.
        /// </summary>
        /// <param name="callbackQuery"></param>
        public async Task CallbackHandlerAsync(CallbackQuery callbackQuery)
        {          
            if (callbackQuery.Id != null)
            {
                var action = callbackQuery.Data; // Действие
                var userId = callbackQuery.Message.Chat.Id; // Id Пользователя
                var messageId = callbackQuery.Message.MessageId; // Id Сообщения

                await BotVM.BotClient.AnswerCallbackQueryAsync(callbackQuery.Id); // Обратный ответ

                try
                {
                    if (action.Contains("GetFile_")) // Проверка на получение файла
                    {
                        await _cloudVM.GetUserFileAsync(userId, action.Replace("GetFile_", ""));
                        return;
                    }
                    else if (action.Contains("DelFile_")) // Проверка на удаление файла
                    {
                        await _cloudVM.DeleteUserFileAsync(userId, action.Replace("DelFile_", ""));
                        return;
                    }
                    else if (action.Contains("RSUB_")) // Проверка на продление подписки
                    {
                        await MessageHandler.RemoveMessageAsync(userId, messageId);

                        var _action = action.Replace("RSUB_", "");

                        if (_action == "NotRenewal")
                        {
                            await _subscriptionsProcessing.UnsubscribeProcedureAsync(userId);
                        }
                        else
                        {
                            await _subscriptionsProcessing.RegistrationSubscriptionAsync(userId, _action, false);
                        }
                        return;
                    }
                    else if (action.Contains("SUB_")) // Проверка на оформление подписки
                    {
                        await _subscriptionsProcessing.RegistrationSubscriptionAsync(userId, action.Replace("SUB_", ""));
                        return;
                    }


                    switch (action)
                    {
                        case "ProfileInfo":
                            await MessageHandler.EditKeyboardMessageAsync(userId, messageId, $"{await _cloudVM.UserInfoAsync(userId)}", null);
                            break;
                        case "GetFile":
                            await _cloudVM.UserFilesAsync(callbackQuery, messageId, FileActions.Action.GetFile);
                            break;
                        case "DeleteFile":
                            await _cloudVM.UserFilesAsync(callbackQuery, messageId, FileActions.Action.DeleteFile);
                            break;
                        case "Subscribe":
                            await _buttonHandler.SendingDonationMenuAsync(userId, messageId);
                            break;

                        default: break;
                    }
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.Message);
                }
            }
            
        }
        #endregion Public_Methods
    }
}
