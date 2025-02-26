using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmCurriculumGroupSubject : ISdmBaseMethod<CurriculumGroupSubject>
{
    public static string TableName => "curriculum_group_subject";
    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }
    public static List<CurriculumGroupSubject> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<CurriculumGroupSubject>();
        while (query.Next())
        {
            result.Add(new CurriculumGroupSubject(
                query.ToInt(0),
                SdmCurriculumGroup.GetBy(query.ToInt(1)),
                SdmSubject.GetBy(query.ToString(2))
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<CurriculumGroupSubject> GetAllBy(int curriculumGroupId)
    {
        var select = GetQueryObj();
        select.WhereEqual("cgs_cg_id", curriculumGroupId.ToString());

        var result = ProcessQuery(select, true);
        return result;
    }
    public static CurriculumGroupSubject? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("cgs_id", id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }
    public static CurriculumGroupSubject? GetBy(int curriculumGroupId, string subjectId)
    {
        var select = GetQueryObj();
        select.WhereEqual("cgs_cg_id", curriculumGroupId.ToString());
        select.WhereEqual("cgs_sbj_id", subjectId);

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static void Insert(CurriculumGroupSubject curriculumGroupSubject)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("cgs_cg_id", curriculumGroupSubject.Group?.Id.ToString());
        insert.Insert("cgs_sbj_id", curriculumGroupSubject.Subject?.Id);

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    public static void DeleteBy(int id)
    {
        var delete = new SdmMysqlQueryDelete(TableName);

        delete.WhereEqual("cgs_id", id.ToString());

        var query = SdmMysqlQuery.Execute(delete);
        query.CleanUp();
    }
    public static void DeleteBy(CurriculumGroup curriculumGroup)
    {
        var delete = new SdmMysqlQueryDelete(TableName);

        delete.WhereEqual("cgs_cg_id", curriculumGroup.Id.ToString());

        var query = SdmMysqlQuery.Execute(delete);
        query.CleanUp();
    }
}