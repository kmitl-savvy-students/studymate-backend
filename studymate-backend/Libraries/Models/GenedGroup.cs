namespace studymate_backend.Libraries.Models;

public class GenedGroup(
    string id,
    string group_name
) : IBaseModel
{
    public string id { get; set; } = id;
    public string group_name { get; set; } = group_name;
}