using Npgsql;
using System;

namespace studymate_backend.Libraries.Database
{
    public class SdmDataSource
    {
        private static NpgsqlDataSource? DataSource { get; set; }

        public static NpgsqlDataSource? Get()
        {
            if (DataSource != null)
                return DataSource;

            try
            {
                var server = Environment.GetEnvironmentVariable("DB_SERVER");
                var database = Environment.GetEnvironmentVariable("DB_NAME");
                var userId = Environment.GetEnvironmentVariable("DB_USER");
                var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

                var connectionString = $"Host={server};Database={database};Username={userId};Password={password};";

                DataSource = NpgsqlDataSource.Create(connectionString);
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine("ERROR: SdmDataSource.get(): " + ex.Message);
            }

            return DataSource;
        }

        public static void Dispose()
        {
            DataSource?.Dispose();
            DataSource = null;
        }
    }
}