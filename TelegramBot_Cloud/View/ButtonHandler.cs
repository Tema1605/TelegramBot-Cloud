using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot_Cloud.ViewModel;

namespace TelegramBot_Cloud.View
{
    internal class ButtonHandler
    {
        #region Private_Fields
        private const string _deleteFile= "DelFile_";
        private const string _getFile= "GetFile_";
        private readonly string _greetingMessage =
            "Приветствую вас!\nЯ - ваш личный помощник и облачный хранитель файлов. " +
            "Я здесь, чтобы облегчить вам жизнь, предоставляя доступ к вашим файлам где бы вы ни находились. " +
            "Я могу помочь вам организовать и сохранить важные документы, фотографии, видео и многое другое. " +
            "Просто отправьте мне файл, и я надежно сохрани его для вас.\n\n Список команд:\n" +
            "/start - Перезапуск бота\n" +
            "/menu - Меню\n" +
            "/pay - Купить подписку";
        private readonly string _donateInfo =
            "ℹ️ Информация о ежемесячной подписке ℹ️\n\n" +
            "💼 Basic\n" +
            "16MB ☁️\n\n" +
            "🌟 Premium\n" +
            "32MB ☁️\n\n" +
            "🚀 Professional\n" +
            "64MB ☁️\n\n" +
            "• Подписка действует 1 месяц после покупки";
        #endregion Private_Fields

        #region Public_Methods
        public async Task ShowMenu(long userId)
        {
            var buttonMenu = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Информация Профиля", "ProfileInfo"),
                    InlineKeyboardButton.WithCallbackData("Оформить подписку", "Subscribe"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Получить файл", "GetFile"),
                    InlineKeyboardButton.WithCallbackData("Удалить Файл", "DeleteFile"),
                },
            });
            await MessageHandler.SendMessageUser(userId, "Выберите пункт меню", buttonMenu);
        }
        public IReplyMarkup GenerateInlineKeyboardButtons(List<string> fileList, FileActions.Action action)
        {
            string actionFile = string.Empty;
            switch (action)
            {
                case FileActions.Action.GetFile:
                    actionFile = _getFile;
                    break;
                case FileActions.Action.DeleteFile:
                    actionFile = _deleteFile;
                    break;
                default:
                    break;
            }

            List<InlineKeyboardButton[]> buttonsArray = new List<InlineKeyboardButton[]>();

            for (int i = 0; i < fileList.Count; i++)
            {
                string buttonText = $"{fileList[i]}";

                InlineKeyboardButton button = new InlineKeyboardButton(buttonText)
                {
                    CallbackData = $"{actionFile}{fileList[i]}"
                };

                buttonsArray.Add(new InlineKeyboardButton[] { button });
            }

            return new InlineKeyboardMarkup(buttonsArray.ToArray());
        }
        public async Task DonationMenu(long userId)
        {
            var buttonDonateMenu = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Basic 16MB ☁️", "SUB_Basic"),
                    InlineKeyboardButton.WithCallbackData("Premium 32MB ☁️", "SUB_Premium"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Professional 64MB ☁️", "SUB_Professional"),                    
                },
            });
            await MessageHandler.SendMessageUser(userId, _donateInfo, buttonDonateMenu);
        }

        public async Task SubscriptionRenewalButtonsMenu(long userId, string subName)
        {
            var btnMenu = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✅ Продлить подписку", $"RSUB_{subName}"),
                    InlineKeyboardButton.WithCallbackData("❌ Продолжить без подписки", "RSUB_NotRenewal"),
                }
            });
            await MessageHandler.SendMessageUser(userId, $"У вас закончилась ежемесячная подписка {subName}", btnMenu);
        }
        public async Task Greeting(long chatId)
        {
            await BotVM.BotClient.SendTextMessageAsync(chatId, _greetingMessage);
        }
        #endregion Public_Methods
    }
}
