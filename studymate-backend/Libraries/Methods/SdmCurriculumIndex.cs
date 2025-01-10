using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmCurriculumIndex
{
    
    public static string TableName => "cu_curriculum_index";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }
    
    public static List<CurriculumIndex> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);

        var result = new List<CurriculumIndex>();

        while (query.Next())
        {
            result.Add(new CurriculumIndex(
                query.ToString(0),
                query.ToString(1),
                query.ToString(2),
                query.ToString(3),
                query.ToString(4),
                query.ToString(5),
                query.ToString(6),
                query.ToInt(7)
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }
    
    public static List<CurriculumIndex> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }
    
    public static List<CurriculumIndex> GetById(string id)
    {
        var select = GetQueryObj();
        select.WhereEqual("curri_id", id);

        var result = ProcessQuery(select, true);
        if (result.Count == 0)
            return null;
        return result;
    }

    
    public static List<CurriculumIndex> GetByFacultyId(string id)
    {
        var select = GetQueryObj();
        select.WhereEqual("faculty_id", id);

        var result = ProcessQuery(select, true);
        if (result.Count == 0)
            return null;
        return result;
    }

    
}