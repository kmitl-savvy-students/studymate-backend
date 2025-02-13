namespace studymate_backend.Libraries.Models;

public class SubjectClass(
    string id,
    Subject subject,
    
    string buildingId,
    string buildingName,
    string className,
    string roomName,
    string section,
    
    string remark,
    string rule
) :IBaseModel
{
    public string Id { get; } = id;
    public Subject Subject { get; set; } = subject;

    public string BuildingId { get; set; } = buildingId;
    public string BuildingName { get; set; } = buildingName;
    public string ClassName { get; set; } = className;
    public string RoomName { get; set; } = roomName;
    public string Section { get; set; } = section;
    
    public string Remark { get; set; } = remark;
    public string Rule { get; set; } = rule;
}