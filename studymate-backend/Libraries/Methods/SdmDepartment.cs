using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmDepartment : ISdmBaseMethod<Department>
{
    public static string TableName => "department";
    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }
    public static List<Department> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<Department>();
        while (query.Next())
        {
            result.Add(new Department(
                query.ToInt(0),
                query.ToInt(1),
                query.ToString(2),
                SdmFaculty.GetBy(query.ToInt(3)),
                query.ToString(4),
                query.ToString(5)
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<Department> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }
    public static List<Department> GetAllBy(int facultyId)
    {
        var select = GetQueryObj();
        select.WhereEqual("dep_fac_id", facultyId.ToString());

        var result = ProcessQuery(select, true);
        return result;
    }
    public static Department? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("dep_id", id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static void Insert(Department department)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("dep_kmitl_id", department.KmitlId);
        insert.Insert("dep_fac_id", department.Faculty?.Id.ToString());
        insert.Insert("dep_name_th", department.NameTh);
        insert.Insert("dep_name_en", department.NameEn);

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    public static void UpdateBy(Department department)
    {
        var update = new SdmMysqlQueryUpdate(TableName);

        update.Set("dep_is_visible", department.isVisible.ToString());
        update.Set("dep_kmitl_id", department.KmitlId);
        update.Set("dep_fac_id", department.Faculty?.Id.ToString());
        update.Set("dep_name_th", department.NameTh);
        update.Set("dep_name_en", department.NameEn);

        update.WhereEqual("dep_id", department.Id.ToString());

        var query = SdmMysqlQuery.Execute(update);
        query.CleanUp();
    }
}