using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract partial class SdmSubjectClass
{
    private const string KmitlPublicApiUrl = "https://regis.reg.kmitl.ac.th/api/";
    private const string KmitlSubjectApiUrl = "https://api.reg.kmitl.ac.th/subject/";

    private static readonly HttpClient HttpClient = new();

    public static async Task<Subject?> GetBy(string subjectId)
    {
        var apiUrl = $"{KmitlSubjectApiUrl}?" +
                     $"function=get-registrar-subject" +
                     $"&level_id=1" +
                     $"&subject_id={subjectId}";

        Console.WriteLine(apiUrl);

        var subject = SdmSubject.GetBy(subjectId);
        if (subject != null)
            return subject;

        try
        {
            var response = await HttpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var kmitlSubjects = JsonSerializer.Deserialize<List<DtoKmitlSubject>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (kmitlSubjects == null || kmitlSubjects.Count == 0)
                return null;

            subject = new Subject(
                subjectId,
                kmitlSubjects[0].SubjectNameTh ?? "",
                kmitlSubjects[0].SubjectNameEn ?? "",
                int.Parse(kmitlSubjects[0].Credit ?? "0"),
                kmitlSubjects[0].Detail ?? ""
            );
            SdmSubject.Insert(subject);
            return subject;
        }
        catch (Exception)
        {
            return null;
        }
    }
    public static async Task<bool> GetBy(
        Teachtable? inputTeachtable,
        Models.Program? program,
        string subjectId
    )
    {
        if (inputTeachtable == null || program == null)
            return false;

        var apiUrl = $"{KmitlPublicApiUrl}?" +
                     $"function=get-teach-table-show" +
                     $"&mode=by_subject_id" +
                     $"&selected_year={inputTeachtable.Year + 543}" +
                     $"&selected_semester={inputTeachtable.Term}" +
                     $"&selected_faculty={program.Department?.Faculty?.KmitlId}" +
                     $"&selected_department={program.Department?.KmitlId}" +
                     $"&selected_curriculum={program.KmitlId}" +
                     $"&search_all_faculty=false" +
                     $"&search_all_department=false" +
                     $"&search_all_curriculum=false" +
                     $"&search_all_class_year=true" +
                     $"&selected_subject_id={subjectId}";
        try
        {
            var response = await HttpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var kmitlResponses = JsonSerializer.Deserialize<List<DtoKmitlResponse>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (kmitlResponses == null || kmitlResponses.Count == 0)
                return false;

            foreach (var faculty in kmitlResponses)
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
        }
        catch (Exception)
        {
            return false;
        }

        return false;
    }
    public static async Task<SubjectClass?> GetBy(
        Teachtable? inputTeachtable,
        Models.Program? program,
        string subjectId, string section
    )
    {
        if (inputTeachtable == null || program == null)
            return null;

        var apiUrl = $"{KmitlPublicApiUrl}?" +
                     $"function=get-teach-table-show" +
                     $"&mode=by_subject_id" +
                     $"&selected_year={inputTeachtable.Year + 543}" +
                     $"&selected_semester={inputTeachtable.Term}" +
                     $"&selected_faculty={program.Department?.Faculty?.KmitlId}" +
                     $"&selected_department={program.Department?.KmitlId}" +
                     $"&selected_curriculum={program.KmitlId}" +
                     $"&search_all_faculty=false" +
                     $"&search_all_department=false" +
                     $"&search_all_curriculum=false" +
                     $"&search_all_class_year=true" +
                     $"&selected_subject_id={subjectId}";
        try
        {
            var response = await HttpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var kmitlResponses = JsonSerializer.Deserialize<List<DtoKmitlResponse>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (kmitlResponses == null || kmitlResponses.Count == 0)
                return null;

            foreach (var faculty in kmitlResponses)
            {
                if (faculty.Teachtables == null || faculty.Teachtables.Count == 0)
                    continue;

                foreach (var teachtable in faculty.Teachtables)
                {
                    if (teachtable.Data == null || teachtable.Data.Count == 0)
                        continue;

                    foreach (var teachtableSubject in teachtable.Data.Where(teachtableSubject => teachtableSubject.Section == section))
                        return new SubjectClass(
                            SdmSubject.GetBy(teachtableSubject.SubjectId),
                            faculty.Class ?? "ไม่ระบุ", [],
                            int.Parse(teachtableSubject.Section ?? "0"),
                            teachtableSubject.CreditLps ?? "ไม่ระบุ",
                            teachtableSubject.ClassBuilding ?? "ไม่ระบุ",
                            teachtableSubject.RoomNumber ?? "ไม่ระบุ",
                            TransformTeacherList(teachtableSubject.TeacherListTh),
                            TransformTeacherList(teachtableSubject.TeacherListEn),
                            TransformClassDatetime(teachtableSubject.ClassDateTime),
                            TransformDateTime(teachtableSubject.MidtermStartDateTime, teachtableSubject.MidtermEndDateTime, "กลางภาค"),
                            TransformDateTime(teachtableSubject.FinalStartDateTime, teachtableSubject.FinalEndDateTime, "ปลายภาค"),
                            0,
                            teachtableSubject.SessionType switch
                            {
                                "ท" => "ทฤษฎี",
                                "ป" => "ปฏิบัติ",
                                _ => teachtableSubject.SessionType ?? "ไม่ระบุ"
                            },
                            teachtableSubject.Rule, teachtableSubject.Remark
                        );
                }
            }
        }
        catch (Exception)
        {
            return null;
        }

        return null;
    }

    public static async Task<List<SubjectClass>> GetAllBy(
        Teachtable? inputTeachtable,
        Models.Program? program,
        string year
    )
    {
        if (inputTeachtable == null || program == null)
            return [];

        var apiUrl = $"{KmitlPublicApiUrl}?" +
                     $"function=get-teach-table-show" +
                     $"&mode=by_class" +
                     $"&selected_year={inputTeachtable.Year + 543}" +
                     $"&selected_semester={inputTeachtable.Term}" +
                     $"&selected_class_year={year}" +
                     $"&selected_faculty={program.Department?.Faculty?.KmitlId}" +
                     $"&selected_department={program.Department?.KmitlId}" +
                     $"&selected_curriculum={program.KmitlId}" +
                     $"&search_all_faculty=false" +
                     $"&search_all_department=false" +
                     $"&search_all_curriculum=false" +
                     $"&search_all_class_year={(year == "0" ? "true" : "false")}";
        try
        {
            var response = await HttpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var kmitlResponses = JsonSerializer.Deserialize<List<DtoKmitlResponse>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (kmitlResponses == null || kmitlResponses.Count == 0)
                return [];

            var subjectClasses = new List<SubjectClass>();
            foreach (var faculty in kmitlResponses)
            {
                if (faculty.Teachtables == null || faculty.Teachtables.Count == 0)
                    continue;

                foreach (var teachtable in faculty.Teachtables)
                {
                    if (teachtable.Data == null || teachtable.Data.Count == 0)
                        continue;

                    subjectClasses.AddRange(
                        teachtable.Data.Select(teachtableSubject => new SubjectClass(
                            SdmSubject.GetBy(teachtableSubject.SubjectId),
                            faculty.Class ?? "ไม่ระบุ", [],
                            int.Parse(teachtableSubject.Section ?? "0"),
                            teachtableSubject.CreditLps ?? "ไม่ระบุ",
                            teachtableSubject.ClassBuilding ?? "ไม่ระบุ",
                            teachtableSubject.RoomNumber ?? "ไม่ระบุ",
                            TransformTeacherList(teachtableSubject.TeacherListTh),
                            TransformTeacherList(teachtableSubject.TeacherListEn),
                            TransformClassDatetime(teachtableSubject.ClassDateTime),
                            TransformDateTime(teachtableSubject.MidtermStartDateTime, teachtableSubject.MidtermEndDateTime, "กลางภาค"),
                            TransformDateTime(teachtableSubject.FinalStartDateTime, teachtableSubject.FinalEndDateTime, "ปลายภาค"),
                            0,
                            teachtableSubject.SessionType switch
                            {
                                "ท" => "ทฤษฎี",
                                "ป" => "ปฏิบัติ",
                                _ => teachtableSubject.SessionType ?? "ไม่ระบุ"
                            }, teachtableSubject.Rule, teachtableSubject.Remark)
                        )
                    );
                }
            }

            return subjectClasses;
        }
        catch (Exception)
        {
            return [];
        }
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

    private static List<string> TransformDateTime(string? startDateTime, string? endDateTime, string phase)
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
        public required string? MidtermStartDateTime { get; init; } = string.Empty;

        [JsonPropertyName("midterm_end_date_time")]
        public required string? MidtermEndDateTime { get; init; } = string.Empty;

        [JsonPropertyName("final_start_date_time")]
        public required string? FinalStartDateTime { get; init; } = string.Empty;

        [JsonPropertyName("final_end_date_time")]
        public required string? FinalEndDateTime { get; init; } = string.Empty;

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
}