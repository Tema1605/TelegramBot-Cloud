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
        #region Private_Methods
        /// <summary>
        /// Проверяет, существует ли папка, в противном случае создает.
        /// </summary>
        /// <param name="path">Путь до папки.</param>
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

        /// <summary>
        /// Проверяет, является ли папка пустой.
        /// </summary>
        /// <param name="path">Путь до папки.</param>
        /// <returns>True, если папка не пустая. False, в противном случае.</returns>
        private bool IsFolderEmpty(string path)
        {
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path).Length != 0 || Directory.GetDirectories(path).Length != 0;
            }

            return false;
        }
        #endregion Private_Methods

        #region Public_Methods
        /// <summary>
        /// Сохраняет файл.
        /// </summary>
        /// <param name="file">Файл.</param>
        /// <param name="fileName">Имя Файла.</param>
        /// <param name="path">Путь до папки.</param>
        /// <returns>True, если файл сохранен. False, в противном случае.</returns>
        public async Task<bool> FileSaving(Telegram.Bot.Types.File file, string fileName, string path)
        {            
            CheckAndCreateFolder(path); //Проверка существования или создание папки пользователя
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

        /// <summary>
        /// Вычисляет, вес папки.
        /// </summary>
        /// <param name="path">Путь до папки.</param>
        /// <returns>Возвращает вес папки в МБ.</returns>
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
                Console.WriteLine("Ошибка при расчете занимаемого пространства: " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Отправляет файл пользователю.
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        /// <param name="path">Путь до папки.</param>
        /// <param name="fileName">Имя файла.</param>
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

        /// <summary>
        /// Удаляет файл.
        /// </summary>
        /// <param name="path">Путь до файла.</param>
        /// <returns>True, если файл удален. False, в противном случае.</returns>
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

        /// <summary>
        /// Получает список файлов пользователя.
        /// </summary>
        /// <param name="path">Путь до папки.</param>
        /// <returns>Возвращает список файлов пользователя.</returns>
        public List<string> GetListUserFiles(string path)
        {
            CheckAndCreateFolder(path); //Проверка существования или создание папки пользователя

            if (IsFolderEmpty(path)) //Проверка на наличие файлов в папке
            {
                List<string> filesDir = (from a in Directory.GetFiles(path) select Path.GetFileName(a)).ToList();
                return filesDir;
            }
            return null;
        }
        #endregion Public_Methods
    }
}
