using Npgsql;
using studymate_backend.Libraries.Database.QueryBuilders;

namespace studymate_backend.Libraries.Database;

public class SdmPgsqlQuery(
    ISdmPgsqlQueryBase queryBase
)
{
    private NpgsqlCommand? _command;
    private NpgsqlDataReader? _reader;
    private NpgsqlDataSource? _source;

    public static SdmPgsqlQuery Execute(ISdmPgsqlQueryBase queryBase)
    {
        var query = new SdmPgsqlQuery(queryBase);
        query.GetReader();
        return query;
    }

    private NpgsqlDataReader? GetReader()
    {
        if (_reader != null)
            return _reader;

        var dataSource = SdmDataSource.Get();
        if (dataSource == null)
            return null;

        _source = dataSource;
        try
        {
            _command = _source.CreateCommand(queryBase.Build());
            _reader = _command.ExecuteReader();
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine("ERROR: SdmPostgresQuery.getReader(): " + ex.Message);
        }

        return _reader;
    }

    public string ToString(int columnIndex)
    {
        var reader = GetReader();

        if (reader == null)
            return string.Empty;

        return reader.IsDBNull(columnIndex) ? string.Empty : reader.GetString(columnIndex);
    }
    public int ToInt(int columnIndex)
    {
        var reader = GetReader();

        if (reader == null)
            return -1;

        return reader.IsDBNull(columnIndex) ? -1 : reader.GetInt32(columnIndex);
    }

    public bool Next()
    {
        return GetReader()?.Read() ?? false;
    }

    public void CleanUp()
    {
        _reader?.Dispose();
        _command?.Dispose();
    }
}
