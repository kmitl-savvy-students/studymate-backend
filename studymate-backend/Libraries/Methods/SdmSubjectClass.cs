using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public partial class SdmSubjectClass
{
    private const string KmitlPublicApiUrl = "https://regis.reg.kmitl.ac.th/api/";
    private const string KmitlSubjectApiUrl = "https://api.reg.kmitl.ac.th/subject/";

    private static readonly HttpClient HttpClient = new();

    #region Get Subjects
    public static async Task<Subject?> GetBy(string subjectId)
    {
        var apiUrl = $"{KmitlSubjectApiUrl}?" +
                     $"function=get-registrar-subject" +
                     $"&level_id=1" +
                     $"&subject_id={subjectId}";

        var subject = SdmSubject.GetBy(subjectId);
        if (subject != null)
            return subject;

        var responses = await GetDeserializedObjects<List<DtoKmitlSubject>>(apiUrl);
        if (responses == null || responses.Count == 0)
            return null;

        var teachtableSubject = responses[0];

        subject = new Subject(
            subjectId,
            teachtableSubject.SubjectNameTh ?? "ไม่มี",
            teachtableSubject.SubjectNameEn ?? "ไม่มี",
            int.Parse(teachtableSubject.Credit ?? "0"),
            teachtableSubject.Detail ?? "ไม่มี"
        );
        SdmSubject.Insert(subject);

        return subject;
    }
    public static async Task<bool> GetBy(
        Teachtable inputTeachtable,
        Curriculum curriculum,
        string subjectId, string isGened
    )
    {
        var apiUrl = $"{KmitlPublicApiUrl}?" +
                     $"function=get-teach-table-show" +
                     $"&mode=by_subject_id" +
                     $"&selected_year={inputTeachtable.Year + 543}" +
                     $"&selected_semester={inputTeachtable.Term}" +
                     $"&selected_faculty={curriculum.Program?.Department?.Faculty?.KmitlId}" +
                     $"&selected_department={curriculum.Program?.Department?.KmitlId}" +
                     $"&selected_curriculum={(isGened == "1" ? "x" : curriculum.Program?.KmitlId)}" +
                     $"&search_all_faculty=false" +
                     $"&search_all_department=false" +
                     $"&search_all_curriculum=false" +
                     $"&search_all_class_year=true" +
                     $"&selected_subject_id={subjectId}";

        var responses = await GetDeserializedObjects<List<DtoKmitlResponse>>(apiUrl);
        if (responses == null || responses.Count == 0)
            return false;

        foreach (var faculty in responses)
        {
            if (faculty.Teachtables == null || faculty.Teachtables.Count == 0)
                continue;

            foreach (var teachtable in faculty.Teachtables)
            {
                if (teachtable.Data == null || teachtable.Data.Count == 0)
                    continue;

                return true;
            }
        }

        return false;
    }
    public static async Task<SubjectClass?> GetBy(
        Teachtable inputTeachtable,
        Curriculum curriculum,
        string subjectId, string section, string isGened
    )
    {
        var apiUrl = $"{KmitlPublicApiUrl}?" +
                     $"function=get-teach-table-show" +
                     $"&mode=by_subject_id" +
                     $"&selected_year={inputTeachtable.Year + 543}" +
                     $"&selected_semester={inputTeachtable.Term}" +
                     $"&selected_faculty={curriculum.Program?.Department?.Faculty?.KmitlId}" +
                     $"&selected_department={curriculum.Program?.Department?.KmitlId}" +
                     $"&selected_curriculum={(isGened == "1" ? "x" : curriculum.Program?.KmitlId)}" +
                     $"&search_all_faculty=false" +
                     $"&search_all_department=false" +
                     $"&search_all_curriculum=false" +
                     $"&search_all_class_year=true" +
                     $"&selected_subject_id={subjectId}";

        var responses = await GetDeserializedObjects<List<DtoKmitlResponse>>(apiUrl);
        if (responses == null || responses.Count == 0)
            return null;

        foreach (var faculty in responses)
        {
            if (faculty.Teachtables == null || faculty.Teachtables.Count == 0)
                continue;

            foreach (var teachtable in faculty.Teachtables)
            {
                if (teachtable.Data == null || teachtable.Data.Count == 0)
                    continue;

                foreach (var teachtableSubject in teachtable.Data)
                {
                    if (teachtableSubject.Section != section)
                        continue;

                    var subjectClass = CreateSubjectClass(
                        teachtableSubject,
                        faculty.Class,
                        curriculum.CurriculumGroup
                    );
                    if (subjectClass == null)
                        continue;

                    return subjectClass;
                }
            }
        }

        return null;
    }
    public static async Task<List<SubjectClass>> GetAllBy(
        Teachtable inputTeachtable,
        Curriculum curriculum,
        string year,
        string isGened
    )
    {
        var apiUrl = $"{KmitlPublicApiUrl}?" +
                     $"function=get-teach-table-show" +
                     $"&mode=by_class" +
                     $"&selected_year={inputTeachtable.Year + 543}" +
                     $"&selected_semester={inputTeachtable.Term}" +
                     $"&selected_class_year={year}" +
                     $"&selected_faculty={curriculum.Program?.Department?.Faculty?.KmitlId}" +
                     $"&selected_department={curriculum.Program?.Department?.KmitlId}" +
                     $"&selected_curriculum={(isGened == "1" ? "x" : curriculum.Program?.KmitlId)}" +
                     $"&search_all_faculty=false" +
                     $"&search_all_department=false" +
                     $"&search_all_curriculum=false" +
                     $"&search_all_class_year={(year == "0" ? "true" : "false")}";

        var responses = await GetDeserializedObjects<List<DtoKmitlResponse>>(apiUrl);
        if (responses == null || responses.Count == 0)
            return [];

        var results = new List<SubjectClass>();
        foreach (var faculty in responses)
        {
            if (faculty.Teachtables == null || faculty.Teachtables.Count == 0)
                continue;

            foreach (var teachtable in faculty.Teachtables)
            {
                if (teachtable.Data == null || teachtable.Data.Count == 0)
                    continue;

                foreach (var teachtableSubject in teachtable.Data)
                {
                    var subjectClass = CreateSubjectClass(
                        teachtableSubject,
                        faculty.Class,
                        curriculum.CurriculumGroup
                    );
                    if (subjectClass == null)
                        continue;

                    results.Add(subjectClass);
                }
            }
        }

        return results;
    }
    #endregion
    #region Helper Functions
    private static SubjectClass? CreateSubjectClass(
        DtoKmitlTeachtableSubject teachtableSubject,
        string? classYear,
        CurriculumGroup? curriculumGroup
    )
    {
        var subjectClassSubject = SdmSubject.GetBy(teachtableSubject.SubjectId);
        if (subjectClassSubject == null)
            return null;

        SdmCurriculumGroup.AssignColors(curriculumGroup);
        var subjectClassGroupNames = SdmCurriculumGroup.GetAllBy(subjectClassSubject, curriculumGroup);

        var subjectClassSection = int.Parse(teachtableSubject.Section ?? "0");

        var subjectClassClassYear = classYear ?? "ไม่ระบุ";
        var subjectClassCreditLps = teachtableSubject.CreditLps ?? "ไม่ระบุ";
        var subjectClassClassBuilding = teachtableSubject.ClassBuilding ?? "ไม่ระบุ";
        var subjectClassRoomNumber = teachtableSubject.RoomNumber ?? "ไม่ระบุ";
        var subjectClassRule = teachtableSubject.Rule ?? "ไม่ระบุ";
        var subjectClassRemark = teachtableSubject.Remark ?? "ไม่ระบุ";

        var subjectClassRating = SdmSubjectReview.GetAverageRatingOfReview(subjectClassSubject.Id);
        var subjectClassReview = SdmSubjectReview.GetCountOfReview(subjectClassSubject.Id);

        var subjectClassTeacherListTh = TransformTeacherList(teachtableSubject.TeacherListTh);
        var subjectClassTeacherListEn = TransformTeacherList(teachtableSubject.TeacherListEn);

        var subjectClassClassDatetime = TransformClassDatetime(teachtableSubject.ClassDateTime);
        var subjectClassMidtermDatetime = TransformDatetime(teachtableSubject.MidtermStartDatetime, teachtableSubject.MidtermEndDatetime, "กลางภาค");
        var subjectClassFinaltermDatetime = TransformDatetime(teachtableSubject.FinaltermStartDatetime, teachtableSubject.FinaltermEndDatetime, "ปลายภาค");

        var subjectClassSessionType = teachtableSubject.SessionType switch
        {
            "ท" => "ทฤษฎี",
            "ป" => "ปฏิบัติ",
            _ => teachtableSubject.SessionType ?? "ไม่ระบุ"
        };

        return new SubjectClass(
            subjectClassSubject, subjectClassClassYear, subjectClassGroupNames,
            subjectClassSection, subjectClassCreditLps, subjectClassClassBuilding,
            subjectClassRoomNumber, subjectClassTeacherListTh, subjectClassTeacherListEn,
            subjectClassClassDatetime, subjectClassMidtermDatetime, subjectClassFinaltermDatetime,
            subjectClassRating, subjectClassReview, subjectClassSessionType,
            subjectClassRule, subjectClassRemark
        );
    }
    private static async Task<T?> GetDeserializedObjects<T>(string apiUrl)
    {
        try
        {
            var response = await HttpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            var deserializedObject = JsonSerializer.Deserialize<T>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return deserializedObject;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[ERROR] HTTP Request Error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[ERROR] JSON Parsing Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Unexpected Error: {ex.Message}");
        }

        return default;
    }
    private static List<string> TransformTeacherList(string? rawTeacherList)
    {
        if (string.IsNullOrWhiteSpace(rawTeacherList))
            return [];

        var cleanTeacherList = RegexCleanTeacherList().Replace(rawTeacherList, "\n");
        var teachers = cleanTeacherList.Split(["\n"], StringSplitOptions.RemoveEmptyEntries);
        return teachers.Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
    }
    private static List<string> TransformClassDatetime(string? rawDatetime)
    {
        if (string.IsNullOrWhiteSpace(rawDatetime))
            return ["ไม่ระบุ"];

        var dayMapping = new Dictionary<string, string>
        {
            { "จ.", "จันทร์" },
            { "อ.", "อังคาร" },
            { "พ.", "พุธ" },
            { "พฤ.", "พฤหัสบดี" },
            { "ศ.", "ศุกร์" },
            { "ส.", "เสาร์" },
            { "อา.", "อาทิตย์" }
        };

        var result = new List<string>();

        try
        {
            var parts = rawDatetime.Split(["<div>", "</div>", "+"], StringSplitOptions.RemoveEmptyEntries);

            string? currentDay = null;
            foreach (var part in parts)
            {
                var dayMatch = dayMapping.FirstOrDefault(m => part.StartsWith(m.Key));
                if (dayMatch.Key != null)
                {
                    currentDay = dayMatch.Value;
                    if (!result.Contains(currentDay)) result.Add(currentDay);

                    var timePart = part.Replace(dayMatch.Key, "").Trim();
                    if (!string.IsNullOrWhiteSpace(timePart)) result.Add(timePart);
                }
                else if (currentDay != null)
                {
                    var cleanedPart = part.Trim();
                    foreach (var dayKey in dayMapping.Keys.Where(dayKey => cleanedPart.StartsWith(dayKey)))
                    {
                        cleanedPart = cleanedPart.Replace(dayKey, "").Trim();
                        break;
                    }

                    result.Add(cleanedPart);
                }
            }
        }
        catch (Exception)
        {
            result.Add("ข้อมูลเวลาไม่ถูกต้อง");
        }

        return result;
    }
    private static List<string> TransformDatetime(string? startDateTime, string? endDateTime, string phase)
    {
        if (string.IsNullOrWhiteSpace(startDateTime) || string.IsNullOrWhiteSpace(endDateTime))
            return [phase, "ไม่ระบุ", "ไม่ระบุ"];

        var start = DateTime.Parse(startDateTime);
        var end = DateTime.Parse(endDateTime);

        var dayOfWeek = start.ToString("dddd", new CultureInfo("th-TH")).Replace("วัน", "");
        var date = start.ToString("d MMMM yyyy", new CultureInfo("th-TH"));
        var timeRange = $"{start:HH:mm}-{end:HH:mm}";

        return [phase, $"{dayOfWeek} {date}", timeRange];
    }
    [GeneratedRegex("<.*?>")]
    private static partial Regex RegexCleanTeacherList();
    #endregion
    #region Dto Classes
    public class DtoKmitlResponse
    {
        [JsonPropertyName("class")] public required string? Class { get; init; } = string.Empty;
        [JsonPropertyName("teachtable")] public required List<DtoKmitlTeachtable>? Teachtables { get; init; } = [];
    }

    public class DtoKmitlTeachtable
    {
        [JsonPropertyName("data")] public required List<DtoKmitlTeachtableSubject>? Data { get; init; } = [];
    }

    public class DtoKmitlTeachtableSubject
    {
        [JsonPropertyName("subject_id")] public required string? SubjectId { get; init; } = string.Empty;
        [JsonPropertyName("credit_lps")] public required string? CreditLps { get; init; } = string.Empty;
        [JsonPropertyName("section")] public required string? Section { get; init; } = string.Empty;
        [JsonPropertyName("lect_or_prac")] public required string? SessionType { get; init; } = string.Empty;
        [JsonPropertyName("room_no")] public required string? RoomNumber { get; init; } = string.Empty;
        [JsonPropertyName("classdatetime")] public required string? ClassDateTime { get; init; } = string.Empty;
        [JsonPropertyName("classbuilding")] public required string? ClassBuilding { get; init; } = string.Empty;
        [JsonPropertyName("teacher_list_th")] public required string? TeacherListTh { get; init; } = string.Empty;
        [JsonPropertyName("teacher_list_en")] public required string? TeacherListEn { get; init; } = string.Empty;

        [JsonPropertyName("midterm_start_date_time")]
        public required string? MidtermStartDatetime { get; init; } = string.Empty;

        [JsonPropertyName("midterm_end_date_time")]
        public required string? MidtermEndDatetime { get; init; } = string.Empty;

        [JsonPropertyName("final_start_date_time")]
        public required string? FinaltermStartDatetime { get; init; } = string.Empty;

        [JsonPropertyName("final_end_date_time")]
        public required string? FinaltermEndDatetime { get; init; } = string.Empty;

        [JsonPropertyName("rule")] public required string? Rule { get; init; } = string.Empty;
        [JsonPropertyName("remark")] public required string? Remark { get; init; } = string.Empty;
    }

    public class DtoKmitlSubject
    {
        [JsonPropertyName("subject_name_th")] public required string? SubjectNameTh { get; init; } = string.Empty;
        [JsonPropertyName("subject_name_en")] public required string? SubjectNameEn { get; init; } = string.Empty;
        [JsonPropertyName("credit")] public required string? Credit { get; init; } = string.Empty;
        [JsonPropertyName("detail")] public required string? Detail { get; init; } = string.Empty;
    }
    #endregion
}