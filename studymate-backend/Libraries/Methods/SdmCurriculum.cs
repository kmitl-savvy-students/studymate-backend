using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmCurriculum : ISdmBaseMethod<Curriculum>
{
    public static string TableName => "curriculum";
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
                SdmProgram.GetBy(query.ToInt(1)),
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
    public static List<Curriculum> GetAllBy(int programId)
    {
        var select = GetQueryObj();
        select.WhereEqual("curr_prog_id", programId.ToString());

        var result = ProcessQuery(select, true);
        return result;
    }
    public static Curriculum? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("curr_id", id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static void Insert(Curriculum curriculum)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("curr_prog_id", curriculum.Program?.Id.ToString());
        insert.Insert("curr_year", curriculum.Year.ToString());
        insert.Insert("curr_name_th", curriculum.NameTh);
        insert.Insert("curr_name_en", curriculum.NameEn);
        insert.Insert("curr_cg_id", curriculum.CurriculumGroup?.Id.ToString());

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    public static void UpdateBy(Curriculum curriculum)
    {
        var update = new SdmMysqlQueryUpdate(TableName);

        update.Set("curr_prog_id", curriculum.Program?.Id.ToString());
        update.Set("curr_year", curriculum.Year.ToString());
        update.Set("curr_name_th", curriculum.NameTh);
        update.Set("curr_name_en", curriculum.NameEn);
        update.Set("curr_cg_id", curriculum.CurriculumGroup?.Id.ToString());

        update.WhereEqual("curr_id", curriculum.Id.ToString());

        var query = SdmMysqlQuery.Execute(update);
        query.CleanUp();
    }
}