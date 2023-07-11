using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TelegramBot_Cloud.Model
{
    internal class SqlConnect : IDisposable
    {
        SqlConnection sqlConnection = new SqlConnection(@"Data Source=LAPTOP-5OB9P5SK\SQLEXPRESS;Initial Catalog=TelegramBotCloud;Integrated Security=True");

        internal void OpenConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Closed) { }
            try
            {
                sqlConnection.Open();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex}\n{ex.Message}");
            }
        }

        internal void CloseConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Open) { }
            sqlConnection.Close();
        }

        internal SqlConnection GetConnection()
        {
            return sqlConnection;
        }

        public void Dispose()
        {
            CloseConnection();
        }
    }
}
