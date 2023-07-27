using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace TelegramBot_Cloud.Model
{
    internal class DataProcessing
    {
        /// <summary>
        /// Проверка на существование данных о пользователе в БД.
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        /// <returns>True, если пользователь зарегестрирован. False, в противном случае.</returns>
        public async Task<bool> UserVerificationAsync(long userId)
        {
            bool userExists;

            using (SqlConnect connection = new SqlConnect())
            {
                try
                {
                    string query = "SELECT COUNT(*) FROM Users WHERE id = @userId";
                    await connection.OpenConnectionAsync();

                    using (SqlCommand command = new SqlCommand(query, connection.GetConnection()))
                    {
                        command.Parameters.AddWithValue("@userId", userId);

                        int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                        userExists = (count > 0);
                    }
                    return userExists;
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.Message);
                    return false;
                }                
            }
        }

        /// <summary>
        /// Заполнение данных о пользователе в БД.
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        /// <param name="username">Имя пользователя.</param>
        /// <param name="firstName">Имя.</param>
        public async Task RegistrationUserAsync(long userId, string username = null, string firstName = null)
        {
            using (SqlConnect connection = new SqlConnect())
            {
                try
                {
                    await connection.OpenConnectionAsync();

                    string query = 
                        "INSERT INTO Users (id, username, first_name) VALUES (@id, @username, @firstName);" +
                        "INSERT INTO User_subscription (user_id, sub_id) VALUES (@id, @sub)";

                    using (SqlCommand command = new SqlCommand(query, connection.GetConnection()))
                    {
                        command.Parameters.AddWithValue("@id", userId);
                        command.Parameters.AddWithValue("@username", DBNull.Value);
                        command.Parameters.AddWithValue("@firstName", DBNull.Value);
                        command.Parameters.AddWithValue("@sub", "Standart");

                        if (username != null)
                        {
                            command.Parameters["@username"].Value = username;
                        }

                        if (firstName != null)
                        {
                            command.Parameters["@firstName"].Value = firstName;
                        }

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch(Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.Message);
                }
            }
        }

        /// <summary>
        /// Сохранение данных о файле в БД
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        /// <param name="filename">Имя файла.</param>
        /// <param name="fileWeight">Вес файла.</param>
        public async Task SavingFileDataAsync(long userId, string filename, double fileWeight)
        {
            using (SqlConnect connection = new SqlConnect())
            {
                try
                {
                    await connection.OpenConnectionAsync();

                    string query = "INSERT INTO Files (user_id, file_name, file_size, created_at_time) VALUES (@userId, @fileName, @fileSize, @createdAtTime)";

                    using (SqlCommand command = new SqlCommand(query, connection.GetConnection()))
                    {
                        command.Parameters.Add("@userId", SqlDbType.BigInt).Value = userId;
                        command.Parameters.Add("@fileName", SqlDbType.NVarChar).Value = filename;
                        command.Parameters.Add("@fileSize", SqlDbType.Float).Value = fileWeight;
                        command.Parameters.Add("@createdAtTime", SqlDbType.DateTime).Value = DateTime.Now;

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.Message);
                }
            }
        }

        /// <summary>
        /// Получение текущей подписки пользователя.
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        /// <returns>Название подписки пользователя</returns>
        public async Task<string> GetUserSubscribeAsync(long userId)
        {
            using (SqlConnect connection = new SqlConnect())
            {
                try
                {
                    await connection.OpenConnectionAsync();

                    string query = $"select sub_id from User_subscription where user_id = {userId}";
                    dynamic result = null;

                    using (SqlCommand command = new SqlCommand(query, connection.GetConnection()))
                    {
                        result = await command.ExecuteScalarAsync();
                    }
                    string subName = result.ToString();
                    return subName;
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.Message);
                    return null;
                }
            }
        }

        /// <summary>
        /// Получение даты окончания текущей подписки пользователя.
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        /// <returns>Дата окончания подписки.</returns>
        public async Task<DateTime> GetExpirationDateSubscribeAsync(long userId)
        {
            using (SqlConnect connection = new SqlConnect())
            {
                try
                {
                    await connection.OpenConnectionAsync();

                    string query = $"select expiration_date from User_subscription where user_id = {userId}";

                    using (SqlCommand command = new SqlCommand(query, connection.GetConnection()))
                    {
                        var result =  await command.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            DateTime dateTimeValue = (DateTime)result;
                            return dateTimeValue;
                        }
                        return DateTime.MinValue;
                    }
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.Message);
                    return DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// Обновление подписки пользователя.
        /// </summary>
        /// <param name="userId">Id Пользователя.</param>
        /// <param name="subName">Название подписки.</param>
        public async Task UpdateUserSubscribeAsync(long userId, string subName)
        {
            using (SqlConnect connection = new SqlConnect())
            {
                try
                {
                    await connection.OpenConnectionAsync();

                    string query = 
                        $"UPDATE User_subscription " +
                        $"SET sub_id = '{subName}', expiration_date = DATEADD(month, 1, CURRENT_TIMESTAMP) " +
                        $"WHERE user_id = {userId}";

                    using (SqlCommand command = new SqlCommand(query, connection.GetConnection()))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.Message);
                }
            }
        }

        /// <summary>
        /// Получение списка пользователей у которых закончилась подписка
        /// </summary>
        /// <returns>Список Id пользователей у которых закончилась подписка</returns>
        public async Task<List<long>> GetExpiredSubscriptionsAsync()
        {
            using (SqlConnect connection = new SqlConnect())
            {
                try
                {
                    List<long> usersId = new List<long>();
                    await connection.OpenConnectionAsync();

                    string query =
                        $"SELECT user_id " +
                        $"FROM User_subscription " +
                        $"WHERE expiration_date < GetDate()" +
                        $"AND sub_id != 'Standart'";

                    using (SqlCommand command = new SqlCommand(query, connection.GetConnection()))
                    {
                        var _row = await command.ExecuteReaderAsync();

                        if (_row == null) return null;
                        if (_row.HasRows)
                        {
                            while (_row.Read())
                            {
                                usersId.Add(_row.GetInt64(0));
                            }
                        }
                        _row.Close();
                        return usersId;
                    }
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.Message);
                    return null;
                }
            }
        }
    }
}
