namespace studymate_backend.Libraries.Models;

public class SubjectClass(
    Subject subject,

    string classLevel,
    List<string> groupName,

    int section,
    string? buildingName,  // classbuilding
    string? roomNumber,    // room_no

    List<string> teacherListTh,
    List<string> teacherListEn,

    List<string> classDatetime,   // classdatetime
    List<string> midtermDatetime, // midterm_date_time
    List<string> finalDatetime,   // final_date_time

    double rating,

    string sessionType,  // Previously lect_or_prac
    string? rule,
    string remark
) : IBaseModel
{
    public Subject Subject { get; set; } = subject;

    public string ClassLevel { get; set; } = classLevel;
    public List<string> GroupName { get; set; } = groupName;

    public int Section { get; set; } = section;
    public string? BuildingName { get; set; } = buildingName;
    public string? RoomNumber { get; set; } = roomNumber;

    public List<string> TeacherListTh { get; set; } = teacherListTh;
    public List<string> TeacherListEn { get; set; } = teacherListEn;

    public List<string> ClassDatetime { get; set; } = classDatetime;
    public List<string> MidtermDatetime { get; set; } = midtermDatetime;
    public List<string> FinalDatetime { get; set; } = finalDatetime;

    public double Rating { get; set; } = rating;

    public string SessionType { get; set; } = sessionType; // Improved naming from lect_or_prac
    public string? Rule { get; set; } = rule;
    public string Remark { get; set; } = remark;
}