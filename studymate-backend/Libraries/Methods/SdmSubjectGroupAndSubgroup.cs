using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmSubjectGroupAndSubgroup
{
    public static string TableNameSubject => "curriculum_subject";
    public static string TableNameGroup => "curriculum_group";
    public static string TableNameSubgroup => "curriculum_subgroup";

    public static SdmPgsqlQuerySelect GetQueryObj(string tableName)
    {
        return new SdmPgsqlQuerySelect(tableName);
    }

    public static (string groupName, string subgroupName)? GetSubjectGroupAndSubgroupBySubjectId(string subjectId, string uniqueId, string year)
    {
    // Query curriculum_subject
        var subjectQuery = new SdmPgsqlQuerySelect("curriculum_subject")
            .AddWhereCondition("subject_id", subjectId)
            .AddWhereCondition("unique_id", uniqueId)
            .AddWhereCondition("year", year);

        var subjectResult = SdmPgsqlQuery.Execute(subjectQuery);
        if (!subjectResult.Next())
        {
            subjectResult.CleanUp();
            return null;
        }

        var categoryId = subjectResult.ToInt(1); // category_id เป็น smallint
        var groupId = subjectResult.ToInt(2);    // group_id เป็น smallint
        var subgroupId = subjectResult.ToInt(3); // subgroup_id เป็น smallint

        // Query curriculum_group
        var groupQuery = new SdmPgsqlQuerySelect("curriculum_group")
            .AddWhereCondition("category_id", categoryId.ToString())
            .AddWhereCondition("group_id", groupId.ToString())
            .AddWhereCondition("unique_id", uniqueId)
            .AddWhereCondition("year", year);

        var groupResult = SdmPgsqlQuery.Execute(groupQuery);
        string? groupName = null;
        if (groupResult.Next())
        {
            groupName = groupResult.ToString(4); // group_name เป็น varchar
        }
        groupResult.CleanUp();

        // Query curriculum_subgroup
        var subgroupQuery = new SdmPgsqlQuerySelect("curriculum_subgroup")
            .AddWhereCondition("category_id", categoryId.ToString())
            .AddWhereCondition("group_id", groupId.ToString())
            .AddWhereCondition("subgroup_id", subgroupId.ToString())
            .AddWhereCondition("unique_id", uniqueId)
            .AddWhereCondition("year", year);

        var subgroupResult = SdmPgsqlQuery.Execute(subgroupQuery);
        string? subgroupName = null;
        if (subgroupResult.Next())
        {
            subgroupName = subgroupResult.ToString(5); // subgroup_name เป็น varchar
        }
        subgroupResult.CleanUp();

        if (groupName == null && subgroupName == null)
        {
            return null;
        }

        return (groupName, subgroupName);
        } 
}
