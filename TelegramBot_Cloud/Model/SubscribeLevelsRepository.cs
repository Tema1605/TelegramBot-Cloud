using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TelegramBot_Cloud.Model
{
    internal static class SubscribeLevelsRepository
    {
        private static readonly ObservableCollection<SubscribeLevels> _subscribeLevels
            = new ObservableCollection<SubscribeLevels>() 
            { 
                new SubscribeLevels() { Name = "Standart", LimitBytes = 8388608},  
                new SubscribeLevels() { Name = "Basic", LimitBytes = 16777216},
                new SubscribeLevels() { Name = "Premium", LimitBytes = 33554432},
                new SubscribeLevels() { Name = "Professional", LimitBytes = 67108864}
            };

        /// <summary>
        /// Получение лимита памяти подписки.
        /// </summary>
        /// <param name="subName">Название подписки</param>
        /// <returns>Лимит памяти подписки в байтах</returns>
        public static long GetMemorySubscribe(string subName)
        {
            try
            {
                var limitMemory = _subscribeLevels.FirstOrDefault(x => x.Name == subName).LimitBytes;
                return limitMemory;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }
    }
}
