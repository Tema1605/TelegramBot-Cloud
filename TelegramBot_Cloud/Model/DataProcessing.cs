using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBot_Cloud.Model
{
    internal class DataProcessing
    {
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

        public async Task<string> GetUserSubscribe(long userId)
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

        public async Task<object> GetExpirationDateSubscribe(long userId)
        {
            using (SqlConnect connection = new SqlConnect())
            {
                try
                {
                    await connection.OpenConnectionAsync();

                    string query = $"select expiration_date from User_subscription where user_id = {userId}";

                    using (SqlCommand command = new SqlCommand(query, connection.GetConnection()))
                    {
                        return await command.ExecuteScalarAsync();
                    }
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.Message);
                    return null;
                }
            }
        }

        public async Task UpdateUserSubscribe(long userId, string subName)
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

        public async Task<List<long>> GetExpiredSubscriptions()
        {
            using (SqlConnect connection = new SqlConnect())
            {
                try
                {
                    List<long> usersId = new List<long>();
                    await connection.OpenConnectionAsync();

                    string query = $"" +
                        $"SELECT user_id " +
                        $"FROM User_subscription " +
                        $"WHERE expiration_date < GetDate()";

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

        //public async Task GetDonateListAsync()
        //{
        //    using(SqlConnect connection = new SqlConnect())
        //    {
        //        try
        //        {
        //            await connection.OpenConnectionAsync();

        //            string query = "select * from Subscribe_level";
        //            //ObservableCollection<string> donate = new ObservableCollection<string>();

        //            using (SqlCommand command = new SqlCommand(query, connection.GetConnection()))
        //            {
        //                var _row = command.ExecuteReader();

        //                if (_row == null) return;
        //                if (_row.HasRows)
        //                {
        //                    while (_row.Read())
        //                    {
        //                        SubscribeLevelsRepository._subscribeLevels.Add(new SubscribeLevels()
        //                        {
        //                            Name = _row.GetString(0),
        //                            LimitBytes = _row.GetInt64(1)
        //                        });
        //                    }
        //                }
        //                _row.Close();
        //            }
        //        }
        //        catch(Exception ex)
        //        {
        //            await Console.Out.WriteLineAsync(ex.Message);
        //        }
        //    }
        //}
    }
}
