using System;
using System.Data.SqlClient;

namespace TelegramBot_Cloud.Model
{
    internal class DataProcessing
    {
        public bool UserVerification(long userId)
        {
            object result;

            using (SqlConnect connection = new SqlConnect())
            {
                string query = $"select id from Users where id = {userId};";

                connection.OpenConnection();

                SqlCommand command = new SqlCommand(query, connection.GetConnection());
                result = command.ExecuteScalar();
            }

            if(result != null && result != DBNull.Value) return true;
            else return false;
        }
        public void RegistrationUser(long userId, string username = null, string firstname = null)
        {
            using (SqlConnect connection = new SqlConnect())
            {
                connection.OpenConnection();

                string query = "insert into Users(id,username,first_name) values(@id, @username, @firstName)";

                SqlCommand command = new SqlCommand(query, connection.GetConnection());
                command.Parameters.AddWithValue("@id", userId);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@firstName", firstname);

                command.ExecuteNonQuery();
            }
        }

        public void SavingFileData(long userId, string filename, double fileWeight)
        {
            using (SqlConnect connection = new SqlConnect())
            {
                connection.OpenConnection();

                string query = "insert into Files(user_id, file_name, file_size, created_at_time) values(@userId, @fileName, @fileSize, @createdAtTime)";

                SqlCommand command = new SqlCommand(query, connection.GetConnection());
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@fileName", filename);
                command.Parameters.AddWithValue("@fileSize", fileWeight);
                command.Parameters.AddWithValue("@createdAtTime", DateTime.Now);

                command.ExecuteNonQuery();
            }
        }
    }
}
