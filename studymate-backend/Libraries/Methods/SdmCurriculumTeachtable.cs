using System.Text.Json;

namespace studymate_backend.Libraries.Methods;

public class SdmCurriculumTeachtable
{
    private static readonly HttpClient HttpClient = new HttpClient();

    public static async Task<JsonElement> FetchFilteredTeachTableData(
        int selectedYear,
        int selectedSemester,
        string selectedFaculty,
        string selectedDepartment,
        string selectedCurriculum,
        int selectedClassYear,
        string curriculumYear,
        string uniqueId)
    {
        string apiUrl = $"https://k8s.reg.kmitl.ac.th/reg/api/?" +
                        $"function=get-teach-table-show&mode=by_class" +
                        $"&selected_year={selectedYear}" +
                        $"&selected_semester={selectedSemester}" +
                        $"&selected_faculty={selectedFaculty}" +
                        $"&selected_department={selectedDepartment}" +
                        $"&selected_curriculum={selectedCurriculum}" +
                        $"&selected_class_year={selectedClassYear}" +
                        $"&search_all_faculty=false" +
                        $"&search_all_department=false" +
                        $"&search_all_curriculum=false" +
                        $"&search_all_class_year={(selectedClassYear == 0 ? "true" : "false")}";

        try
        {
            Console.WriteLine($"Calling Public API: {apiUrl}");
            var response = await HttpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Public API Error: {response.StatusCode} - {response.ReasonPhrase}");
                throw new Exception($"Public API Error: {response.StatusCode}");
            }

            var data = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(data))
            {
                Console.WriteLine("Public API returned empty response.");
                throw new Exception("Public API returned empty response.");
            }

            Console.WriteLine($"Public API Response: {data}");
            var jsonDoc = JsonDocument.Parse(data);

