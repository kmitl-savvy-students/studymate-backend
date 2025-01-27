using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmFaculty : ISdmBaseMethod<Faculty>
{
    public static string TableName => "Faculty";
    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }
    public static List<Faculty> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<Faculty>();
        while (query.Next())
        {
            result.Add(new Faculty(
                query.ToInt(0),
                query.ToString(1),
                query.ToString(2)
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<Faculty> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }
    public static Faculty? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("Id", id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static void Insert(Faculty faculty)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("NameTh", faculty.NameTh);
        insert.Insert("NameEn", faculty.NameEn);

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    public static void UpdateBy(Faculty faculty)
    {
        var update = new SdmMysqlQueryUpdate(TableName);

        update.Set("NameTh", faculty.NameTh);
        update.Set("NameEn", faculty.NameEn);

        update.WhereEqual("Id", faculty.Id.ToString());

        var query = SdmMysqlQuery.Execute(update);
        query.CleanUp();
    }
}