using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TelegramBot_Cloud.Model
{
    internal class SqlConnect : IDisposable
    {
        SqlConnection sqlConnection = new SqlConnection(
            @"Data Source=TEMADRUZHININ\SQLEXPRESS;Initial Catalog=TelegramBotCloud;Integrated Security=True");
        public async Task OpenConnectionAsync()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                try
                {
                    await sqlConnection.OpenAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex}\n{ex.Message}");
                }
            }
        }
        public void CloseConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Open)
            {
                sqlConnection.Close();
            }
        }
        public SqlConnection GetConnection()
        {
            return sqlConnection;
        }
        public void Dispose()
        {
            CloseConnection();
        }
    }
}
