using studymate_backend.Libraries.Models;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmSubjectClass
{
    private const string KmitlPublicApiUrl = "https://regis.reg.kmitl.ac.th/api/";

    private static readonly HttpClient HttpClient = new();
    
    public static async Task<List<SubjectClass>> GetAllBy(
    Teachtable? inputTeachtable,
    Models.Program? program,
    string year
    )
    {
        if (inputTeachtable == null || program == null)
        {
            Console.WriteLine("❌ Invalid parameters: inputTeachtable or program is null.");
            return new List<SubjectClass>();
        }

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
            Console.WriteLine($"🌍 Fetching Public API: {apiUrl}");

            var response = await HttpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📩 Raw JSON Response: {responseContent}");

            var kmitlResponses = JsonSerializer.Deserialize<List<DtoKmitlResponse>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (kmitlResponses == null || !kmitlResponses.Any())
            {
                Console.WriteLine("❌ Deserialization failed or empty data.");
                return new List<SubjectClass>();
            }

            Console.WriteLine($"✅ Deserialized DtoKmitlResponses count: {kmitlResponses.Count}");

            var subjectClasses = new List<SubjectClass>();

            foreach (var faculty in kmitlResponses)
            {
                Console.WriteLine($"🏛 Processing Faculty: faculty_id({faculty.faculty_id})" + 
                                  $"department_id({faculty.department_id}) " +
                                  $"curriculum2_id({faculty.curriculum2_id}) " +
                                  $"class({faculty.classYear}) " +
                                  $"faculty_name_th({faculty.faculty_name_th}) " +
                                  $"facutly_name_en({faculty.faculty_name_en}) " +
                                  $"department_name_th({faculty.department_name_th}) " +
                                  $"department_name_en({faculty.department_name_en})" +
                                  $"curriculum_name_th({faculty.curriculum_name_th})" +
                                  $"curriculum_name_en({faculty.curriculum_name_en})"
                                  );

                if (faculty.Teachtables == null || !faculty.Teachtables.Any())
                {
                    Console.WriteLine("❌ No Teachtables found. in Faculty.");
                    continue;
                }

                foreach (var teachtable in faculty.Teachtables)
                {
                    Console.WriteLine($"📚 Processing Teachtable: subject_type_name_th({teachtable.subject_type_name_th}) " +
                                      $"subject_type_name_en({teachtable.subject_type_name_en})");

                    if (teachtable.Data == null || !teachtable.Data.Any())
                    {
                        Console.WriteLine("❌ No Subjects found in Teachtable.");
                        continue;
                    }

                    foreach (var subject in teachtable.Data)
                    {
                        Console.WriteLine($"📖 Processing Subject: {subject.subject_id} - {subject.subject_name_th}");

                        // ป้องกัน null
                        int.TryParse(subject.credit, out var creditValue);
                        int.TryParse(subject.section, out var sectionValue);

                        var subjectClass = new SubjectClass(
                            subject: new Subject(
                                subject.subject_id ?? "ไม่ระบุ",
                                subject.subject_name_th.Trim() ?? "ไม่ระบุ",
                                subject.subject_name_en.Trim() ?? "ไม่ระบุ",
                                creditValue,
                                ""
                            ),
                            classLevel: faculty.classYear ?? "ไม่ระบุ",
                            groupName: new List<string> {},
                            section: sectionValue,
                            
                            creditLPS: subject.credit_lps,
                            
                            buildingName: subject.classbuilding ?? "ไม่ระบุ",
                            roomNumber: subject.room_no ?? "ไม่ระบุ",
                            teacherListTh: TransformTeacherList(subject.teacher_list_th),  
                            teacherListEn: TransformTeacherList(subject.teacher_list_en),
                            classDatetime: TransformClassDatetime(subject.classdatetime),
                            midtermDatetime: TransformDateTime(subject.midterm_start_date_time, subject.midterm_end_date_time, "กลางภาค"),
                            finalDatetime: TransformDateTime(subject.final_start_date_time, subject.final_end_date_time, "ปลายภาค"),
                            rating: 0,
                            sessionType: subject.lect_or_prac == "ท" ? "ทฤษฎี" : subject.lect_or_prac == "ป" ? "ปฏิบัติ" : subject.lect_or_prac,
                            rule: subject.rule,
                            remark: subject.remark
                        );

                        subjectClasses.Add(subjectClass);
                    }
                }
            }

            Console.WriteLine($"✅ Total SubjectClass created: {subjectClasses.Count}");
            return subjectClasses;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔥 Error fetching public API: {ex.Message}");
            return new List<SubjectClass>();
        }
    }
    
    private static List<string> TransformTeacherList(string rawTeacherList)
    {
        if (string.IsNullOrWhiteSpace(rawTeacherList)) return new List<string>();

        var cleanTeacherList = Regex.Replace(rawTeacherList, "<.*?>", "\n");
        var teachers = cleanTeacherList.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        return teachers.Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
    }
    
    private static List<string> TransformClassDatetime(string rawDatetime)
    {
        if (string.IsNullOrWhiteSpace(rawDatetime))
        {
            Console.WriteLine("Warning: Received empty or null 'classdatetime'.");
            return new List<string> { "ไม่ระบุ" };
        }

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
            var parts = rawDatetime.Split(new[] { "<div>", "</div>", "+" }, StringSplitOptions.RemoveEmptyEntries);

            string? currentDay = null;
            foreach (var part in parts)
            {
                var dayMatch = dayMapping.FirstOrDefault(m => part.StartsWith(m.Key));
                if (dayMatch.Key != null)
                {
                    currentDay = dayMatch.Value;
                    if (!result.Contains(currentDay))
                    {
                        result.Add(currentDay);
                    }

                    var timePart = part.Replace(dayMatch.Key, "").Trim();
                    if (!string.IsNullOrWhiteSpace(timePart))
                    {
                        result.Add(timePart);
                    }
                }
                else if (currentDay != null)
                {
                    var cleanedPart = part.Trim();
                    foreach (var dayKey in dayMapping.Keys)
                    {
                        if (cleanedPart.StartsWith(dayKey))
                        {
                            cleanedPart = cleanedPart.Replace(dayKey, "").Trim();
                            break;
                        }
                    }
                    result.Add(cleanedPart);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error transforming 'classdatetime': {ex.Message}");
            result.Add("ข้อมูลเวลาไม่ถูกต้อง");
        }

        return result;
    }

    public class DtoKmitlResponse
    {
        public string? faculty_id { get; init; }
        public string? department_id { get; init; }
        public string? curriculum2_id { get; init; }
        
        [JsonPropertyName("class")]
        public string? classYear { get; init; } // ใช้ชื่ออื่นใน C# แต่แมปกับ "class" ใน JSON
        
        public string? faculty_name_th { get; init; }
        public string? faculty_name_en { get; init; }
        public string? department_name_th { get; init; }
        public string? department_name_en { get; init; }
        public string? curriculum_name_th { get; init; }
        public string? curriculum_name_en { get; init; }
        // public List<DtoKmitlTeachtable>? Teachtables { get; init; }
        
        [JsonPropertyName("teachtable")]  // เปลี่ยนจาก "teachtables" เป็น "teachtable"
        public List<DtoKmitlTeachtable>? Teachtables { get; init; }
    }
    
    public class DtoKmitlTeachtable
    {
        public string? subject_type_name_th { get; init; }
        public string? subject_type_name_en { get; init; }
        public List<DtoKmitlTeachtableSubject>? Data { get; init; }
    }
    
    public class DtoKmitlTeachtableSubject
    {
        public string? teach_table_id { get; init; }
        public string? subject_id { get; init; }
        public string? subject_name_th { get; init; }
        public string? subject_name_en { get; init; }
        public string? credit { get; init; }
        public string? credit_lps { get; init; }
        public string? credit_str { get; init; }
        public string? section { get; init; }
        public string? sec_pair { get; init; }
        public string? lect_or_prac { get; init; }
        public string? classdatetime { get; init; }
        public string? classroom { get; init; }
        public string? room_no { get; init; }
        public string? classbuilding { get; init; }
        public string? building_no { get; init; }
        public string? teacher_list_th { get; init; }
        public string? teacher_list_en { get; init; }
        public string? midterm_start_date_time { get; init; }
        public string? midterm_end_date_time { get; init; }
        public string? final_start_date_time { get; init; }
        public string? final_end_date_time { get; init; }
        public string? exam_text_detail { get; init; }
        public string? rule { get; init; }
        public string? remark { get; init; }
        public string? closed { get; init; }
        public string? limit { get; init; }
        public int pre_count { get; init; }
        public int queue_left { get; init; }
        public int count { get; init; }
        public int class_group_display { get; init; }
    }
    
    public static async Task<string> GetPublicAPI(
        Teachtable? teachtable,
        Models.Program? program,
        string year
    )
    {
        if (teachtable == null || program == null)
            return "Invalid parameters: teachtable or program is null.";

        var apiUrl = $"{KmitlPublicApiUrl}?" +
                     $"function=get-teach-table-show" +
                     $"&mode=by_class" +
                     $"&selected_year={teachtable.Year + 543}" +
                     $"&selected_semester={teachtable.Term}" +
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
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
    
    private static List<string> TransformDateTime(string startDateTime, string endDateTime, string phase)
    {
        if (string.IsNullOrWhiteSpace(startDateTime) || string.IsNullOrWhiteSpace(endDateTime))
        {
            return new List<string> { phase, "ไม่ระบุ", "ไม่ระบุ" };
        }

        var start = DateTime.Parse(startDateTime);
        var end = DateTime.Parse(endDateTime);

        // แปลงชื่อวันและลบคำว่า "วัน" ออก
        var dayOfWeek = start.ToString("dddd", new System.Globalization.CultureInfo("th-TH")).Replace("วัน", "");
        var date = start.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("th-TH"));
        var timeRange = $"{start:HH:mm}-{end:HH:mm}";

        return new List<string> { phase, $"{dayOfWeek} {date}", timeRange };
    }

}