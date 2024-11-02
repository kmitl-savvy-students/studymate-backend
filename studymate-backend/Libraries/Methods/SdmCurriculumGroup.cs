using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmCurriculumGroup : ISdmBaseMethod<CurriculumGroup>
{
    public static string TableName => "cu_curri_group";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<CurriculumGroup> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        // var query = SdmPgsqlQuery.Execute(queryBuilder);
        // var reader = query.GetReader();
        // if (reader == null)
        //     return [];
        //
        // var result = new List<CurriculumGroup>();
        //
        // while (reader.Read())
        // {
        //     var curriculumGroup = new CurriculumGroup(
        //         reader.GetInt32(0),
        //         reader.GetInt32(1),
        //         reader.GetString(2),
        //         reader.GetString(3),
        //         reader.GetString(4),
        //         reader.GetInt32(5),
        //         reader.GetInt32(6),
        //         reader.GetString(7),
        //         reader.GetString(8),
        //         reader.GetString(9)
        //     );
        //
        //     result.Add(curriculumGroup);
        //
        //     if (!isArray)
        //         return result;
        // }
        //
        // return result;
        return [];
    }

    public static List<CurriculumGroup> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }

    public static List<CurriculumGroup> GetByCatIdAndGroupId(int cCatId, int cGroupId)
    {
        var select = GetQueryObj();
        select.WhereEqual("c_cat_id", cCatId.ToString());
        select.WhereEqual("c_group_id", cGroupId.ToString());

        var result = ProcessQuery(select, true);
        if (result.Count == 0)
            return [];
        return result;
    }
}
