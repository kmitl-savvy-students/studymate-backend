using MySql.Data.MySqlClient;
using System;

namespace studymate_backend.Libraries.Database
{
    public static class SdmDataSource
    {
        private static MySqlConnection? Connection { get; set; }

        public static MySqlConnection? Get()
        {
            if (Connection == null)
            {
                try
                {
                    const string connectionString = "server=192.168.50.52;uid=admin;pwd=adminsdmkmitl;database=sdm-kmitl-db";
                    Connection = new MySqlConnection(connectionString);
                    Connection.Open();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("ERROR: SdmDataSource.Get(): " + ex.Message);
                    Connection = null;
                }
            }
            else if (Connection.State != System.Data.ConnectionState.Open)
            {
                Connection.Open();
            }

            return Connection;
        }

        public static void Dispose()
        {
            if (Connection == null)
                return;
            Connection.Close();
            Connection.Dispose();
            Connection = null;
        }
    }
}