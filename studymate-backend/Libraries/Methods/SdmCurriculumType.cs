using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmCurriculumType : ISdmBaseMethod<CurriculumType>
{
    public static string TableName => "CurriculumType";
    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }
    public static List<CurriculumType> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<CurriculumType>();
        while (query.Next())
        {
            result.Add(new CurriculumType(
                query.ToInt(0),
                SdmDepartment.GetBy(query.ToInt(1)),
                query.ToString(2),
                query.ToString(3)
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<CurriculumType> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }
    public static List<CurriculumType> GetAllBy(int departmentId)
    {
        var select = GetQueryObj();
        select.WhereEqual("DepartmentId", departmentId.ToString());

        var result = ProcessQuery(select, true);
        return result;
    }
    public static CurriculumType? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("Id", id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static void Insert(CurriculumType curriculumType)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("DepartmentId", curriculumType.Department?.Id.ToString());
        insert.Insert("NameTh", curriculumType.NameTh);
        insert.Insert("NameEn", curriculumType.NameEn);

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    public static void UpdateBy(CurriculumType curriculumType)
    {
        var update = new SdmMysqlQueryUpdate(TableName);

        update.Set("DepartmentId", curriculumType.Department?.Id.ToString());
        update.Set("NameTh", curriculumType.NameTh);
        update.Set("NameEn", curriculumType.NameEn);

        update.WhereEqual("Id", curriculumType.Id.ToString());

        var query = SdmMysqlQuery.Execute(update);
        query.CleanUp();
    }
}