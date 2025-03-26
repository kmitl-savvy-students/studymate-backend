using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmProgram : ISdmBaseMethod<Models.Program>
{
    public static string TableName => "program";
    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }
    public static List<Models.Program> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<Models.Program>();
        while (query.Next())
        {
            result.Add(new Models.Program(
                query.ToInt(0),
                query.ToBool(1),
                query.ToString(2),
                SdmDepartment.GetBy(query.ToInt(3)),
                query.ToString(4),
                query.ToString(5)
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<Models.Program> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }
    public static List<Models.Program> GetAllBy(int departmentId)
    {
        var select = GetQueryObj();
        select.WhereEqual("prog_dep_id", departmentId.ToString());

        var result = ProcessQuery(select, true);
        return result;
    }
    public static Models.Program? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("prog_id", id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }
    public static Models.Program? GetByKmitl(string id)
    {
        var select = GetQueryObj();
        select.WhereEqual("prog_kmitl_id", id);

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static void Insert(Models.Program program)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("prog_kmitl_id", program.KmitlId);
        insert.Insert("prog_dep_id", program.Department?.Id.ToString());
        insert.Insert("prog_name_th", program.NameTh);
        insert.Insert("prog_name_en", program.NameEn);

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    public static void UpdateBy(Models.Program program)
    {
        var update = new SdmMysqlQueryUpdate(TableName);

        update.Set("prog_is_visible", program.IsVisible ? "1" : "0");
        update.Set("prog_kmitl_id", program.KmitlId);
        update.Set("prog_dep_id", program.Department?.Id.ToString());
        update.Set("prog_name_th", program.NameTh);
        update.Set("prog_name_en", program.NameEn);

        update.WhereEqual("prog_id", program.Id.ToString());

        var query = SdmMysqlQuery.Execute(update);
        query.CleanUp();
    }
}