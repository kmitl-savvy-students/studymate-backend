namespace studymate_backend.Libraries.Models;

public class SubjectClass(
    Subject subject,
    string classLevel,
    List<CurriculumGroup> groupName,
    int section,
    string creditLps,
    string buildingName,
    string roomNumber,
    List<string> teacherListTh,
    List<string> teacherListEn,
    List<string> classDatetime,
    List<string> midtermDatetime,
    List<string> finalDatetime,
    double rating,
    int review,
    string sessionType,
    string rule,
    string remark
) : IBaseModel
{
    public Subject Subject { get; set; } = subject;
    public string ClassLevel { get; set; } = classLevel;
    public List<CurriculumGroup> GroupName { get; set; } = groupName;
    public int Section { get; set; } = section;
    public string CreditLps { get; set; } = creditLps;
    public string BuildingName { get; set; } = buildingName;
    public string RoomNumber { get; set; } = roomNumber;
    public List<string> TeacherListTh { get; set; } = teacherListTh;
    public List<string> TeacherListEn { get; set; } = teacherListEn;
    public List<string> ClassDatetime { get; set; } = classDatetime;
    public List<string> MidtermDatetime { get; set; } = midtermDatetime;
    public List<string> FinalDatetime { get; set; } = finalDatetime;
    public double Rating { get; set; } = rating;
    public int Review { get; set; } = review;
    public string SessionType { get; set; } = sessionType;
    public string Rule { get; set; } = rule;
    public string Remark { get; set; } = remark;
}