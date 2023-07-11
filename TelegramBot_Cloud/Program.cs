using System;
using Telegram.Bot.Types;
using TelegramBot_Cloud.ViewModel;

namespace TelegramBot_Cloud
{
    internal class Program
    {
        static void Main(string[] args)
        {
            StartBot();
            Console.ReadKey();
        }
        private static void StartBot()
        {
            new BotVM();
        }
        
    }
}
