using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmCurriculumGroup : ISdmBaseMethod<CurriculumGroup>
{
    public static string TableName => "curriculum_group";
    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }
    public static List<CurriculumGroup> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<CurriculumGroup>();
        while (query.Next())
        {
            result.Add(new CurriculumGroup(
                query.ToInt(0),
                query.ToInt(1),
                query.ToString(2),
                query.ToString(3),
                query.ToInt(4),
                GetAllBy(query.ToInt(0))
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static List<CurriculumGroup> GetAllBy(int parentId)
    {
        var select = GetQueryObj();
        select.WhereEqual("cg_cg_id", parentId.ToString());

        var result = ProcessQuery(select, true);
        return result;
    }
    public static CurriculumGroup? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("cg_id", id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static CurriculumGroup Insert(CurriculumGroup curriculumGroup)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("cg_cg_id", curriculumGroup.ParentId == -1 ? null : curriculumGroup.ParentId.ToString());
        insert.Insert("cg_type", curriculumGroup.Type);
        insert.Insert("cg_name", curriculumGroup.Name);
        insert.Insert("cg_credit", curriculumGroup.Credit.ToString());

        var query = SdmMysqlQuery.Execute(insert);
        curriculumGroup.Id = query.InsertedId;
        query.CleanUp();
        return curriculumGroup;
    }
    public static void UpdateBy(CurriculumGroup curriculumGroup)
    {
        var update = new SdmMysqlQueryUpdate(TableName);

        update.Set("cg_cg_id", curriculumGroup.ParentId.ToString());
        update.Set("cg_type", curriculumGroup.Type);
        update.Set("cg_name", curriculumGroup.Name);
        update.Set("cg_credit", curriculumGroup.Credit.ToString());

        update.WhereEqual("cg_id", curriculumGroup.Id.ToString());

        var query = SdmMysqlQuery.Execute(update);
        query.CleanUp();
    }
}