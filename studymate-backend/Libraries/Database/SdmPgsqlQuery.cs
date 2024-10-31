using Npgsql;

namespace studymate_backend.Libraries.Database;

public class SdmPgsqlQuery(
    ISdmPgsqlQueryBuilder queryBuilder
)
{
    private NpgsqlDataSource? _source;
    private NpgsqlCommand? _command;
    private NpgsqlDataReader? _reader;

    public NpgsqlDataReader? getReader()
    {
        var dataSource = SdmDataSource.get();
        if (dataSource == null)
            return null;
        
        _source = dataSource;
        try
        {
            _command = _source.CreateCommand(queryBuilder.build());
            _reader = _command.ExecuteReader();
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine("ERROR: SdmPostgresQuery.getReader(): " + ex.Message);
        }

        return _reader;
    }

    public void close()
    {
        _reader?.Close();
        _command?.Dispose();
        _source?.Dispose();
    }
}