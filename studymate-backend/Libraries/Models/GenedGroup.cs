namespace studymate_backend.Libraries.Models;

public class GenedGroup(
    string id,
    string groupName
) : IBaseModel
{
    public string id { get; set; } = id;
    public string groupName { get; set; } = groupName;
}
