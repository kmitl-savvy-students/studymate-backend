using Npgsql;
using studymate_backend.Libraries.Database.QueryBuilders;
using System;
using System.Collections.Generic;
using System.Data;

namespace studymate_backend.Libraries.Database
{
    public class SdmPgsqlQuery
    {
        private readonly ISdmPgsqlQueryBase queryBase;

        // Instead of holding a live reader and command, we'll store all results in memory.
        private List<object?[]>? _rows;
        private int _currentRowIndex = -1;

        // Store column count after loading
        private int _columnCount = 0;

        // For insertedId when executing scalar inserts
        public int insertedId;

        public SdmPgsqlQuery(ISdmPgsqlQueryBase queryBase)
        {
            this.queryBase = queryBase;
        }

        public static SdmPgsqlQuery Execute(ISdmPgsqlQueryBase queryBase)
        {
            var query = new SdmPgsqlQuery(queryBase);

            if (queryBase is SdmPgsqlQueryInsert)
            {
                query.ExecuteScalar();
            }
            else
            {
                query.LoadAllRows();
            }

            return query;
        }

        private void ExecuteScalar()
        {
            var dataSource = SdmDataSource.Get();
            if (dataSource == null)
                return;

            using (var command = dataSource.CreateCommand(queryBase.Build()))
            {
                var result = command.ExecuteScalar();
                if (result is int intValue)
                    insertedId = intValue;
            }
            // Connection is closed automatically after command disposal since we used DataSource.CreateCommand()
        }

        private void LoadAllRows()
        {
            var dataSource = SdmDataSource.Get();
            if (dataSource == null)
                return;

            using (var command = dataSource.CreateCommand(queryBase.Build()))
            using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                _columnCount = reader.FieldCount;
                _rows = new List<object?[]>();

                while (reader.Read())
                {
                    var values = new object?[reader.FieldCount];
                    reader.GetValues(values);
                    _rows.Add(values);
                }
            }
            // Reader and command disposed, connection closed immediately after load.
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
            // Nothing to clean up now since we close and dispose immediately after query execution
            // But we implement this so usage code that calls CleanUp() still works without changes.
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
            return val == null || val is DBNull ? string.Empty : val.ToString() ?? string.Empty;
        }

        public int ToInt(int columnIndex)
        {
            var val = GetValue(columnIndex);
            if (val == null || val is DBNull)
                return -1;

            if (int.TryParse(val.ToString(), out var result))
                return result;

            return -1;
        }

        public string ToDateTime(int columnIndex)
        {
            var val = GetValue(columnIndex);
            if (val == null || val is DBNull)
                return string.Empty;

            if (val is DateTime dt)
                return dt.ToString("yyyy-MM-dd HH:mm:ss zzz");

            // Attempt to parse if it's a string
            if (DateTime.TryParse(val.ToString(), out var parsedDt))
                return parsedDt.ToString("yyyy-MM-dd HH:mm:ss zzz");

            return string.Empty;
        }

        public float ToFloat(int columnIndex)
        {
            var val = GetValue(columnIndex);
            if (val == null || val is DBNull)
                return 0.0f;

            if (float.TryParse(val.ToString(), out var result))
                return result;

            return 0.0f;
        }
    }
}
