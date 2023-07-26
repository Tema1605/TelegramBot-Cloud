using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBot_Cloud.ViewModel;

namespace TelegramBot_Cloud
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await StartBot();
            Console.ReadKey();
        }
        private static async Task StartBot()
        {
            await new BotVM().InitializeAsync();
        }
        
    }
}
