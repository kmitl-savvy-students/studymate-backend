using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmFaculty : ISdmBaseMethod<Faculty>
{
    public static string TableName => "faculty";
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
                query.ToBool(1),
                query.ToString(2),
                query.ToString(3),
                query.ToString(4)
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
        select.WhereEqual("fac_id", id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static void Insert(Faculty faculty)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("fac_kmitl_id", faculty.KmitlId);
        insert.Insert("fac_name_th", faculty.NameTh);
        insert.Insert("fac_name_en", faculty.NameEn);

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    public static void UpdateBy(Faculty faculty)
    {
        var update = new SdmMysqlQueryUpdate(TableName);

        update.Set("fac_is_visible", faculty.IsVisible ? "1" : "0");
        update.Set("fac_kmitl_id", faculty.KmitlId);
        update.Set("fac_name_th", faculty.NameTh);
        update.Set("fac_name_en", faculty.NameEn);

        update.WhereEqual("fac_id", faculty.Id.ToString());

        var query = SdmMysqlQuery.Execute(update);
        query.CleanUp();
    }
}