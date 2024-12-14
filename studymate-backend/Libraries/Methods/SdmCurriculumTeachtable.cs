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
    string uniqueId) // เพิ่ม parameter uniqueId
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
            var response = await HttpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(data);

            return await TransformData(jsonDoc.RootElement, curriculumYear, uniqueId);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching Teach Table: {ex.Message}");
        }
    }

    private static async Task<JsonElement> TransformData(JsonElement root, string curriculumYear, string uniqueId)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartArray();

        foreach (var item in root.EnumerateArray())
        {
            writer.WriteStartObject();

            // เขียน property อื่นๆ ที่ไม่ใช่ teachtable
            foreach (var property in item.EnumerateObject())
            {
                if (property.Name != "teachtable")
                {
                    property.WriteTo(writer);
                }
            }

            // ตัวแปรสะสม Array data
            var allData = new List<JsonElement>();

            // รวบรวมข้อมูลทั้งหมดจาก teachtable
            if (item.TryGetProperty("teachtable", out JsonElement teachTable))
            {
                foreach (var teachEntry in teachTable.EnumerateArray())
                {
                    foreach (var teachProperty in teachEntry.EnumerateObject())
                    {
                        if (teachProperty.Name == "data")
                        {
                            foreach (var entry in teachProperty.Value.EnumerateArray())
                            {
                                allData.Add(entry); // สะสมข้อมูลทั้งหมดใน allData
                            }
                        }
                    }
                }
            }

            // เขียน teachtable ใหม่โดยรวมข้อมูลทั้งหมดใน data ก้อนเดียว
            writer.WritePropertyName("teachtable");
            writer.WriteStartArray();
            writer.WriteStartObject();
            writer.WritePropertyName("data");
            writer.WriteStartArray();

            foreach (var entry in allData)
            {
                writer.WriteStartObject();

                var subjectId = entry.GetProperty("subject_id").GetString();
                writer.WriteString("subject_id", subjectId);
                writer.WriteNumber("credit", int.Parse(entry.GetProperty("credit").GetString()));
                writer.WriteNumber("section", int.Parse(entry.GetProperty("section").GetString()));

                writer.WriteString("subject_name_th", entry.GetProperty("subject_name_th").GetString()?.Replace("\t", "").Trim());
                writer.WriteString("subject_name_en", entry.GetProperty("subject_name_en").GetString()?.Replace("\t", "").Trim());

                var (subjectTypeName, subjectSubTypeName) = await FetchSubjectDetails(subjectId, uniqueId, curriculumYear);
                writer.WriteString("subject_type_name", subjectTypeName ?? "ไม่ระบุ");
                writer.WriteString("subject_subtype_name", subjectSubTypeName ?? "ไม่ระบุ");

                var classdatetime = entry.GetProperty("classdatetime").GetString();
                var transformedDatetime = TransformClassDatetime(classdatetime);
                writer.WritePropertyName("classdatetime");
                writer.WriteStartArray();
                foreach (var dt in transformedDatetime)
                {
                    writer.WriteStringValue(dt);
                }
                writer.WriteEndArray();

                writer.WriteString("classbuilding", entry.GetProperty("classbuilding").GetString());
                writer.WriteString("room_no", entry.GetProperty("room_no").GetString());
                writer.WriteString("rule", entry.GetProperty("rule").GetString());

                var teacherListTh = TransformTeacherList(entry.GetProperty("teacher_list_th").GetString());
                var teacherListEn = TransformTeacherList(entry.GetProperty("teacher_list_en").GetString());

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

                var lectOrPrac = entry.GetProperty("lect_or_prac").GetString() == "ท" ? "ทฤษฎี" :
                    entry.GetProperty("lect_or_prac").GetString() == "ป" ? "ปฏิบัติ" : entry.GetProperty("lect_or_prac").GetString();
                writer.WriteString("lect_or_prac", lectOrPrac);

                var midtermStart = entry.GetProperty("midterm_start_date_time").GetString();
                var midtermEnd = entry.GetProperty("midterm_end_date_time").GetString();
                var midtermDateTime = TransformDateTime(midtermStart, midtermEnd, "กลางภาค");
                writer.WritePropertyName("midterm_date_time");
                writer.WriteStartArray();
                foreach (var value in midtermDateTime)
                {
                    writer.WriteStringValue(value);
                }
                writer.WriteEndArray();

                var finalStart = entry.GetProperty("final_start_date_time").GetString();
                var finalEnd = entry.GetProperty("final_end_date_time").GetString();
                var finalDateTime = TransformDateTime(finalStart, finalEnd, "ปลายภาค");
                writer.WritePropertyName("final_date_time");
                writer.WriteStartArray();
                foreach (var value in finalDateTime)
                {
                    writer.WriteStringValue(value);
                }
                writer.WriteEndArray();

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.WriteEndArray();

            writer.WriteEndObject();
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
        // URL ของ API
        string genEdApiUrl = $"http://localhost:5000/api/gened-subject/get/{subjectId}";
        string subjectGroupApiUrl = $"http://localhost:5000/api/subject-group-and-subgroup/{subjectId}/{uniqueId}/{curriculumYear}";

        try
        {
            // ลองเรียก API GenEd Subject
            var genEdResponse = await HttpClient.GetAsync(genEdApiUrl);
            if (genEdResponse.IsSuccessStatusCode)
            {
                var genEdData = await genEdResponse.Content.ReadAsStringAsync();
                var genEdJson = JsonDocument.Parse(genEdData);

                // ดึงค่า group_name
                var groupName = genEdJson.RootElement
                    .GetProperty("group")
                    .GetProperty("group_name")
                    .GetString();
                
                Console.WriteLine($"Calling API: {genEdApiUrl}");
                Console.WriteLine($"Response from API: {await genEdResponse.Content.ReadAsStringAsync()}");
                return (groupName, null);
            }

            // ถ้าไม่เจอข้อมูลใน GenEd API ให้ลองเรียก Subject Group API
            var subjectGroupResponse = await HttpClient.GetAsync(subjectGroupApiUrl);
            if (subjectGroupResponse.IsSuccessStatusCode)
            {
                var subjectGroupData = await subjectGroupResponse.Content.ReadAsStringAsync();
                var subjectGroupJson = JsonDocument.Parse(subjectGroupData);

                // ดึงค่า group_name และ subgroup_name
                var groupName = subjectGroupJson.RootElement.GetProperty("group_name").GetString();
                var subGroupName = subjectGroupJson.RootElement.GetProperty("subgroup_name").GetString();
                Console.WriteLine($"Calling API: {subjectGroupApiUrl}");
                Console.WriteLine($"Response from API: {await subjectGroupResponse.Content.ReadAsStringAsync()}");
                return (groupName, subGroupName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching subject details: {ex.Message}");
        }

        // คืนค่า "ไม่ระบุ" ถ้าไม่พบข้อมูลในทั้งสอง API
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

    private static List<string> TransformClassDatetime(string rawDatetime)
    {
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