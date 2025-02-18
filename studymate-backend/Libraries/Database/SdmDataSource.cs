using MySql.Data.MySqlClient;

namespace studymate_backend.Libraries.Database;

public static class SdmDataSource
{
    public static MySqlConnection? Get()
    {
        var server = Environment.GetEnvironmentVariable("MYSQL_SERVER");
        var user = Environment.GetEnvironmentVariable("MYSQL_USER");
        var password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");
        var database = Environment.GetEnvironmentVariable("MYSQL_DATABASE");

        if (string.IsNullOrEmpty(server) ||
            string.IsNullOrEmpty(user) ||
            string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(database))
        {
            Console.WriteLine("ERROR: Database connection environment variables are not set.");
            return null;
        }

        try
        {
            var connectionString = $"server={server};uid={user};pwd={password};database={database};";
            var newConnection = new MySqlConnection(connectionString);

            return newConnection;
        }
        catch (MySqlException ex)
        {
            Console.WriteLine("ERROR: Database connection failed: " + ex.Message);
            return null;
        }
    }
}