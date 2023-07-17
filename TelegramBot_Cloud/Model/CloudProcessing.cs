using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot_Cloud.ViewModel;

namespace TelegramBot_Cloud.Model
{
    internal class CloudProcessing
    {
        #region Private_Fields
        //private static readonly string _filePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\TelegramBotCloud";
        
        #endregion Private_Fields

        #region Private_Methods
        private void CheckAndCreateFolder(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при создании папки пользователя: " + ex);
            }
        }
        #endregion Private_Methods

        #region Public_Methods
        public async Task<bool> FileSaving(Telegram.Bot.Types.File file, string fileName, string path)
        {
            CheckAndCreateFolder(path);
            string filePath = $"{path}\\{fileName}";
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await BotVM.BotClient.DownloadFileAsync(file.FilePath, fileStream);
                    return true;
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Ошибка при сохранении файла в облако: " + ex.Message);
                return false;
            }
        }
        public double CalculateFolderWeight(string path)
        {
            long totalSize = 0;
            try
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                FileInfo[] files = directory.GetFiles("*.*", SearchOption.AllDirectories);

                foreach (FileInfo file in files)
                {
                    totalSize += file.Length;
                }
                return Converter.BytesToMegabytes(totalSize);
            } 
            catch(Exception ex)
            {
                Console.WriteLine("Ошибка при расчете свободного места: " + ex.Message);
                return 0;
            }
        }
        public async Task GetFile(long userId, string path, string fileName)
        {
            string filePath = $"{path}\\{fileName}";
            try
            {
                using (Stream stream = System.IO.File.OpenRead(filePath))
                {
                    await BotVM.BotClient.SendDocumentAsync(
                        chatId: userId,
                        document: InputFile.FromStream(stream: stream, fileName: fileName));
                    stream.Close();
                }

            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Ошибка при получении файла из облака: " + ex.Message);                
            }
            
        }
        public async Task<bool> DeleteFile(string path)
        {
            try
            {
                using (FileStream fileStream = System.IO.File.Open(path, FileMode.Open))
                {
                    fileStream.Close();
                    System.IO.File.Delete(path);
                    return true;
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Ошибка при удалении файла в облаке: " + ex.Message);
                return false;
            }
        }
        public List<string> GetListUserFiles(string path)
        {
            CheckAndCreateFolder(path);
            List<string> filesDir = (from a in Directory.GetFiles(path) select Path.GetFileName(a)).ToList();
            return filesDir;
        }
        #endregion Public_Methods
    }
}
