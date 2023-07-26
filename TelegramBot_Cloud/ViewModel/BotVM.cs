using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot_Cloud.Model;
using TelegramBot_Cloud.View;

namespace TelegramBot_Cloud.ViewModel
{
    internal class BotVM
    {
        #region Private_Fields 
        private static readonly string _token = "6193581433:AAHwdLPmS_376F0uU8ASOlzCS_t8P2ewf70";
        private static readonly string _tokenPay = "401643678:TEST:5227cf38-5118-4e17-a695-0389534eb4a3";
        private DataProcessing _dataProcessing;
        private CommandHandler _commandHandler;
        private ButtonHandler _buttonHandler;
        private MessageHandler _messageHandler;
        private CloudProcessing _cloudProcessing;
        private PaymentVM _paymentVM;
        private CloudVM _cloudVM;
        private SubscriptionsProcessing _subscriptionsProcessing;
        #endregion Private_Fields

        #region Internal_Fields
        internal static TelegramBotClient BotClient;
        #endregion Internal_Fields

        #region Constructor
        public async Task<BotVM> InitializeAsync()
        {
            BotClient = new TelegramBotClient(_token) { Timeout = TimeSpan.FromSeconds(300) };
            BotClient.StartReceiving(Update, Error);

            _cloudProcessing = new CloudProcessing();
            _buttonHandler = new ButtonHandler();
            _dataProcessing = new DataProcessing();            
            _paymentVM = new PaymentVM(_dataProcessing, _token, _tokenPay);
            _subscriptionsProcessing = new SubscriptionsProcessing(_dataProcessing, _paymentVM, _buttonHandler, _cloudProcessing);
            _cloudVM = new CloudVM(_cloudProcessing, _buttonHandler, _dataProcessing, _subscriptionsProcessing);
            _commandHandler = new CommandHandler(_buttonHandler, _cloudVM, _subscriptionsProcessing);
            _messageHandler = new MessageHandler(_commandHandler, _cloudVM);

            _subscriptionsProcessing._cloudVM = _cloudVM;
            await _subscriptionsProcessing.CheckingUserSubscriptionsAsync();

            return this;
        }
        #endregion Constructor

        #region Private_Methods
        private async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            if (update.Message != null) 
                await _messageHandler.GetMessageFormat(update);

            else if (update.CallbackQuery != null)
                await _commandHandler.ProcessCallbackQuery(update.CallbackQuery.Data, update.CallbackQuery.Message.Chat.Id);
        }
        private async Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            await Console.Out.WriteLineAsync(arg2.Message);
            throw new NotImplementedException();
        }
        #endregion Private_Methods

    }
}
