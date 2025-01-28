using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmCurriculum : ISdmBaseMethod<Curriculum>
{
    public static string TableName => "Curriculum";
    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }
    public static List<Curriculum> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<Curriculum>();
        while (query.Next())
        {
            result.Add(new Curriculum(
                query.ToInt(0),
                SdmCurriculumType.GetBy(query.ToInt(1)),
                query.ToInt(2),
                query.ToString(3),
                query.ToString(4),
                SdmCurriculumGroup.GetBy(query.ToInt(5))
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<Curriculum> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }
    public static List<Curriculum> GetAllBy(int curriculumTypeId)
    {
        var select = GetQueryObj();
        select.WhereEqual("CurriculumTypeId", curriculumTypeId.ToString());

        var result = ProcessQuery(select, true);
        return result;
    }
    public static Curriculum? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("Id", id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static void Insert(Curriculum curriculum)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("CurriculumTypeId", curriculum.Type?.Id.ToString());
        insert.Insert("Year", curriculum.Year.ToString());
        insert.Insert("NameTh", curriculum.NameTh);
        insert.Insert("NameEn", curriculum.NameEn);
        insert.Insert("CurriculumGroupId", curriculum.Group?.Id.ToString());

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    public static void UpdateBy(Curriculum curriculum)
    {
        var update = new SdmMysqlQueryUpdate(TableName);

        update.Set("CurriculumTypeId", curriculum.Type?.Id.ToString());
        update.Set("Year", curriculum.Year.ToString());
        update.Set("NameTh", curriculum.NameTh);
        update.Set("NameEn", curriculum.NameEn);
        update.Set("CurriculumGroupId", curriculum.Group?.Id.ToString());

        update.WhereEqual("Id", curriculum.Id.ToString());

        var query = SdmMysqlQuery.Execute(update);
        query.CleanUp();
    }
}