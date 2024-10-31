using Npgsql;

namespace studymate_backend.Libraries.Database;

public class SdmDataSource
{
    private static NpgsqlDataSource? dataSource { get; set; }

    public static NpgsqlDataSource? get()
    {
        if (dataSource != null)
            return dataSource;

        try
        {
            var server = Environment.GetEnvironmentVariable("DB_SERVER");
            var database = Environment.GetEnvironmentVariable("DB_NAME");
            var userId = Environment.GetEnvironmentVariable("DB_USER");
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

            var connectionString = $"Host={server};Database={database};Username={userId};Password={password};";

            dataSource = NpgsqlDataSource.Create(connectionString);
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine("ERROR: SdmDataSource.get(): " + ex.Message);
        }

        return dataSource;
    }
}