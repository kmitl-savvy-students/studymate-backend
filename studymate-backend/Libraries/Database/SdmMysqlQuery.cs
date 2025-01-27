using System.Data;
using MySql.Data.MySqlClient;
using studymate_backend.Libraries.Database.QueryBuilders;

namespace studymate_backend.Libraries.Database;

public class SdmMysqlQuery(ISdmMysqlQueryBase queryBase)
{
    private int _columnCount;
    private int _currentRowIndex = -1;
    private List<object?[]>? _rows;

    public int InsertedId;

    public static SdmMysqlQuery Execute(ISdmMysqlQueryBase queryBase)
    {
        var query = new SdmMysqlQuery(queryBase);

        if (queryBase is SdmMysqlQueryInsert)
            query.ExecuteScalar();
        else
            query.LoadAllRows();

        return query;
    }

    private void ExecuteScalar()
    {
        var connection = SdmDataSource.Get();
        if (connection is not { State: ConnectionState.Open })
            throw new InvalidOperationException("Database connection is not available.");

        using var command = new MySqlCommand(queryBase.Build(), connection);
        var result = command.ExecuteScalar();
        if (result is int intValue)
            InsertedId = intValue;
    }


    private void LoadAllRows()
    {
        var connection = SdmDataSource.Get();
        if (connection == null)
            return;

        using var command = new MySqlCommand(queryBase.Build(), connection);
        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);

        _columnCount = reader.FieldCount;
        _rows = [];

        while (reader.Read())
        {
            var values = new object?[reader.FieldCount];
            reader.GetValues(values);
            _rows.Add(values);
        }
    }

    public bool Next()
    {
        if (_rows == null || _rows.Count == 0)
            return false;

        _currentRowIndex++;
        return _currentRowIndex < _rows.Count;
    }

    public void CleanUp()
    {
        _rows = null;
        _currentRowIndex = -1;
    }

    private object? GetValue(int columnIndex)
    {
        if (_rows == null || _currentRowIndex < 0 || _currentRowIndex >= _rows.Count)
            return null;

        if (columnIndex < 0 || columnIndex >= _columnCount)
            return null;

        return _rows[_currentRowIndex][columnIndex];
    }

    public string ToString(int columnIndex)
    {
        var val = GetValue(columnIndex);
        return val is null or DBNull ? string.Empty : val.ToString() ?? string.Empty;
    }

    public bool ToBool(int columnIndex)
    {
        var val = GetValue(columnIndex);
        if (val is null or DBNull)
            return false;

        return bool.TryParse(val.ToString(), out var result) && result;
    }

    public int ToInt(int columnIndex)
    {
        var val = GetValue(columnIndex);
        if (val is null or DBNull)
            return -1;

        if (int.TryParse(val.ToString(), out var result))
            return result;

        return -1;
    }

    public string ToDateTime(int columnIndex)
    {
        var val = GetValue(columnIndex);
        return val switch
        {
            null or DBNull => string.Empty,
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
            _ => DateTime.TryParse(val.ToString(), out var parsedDt)
                ? parsedDt.ToString("yyyy-MM-dd HH:mm:ss")
                : string.Empty
        };
    }

    public float ToFloat(int columnIndex)
    {
        var val = GetValue(columnIndex);
        if (val is null or DBNull)
            return 0.0f;

        return float.TryParse(val.ToString(), out var result) ? result : 0.0f;
    }
}