            return await TransformData(jsonDoc.RootElement, curriculumYear, uniqueId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in FetchFilteredTeachTableData: {ex.Message}");
            throw new Exception($"Error fetching Teach Table Data: {ex.Message}");
        }
    }
    
    public static async Task<JsonElement> FetchFilteredTeachTableSubjectData(
    int selectedYear,
    int selectedSemester,
    string selectedFaculty,
    string selectedDepartment,
    string selectedCurriculum,
    int selectedClassYear,
    string selectedSubjectId,
    string curriculumYear,
    string uniqueId,
    string? section = null) // section เป็น optional
    {
        string apiUrl = $"https://regis.reg.kmitl.ac.th/api/?" +
                        $"function=get-teach-table-show&mode=by_subject_id" +
                        $"&selected_year={selectedYear}" +
                        $"&selected_semester={selectedSemester}" +
                        $"&selected_faculty={selectedFaculty}" +
                        $"&selected_department={selectedDepartment}" +
                        $"&selected_curriculum={selectedCurriculum}" +
                        $"&selected_class_year={selectedClassYear}" +
                        $"&search_all_faculty=false" +
                        $"&search_all_department=false" +
                        $"&search_all_curriculum=false" +
                        $"&search_all_class_year={(selectedClassYear == 0 ? "true" : "false")}" +
                        $"&selected_subject_id={selectedSubjectId}";

        try
        {
            Console.WriteLine($"Calling Public API: {apiUrl}");
            var response = await HttpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Public API Error: {response.StatusCode} - {response.ReasonPhrase}");
                throw new Exception($"Public API Error: {response.StatusCode}");
            }

            var data = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(data))
            {
                Console.WriteLine("Public API returned empty response.");
                return JsonDocument.Parse("[]").RootElement;
            }

            Console.WriteLine($"Public API Response: {data}");
            var jsonDoc = JsonDocument.Parse(data);

            // ส่ง TransformData โดยพิจารณาค่า section (อาจจะ null)
            var transformedData = await TransformData(jsonDoc.RootElement, curriculumYear, uniqueId, section);
            return transformedData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in FetchFilteredTeachTableSubjectData: {ex.Message}");
            throw new Exception($"Error fetching Teach Table Data: {ex.Message}");
        }
    }
    
    private static async Task<JsonElement> TransformData(
    JsonElement root, 
    string curriculumYear, 
    string uniqueId, 
    string? section = null) // เพิ่ม section เป็น optional parameter
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartArray();

        foreach (var item in root.EnumerateArray())
        {
            Console.WriteLine($"Processing item: {item}");

            if (!item.TryGetProperty("class", out var classValue))
            {
                Console.WriteLine("Skipping item: Missing 'class' field.");
                continue;
            }

            foreach (var teachTable in item.GetProperty("teachtable").EnumerateArray())
            {
                Console.WriteLine($"Processing teachTable: {teachTable}");

                if (!teachTable.TryGetProperty("data", out var dataArray) || dataArray.GetArrayLength() == 0)
                {
                    Console.WriteLine("Skipping teachTable: Missing or empty 'data' field.");
                    continue;
                }

                var subjectTypeNameTh = teachTable.GetProperty("subject_type_name_th").GetString();
                var subjectTypeNameEn = teachTable.GetProperty("subject_type_name_en").GetString();

                foreach (var subject in dataArray.EnumerateArray())
                {
                    var currentSection = int.Parse(subject.GetProperty("section").GetString() ?? "0");

                    // กรองเฉพาะ section ถ้ามีการส่งค่าเข้ามา
                    if (!string.IsNullOrWhiteSpace(section) && currentSection.ToString() != section)
                    {
                        continue;
                    }

                    writer.WriteStartObject();

                    writer.WriteString("classLevel", classValue.GetString());
                    writer.WriteString("subject_type_name_th", subjectTypeNameTh ?? "ไม่มีข้อมูล");
                    writer.WriteString("subject_type_name_en", subjectTypeNameEn ?? "ไม่มีข้อมูล");

                    writer.WriteString("subject_id", subject.GetProperty("subject_id").GetString() ?? "ไม่มีข้อมูล");
                    writer.WriteNumber("credit", int.Parse(subject.GetProperty("credit").GetString() ?? "0"));
                    writer.WriteNumber("section", currentSection);
                    writer.WriteString("credit_lps", subject.GetProperty("credit_lps").GetString());

                    writer.WriteString("subject_name_th", subject.GetProperty("subject_name_th").GetString()?.Trim() ?? "ไม่มีข้อมูล");
                    writer.WriteString("subject_name_en", subject.GetProperty("subject_name_en").GetString()?.Trim() ?? "ไม่มีข้อมูล");

                    var (subjectTypeName, subjectSubTypeName) = await FetchSubjectDetails(subject.GetProperty("subject_id").GetString(), uniqueId, curriculumYear);
                    writer.WriteString("subject_type_name", subjectTypeName ?? "ไม่ระบุ");
                    writer.WriteString("subject_subtype_name", subjectSubTypeName ?? "ไม่ระบุ");

                    // Transform classdatetime
                    var classDatetime = subject.GetProperty("classdatetime").GetString();
                    var transformedDatetime = TransformClassDatetime(classDatetime);
                    writer.WritePropertyName("classdatetime");
                    writer.WriteStartArray();
                    foreach (var dt in transformedDatetime)
                    {
                        writer.WriteStringValue(dt);
                    }
                    writer.WriteEndArray();

                    writer.WriteString("classbuilding", subject.GetProperty("classbuilding").GetString());
                    writer.WriteString("room_no", subject.GetProperty("room_no").GetString());

                    writer.WriteString("rule", subject.GetProperty("rule").GetString());

                    var teacherListTh = TransformTeacherList(subject.GetProperty("teacher_list_th").GetString());
                    var teacherListEn = TransformTeacherList(subject.GetProperty("teacher_list_en").GetString());

                    writer.WritePropertyName("teacher_list_th");
                    writer.WriteStartArray();
                    foreach (var teacher in teacherListTh)
                    {
                        writer.WriteStringValue(teacher);
                    }
                    writer.WriteEndArray();

                    writer.WritePropertyName("teacher_list_en");
                    writer.WriteStartArray();
                    foreach (var teacher in teacherListEn)
                    {
                        writer.WriteStringValue(teacher);
                    }
                    writer.WriteEndArray();

                    var lectOrPrac = subject.GetProperty("lect_or_prac").GetString() == "ท" ? "ทฤษฎี" :
                        subject.GetProperty("lect_or_prac").GetString() == "ป" ? "ปฏิบัติ" : subject.GetProperty("lect_or_prac").GetString();
                    writer.WriteString("lect_or_prac", lectOrPrac);

                    var midtermStart = subject.GetProperty("midterm_start_date_time").GetString();
                    var midtermEnd = subject.GetProperty("midterm_end_date_time").GetString();
                    var midtermDateTime = TransformDateTime(midtermStart, midtermEnd, "กลางภาค");
                    writer.WritePropertyName("midterm_date_time");
                    writer.WriteStartArray();
                    foreach (var value in midtermDateTime)
                    {
                        writer.WriteStringValue(value);
                    }
                    writer.WriteEndArray();

                    var finalStart = subject.GetProperty("final_start_date_time").GetString();
                    var finalEnd = subject.GetProperty("final_end_date_time").GetString();
                    var finalDateTime = TransformDateTime(finalStart, finalEnd, "ปลายภาค");
                    writer.WritePropertyName("final_date_time");
                    writer.WriteStartArray();
                    foreach (var value in finalDateTime)
                    {
                        writer.WriteStringValue(value);
                    }
                    writer.WriteEndArray();

                    var interested = 0;
                    var rating = 0.0f;
                    writer.WriteNumber("interested", interested);
                    writer.WriteNumber("rating", rating);
                    writer.WriteString("remark", subject.GetProperty("remark").GetString());

                    writer.WriteEndObject();
                }
            }
        }

        writer.WriteEndArray();
        writer.Flush();
        var filteredJson = JsonDocument.Parse(stream.ToArray());

        return filteredJson.RootElement.Clone();
    }


    private static async Task<(string? subjectTypeName, string? subjectSubTypeName)> FetchSubjectDetails(
        string subjectId,
        string uniqueId,
        string curriculumYear)
    {
        try
        {
            // ดึงข้อมูล GenEd Subject จาก SdmGenedSubject
            var genedSubject = SdmGenedSubject.GetBy(subjectId);
            if (genedSubject != null && genedSubject.group != null)
            {
                var groupName = genedSubject.group.groupName; // ใช้ groupName จาก GenedGroup
                return (groupName, null);
            }

            // ดึงข้อมูล Subject Group และ Subgroup จาก SdmSubjectGroupAndSubgroup
            var subjectGroupAndSubgroup = SdmSubjectGroupAndSubgroup.GetSubjectGroupAndSubgroupBySubjectId(subjectId, uniqueId, curriculumYear);
            if (subjectGroupAndSubgroup != null)
            {
                return (subjectGroupAndSubgroup.Value.groupName, subjectGroupAndSubgroup.Value.subgroupName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching subject details: {ex.Message}");
        }

        // คืนค่า null ถ้าไม่พบข้อมูลในทั้งสองแหล่ง
        return (null, null);
    }

    private static List<string> TransformTeacherList(string rawTeacherList)
    {
        if (string.IsNullOrWhiteSpace(rawTeacherList))
        {
            return new List<string>();
        }

        var cleanTeacherList = System.Text.RegularExpressions.Regex.Replace(rawTeacherList, "<.*?>", "\n");
        var teachers = cleanTeacherList.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        return teachers.Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
    }

    // แก้ไข: ฟังก์ชัน TransformClassDatetime ให้รองรับ null และรูปแบบผิดปกติ
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