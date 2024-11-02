using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmCurriculumSubject : ISdmBaseMethod<CurriculumSubject>
{
    public static string TableName => "cu_curri_subject";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<CurriculumSubject> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        // var query = SdmPgsqlQuery.Execute(queryBuilder);
        // var reader = query.GetReader();
        // if (reader == null)
        //     return [];
        //
        // var result = new List<CurriculumSubject>();
        //
        // while (reader.Read())
        // {
        //     var curriculumSubject = new CurriculumSubject(
        //         reader.GetString(0),
        //         reader.GetInt32(1),
        //         reader.GetInt32(2),
        //         reader.GetInt32(3),
        //         reader.GetString(4),
        //         reader.GetString(5),
        //         reader.GetString(6),
        //         reader.GetString(7)
        //     );
        //
        //     result.Add(curriculumSubject);
        //
        //     if (!isArray)
        //         return result;
        // }
        //
        // return result;
        return [];
    }

    public static List<CurriculumSubject> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }

    public static CurriculumSubject? GetById(string subjectId)
    {
        var select = GetQueryObj();
        select.WhereEqual("subject_id", subjectId);

        var result = ProcessQuery(select, true);
        if (result.Count == 0)
            return null;
        return result[0];
    }
}
