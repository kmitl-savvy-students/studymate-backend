using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmDepartment : ISdmBaseMethod<Department>
{
    public static string TableName => "Department";
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
                SdmFaculty.GetBy(query.ToInt(1)),
                query.ToString(2),
                query.ToString(3)
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
        select.WhereEqual("FacultyId", facultyId.ToString());

        var result = ProcessQuery(select, true);
        return result;
    }
    public static Department? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("Id", id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static void Insert(Department department)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("FacultyId", department.Faculty?.Id.ToString());
        insert.Insert("NameTh", department.NameTh);
        insert.Insert("NameEn", department.NameEn);

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    public static void UpdateBy(Department department)
    {
        var update = new SdmMysqlQueryUpdate(TableName);

        update.Set("FacultyId", department.Faculty?.Id.ToString());
        update.Set("NameTh", department.NameTh);
        update.Set("NameEn", department.NameEn);

        update.WhereEqual("Id", department.Id.ToString());

        var query = SdmMysqlQuery.Execute(update);
        query.CleanUp();
    }
}