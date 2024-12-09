// using System.Text.Json;
//
// namespace studymate_backend.Libraries.Methods;
//
// public class SdmCurriculumTeachtable
// {
//     private static readonly HttpClient HttpClient = new HttpClient();
//
//     public static async Task<JsonElement> FetchFilteredTeachTableData(
//         int selectedYear,
//         int selectedSemester,
//         string selectedFaculty,
//         string selectedDepartment,
//         string selectedCurriculum,
//         int selectedClassYear,
//         string curriculumYear) // เปลี่ยนเป็น string
//     {
//         string apiUrl = $"https://k8s.reg.kmitl.ac.th/reg/api/?" +
//                         $"function=get-teach-table-show&mode=by_class" +
//                         $"&selected_year={selectedYear}" +
//                         $"&selected_semester={selectedSemester}" +
//                         $"&selected_faculty={selectedFaculty}" +
//                         $"&selected_department={selectedDepartment}" +
//                         $"&selected_curriculum={selectedCurriculum}" +
//                         $"&selected_class_year={selectedClassYear}" +
//                         $"&search_all_faculty=false" +
//                         $"&search_all_department=false" +
//                         $"&search_all_curriculum=false" +
//                         $"&search_all_class_year=false";
//
//         try
//         {
//             var response = await HttpClient.GetAsync(apiUrl);
//             response.EnsureSuccessStatusCode();
//
//             var data = await response.Content.ReadAsStringAsync();
//             var jsonDoc = JsonDocument.Parse(data);
//
//             // ส่ง curriculumYear (string) ไปยัง TransformData
//             return await TransformData(jsonDoc.RootElement, curriculumYear, selectedCurriculum);
//         }
//         catch (Exception ex)
//         {
//             throw new Exception($"Error fetching Teach Table: {ex.Message}");
//         }
//     }
//
//     private static async Task<JsonElement> TransformData(JsonElement root, string curriculumYear, string curriculum)
// {
//     using var stream = new MemoryStream();
//     using var writer = new Utf8JsonWriter(stream);
//
//     writer.WriteStartArray();
//
//     foreach (var item in root.EnumerateArray())
//     {
//         writer.WriteStartObject();
//
//         foreach (var property in item.EnumerateObject())
//         {
//             if (property.Name == "teachtable")
//             {
//                 writer.WritePropertyName("teachtable");
//                 writer.WriteStartArray();
//
//                 foreach (var teachTable in property.Value.EnumerateArray())
//                 {
//                     writer.WriteStartObject();
//
//                     foreach (var teachProperty in teachTable.EnumerateObject())
//                     {
//                         if (teachProperty.Name == "data")
//                         {
//                             writer.WritePropertyName("data");
//                             writer.WriteStartArray();
//
//                             foreach (var entry in teachProperty.Value.EnumerateArray())
//                             {
//                                 writer.WriteStartObject();
//                                 
//                                 var subjectId = entry.GetProperty("subject_id").GetString();
//                                 // แปลง String เป็น Number สำหรับ credit และ section
//                                 int credit = int.Parse(entry.GetProperty("credit").GetString());
//                                 int section = int.Parse(entry.GetProperty("section").GetString());
//                                 
//                                 writer.WriteString("subject_id", subjectId);
//                                 writer.WriteNumber("credit", credit); 
//                                 writer.WriteNumber("section", section);
//                                 writer.WriteString("subject_name_th", entry.GetProperty("subject_name_th").GetString());
//                                 writer.WriteString("subject_name_en", entry.GetProperty("subject_name_en").GetString());
//
//                                 // Transform classdatetime
//                                 var classdatetime = entry.GetProperty("classdatetime").GetString();
//                                 var transformedDatetime = TransformClassDatetime(classdatetime);
//                                 writer.WritePropertyName("classdatetime");
//                                 writer.WriteStartArray();
//                                 foreach (var dt in transformedDatetime)
//                                 {
//                                     writer.WriteStringValue(dt);
//                                 }
//                                 writer.WriteEndArray();
//
//                                 // Transform room
//                                 var roomNo = TransformRoom(
//                                     entry.GetProperty("classbuilding").GetString(),
//                                     entry.GetProperty("classroom").GetString()
//                                 );
//                                 writer.WriteString("room_no", roomNo);
//
//                                 // Transform rule
//                                 var rawRule = entry.GetProperty("rule").GetString();
//                                 var transformedRule = TransformRule(rawRule);
//                                 writer.WritePropertyName("rule");
//                                 writer.WriteStartArray();
//                                 foreach (var rule in transformedRule)
//                                 {
//                                     writer.WriteStringValue(rule);
//                                 }
//                                 writer.WriteEndArray();
//                                 
//                                 var teacherListTh = TransformTeacherList(entry.GetProperty("teacher_list_th").GetString());
//                                 var teacherListEn = TransformTeacherList(entry.GetProperty("teacher_list_en").GetString());
//
//                                 writer.WritePropertyName("teacher_list_th");
//                                 writer.WriteStartArray();
//                                 foreach (var teacher in teacherListTh)
//                                 {
//                                     writer.WriteStringValue(teacher);
//                                 }
//                                 writer.WriteEndArray();
//
//                                 writer.WritePropertyName("teacher_list_en");
//                                 writer.WriteStartArray();
//                                 foreach (var teacher in teacherListEn)
//                                 {
//                                     writer.WriteStringValue(teacher);
//                                 }
//                                 writer.WriteEndArray();
//                                 
//                                 // แปลงค่า lect_or_prac
//                                 var lectOrPrac = entry.GetProperty("lect_or_prac").GetString() == "ท" ? "ทฤษฎี" :
//                                     entry.GetProperty("lect_or_prac").GetString() == "ป" ? "ปฏิบัติ" : entry.GetProperty("lect_or_prac").GetString();
//
//                                 // เขียนค่า lect_or_prac
//                                 writer.WriteString("lect_or_prac", lectOrPrac);
//
//                                 
//                                 writer.WriteEndObject();
//                             }
//
//                             writer.WriteEndArray();
//                         }
//                         else if (teachProperty.Name != "subject_type_name_th" && teachProperty.Name != "subject_type_name_en")
//                         {
//                             // คัดกรอง subject_type_name_th และ subject_type_name_en ไม่ให้เขียนลง JSON
//                             teachProperty.WriteTo(writer);
//                         }
//                     }
//
//                     writer.WriteEndObject();
//                 }
//
//                 writer.WriteEndArray();
//             }
//             else
//             {
//                 property.WriteTo(writer);
//             }
//         }
//
//         writer.WriteEndObject();
//     }
//
//     writer.WriteEndArray();
//     writer.Flush();
//     var filteredJson = JsonDocument.Parse(stream.ToArray());
//     return filteredJson.RootElement.Clone();
// }
//
//     private static async Task<(string? subjectTypeName, string? subjectSubTypeName)> FetchSubjectDetails(
//         string subjectId,
//         string uniqueId,
//         string curriculumYear)
//     {
//         // ตรวจสอบว่า curriculumYear มีค่าเป็น "2560" หรือ "2564"
//         if (curriculumYear != "2560" && curriculumYear != "2564")
//         {
//             throw new ArgumentException("curriculumYear must be either '2560' or '2564'.");
//         }
//
//         // URL ของ API
//         string genEdApiUrl = $"https://your-api.com/api/gened-subject/get/{subjectId}";
//         string subjectGroupApiUrl = $"https://your-api.com/api/subject-group-and-subgroup/{subjectId}/{uniqueId}/{curriculumYear}";
//
//         try
//         {
//             // เรียก API GenEd Subject
//             var genEdResponse = await HttpClient.GetAsync(genEdApiUrl);
//             if (genEdResponse.IsSuccessStatusCode)
//             {
//                 var genEdData = await genEdResponse.Content.ReadAsStringAsync();
//                 var genEdJson = JsonDocument.Parse(genEdData);
//
//                 // ดึง group_name
//                 var groupName = genEdJson.RootElement
//                     .GetProperty("group")
//                     .GetProperty("group_name")
//                     .GetString();
//
//                 // หากเจอข้อมูลใน GenEd API ให้คืนค่า subjectTypeName และตั้ง subjectSubTypeName = null
//                 return (groupName, null);
//             }
//
//             // หากไม่เจอข้อมูลใน GenEd API ให้เรียก API Subject Group and Subgroup
//             var subjectGroupResponse = await HttpClient.GetAsync(subjectGroupApiUrl);
//             if (subjectGroupResponse.IsSuccessStatusCode)
//             {
//                 var subjectGroupData = await subjectGroupResponse.Content.ReadAsStringAsync();
//                 var subjectGroupJson = JsonDocument.Parse(subjectGroupData);
//
//                 // ดึง group_name และ subgroup_name
//                 var groupName = subjectGroupJson.RootElement.GetProperty("group_name").GetString();
//                 var subGroupName = subjectGroupJson.RootElement.GetProperty("subgroup_name").GetString();
//
//                 // หากเจอข้อมูลใน Subject Group and Subgroup API ให้คืนค่า
//                 return (groupName, subGroupName);
//             }
//         }
//         catch (Exception ex)
//         {
//             // Log ข้อผิดพลาด
//             Console.WriteLine($"Error fetching subject details: {ex.Message}");
//         }
//
//         // หากไม่พบข้อมูลในทั้งสอง API ให้คืนค่า null
//         return (null, null);
//     }
//
//     private static List<string> TransformTeacherList(string rawTeacherList)
//     {
//         if (string.IsNullOrWhiteSpace(rawTeacherList))
//         {
//             return new List<string>();
//         }
//
//         // ลบ HTML Tags เช่น <div>
//         var cleanTeacherList = System.Text.RegularExpressions.Regex.Replace(rawTeacherList, "<.*?>", "\n");
//
//         // แยกข้อความตามบรรทัดใหม่
//         var teachers = cleanTeacherList.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
//
//         // ตัดช่องว่างรอบข้อความ
//         return teachers.Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
//     }
//     
//     private static List<string> TransformClassDatetime(string rawDatetime)
//     {
//         var dayMapping = new Dictionary<string, string>
//         {
//             { "จ.", "จันทร์" },
//             { "อ.", "อังคาร" },
//             { "พ.", "พุธ" },
//             { "พฤ.", "พฤหัสบดี" },
//             { "ศ.", "ศุกร์" },
//             { "ส.", "เสาร์" },
//             { "อา.", "อาทิตย์" }
//         };
//
//         var result = new List<string>();
//         var parts = rawDatetime.Split(new[] { "<div>", "</div>", "+" }, StringSplitOptions.RemoveEmptyEntries);
//
//         string? currentDay = null;
//         foreach (var part in parts)
//         {
//             // ตรวจจับวัน (เช่น จ., อ., พ.)
//             var dayMatch = dayMapping.FirstOrDefault(m => part.StartsWith(m.Key));
//             if (dayMatch.Key != null)
//             {
//                 // เก็บชื่อวัน
//                 currentDay = dayMatch.Value;
//                 if (!result.Contains(currentDay))
//                 {
//                     result.Add(currentDay);
//                 }
//
//                 // ดึงเฉพาะช่วงเวลา
//                 var timePart = part.Replace(dayMatch.Key, "").Trim();
//                 if (!string.IsNullOrWhiteSpace(timePart))
//                 {
//                     result.Add(timePart);
//                 }
//             }
//             else if (currentDay != null)
//             {
//                 // กรณีเป็นช่วงเวลาในวันเดียวกัน
//                 var cleanedPart = part.Trim();
//                 // ลบ "จ. " ออกจากช่วงเวลา
//                 foreach (var dayKey in dayMapping.Keys)
//                 {
//                     if (cleanedPart.StartsWith(dayKey))
//                     {
//                         cleanedPart = cleanedPart.Replace(dayKey, "").Trim();
//                         break;
//                     }
//                 }
//                 result.Add(cleanedPart);
//             }
//         }
//
//         return result;
//     }
//
//     private static string TransformRoom(string building, string roomNo)
//     {
//         return $"{building} {roomNo}";
//     }
//     
//     private static List<string> TransformRule(string rawRule)
//     {
//         if (string.IsNullOrWhiteSpace(rawRule))
//         {
//             return new List<string>();
//         }
//
//         // ลบ HTML tags เช่น <div>, <font>
//         var cleanRule = System.Text.RegularExpressions.Regex.Replace(rawRule, "<.*?>", "");
//
//         // ใช้ Regex แยกข้อความออกตามรูปแบบที่ต้องการ
//         var splitRules = System.Text.RegularExpressions.Regex.Split(cleanRule, @"(?=เฉพาะ นศ\.)");
//
//         // ตัดช่องว่างรอบข้อความ
//         return splitRules.Select(r => r.Trim()).Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
//     }
//     
// }

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
        string curriculumYear)
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
                        $"&search_all_class_year=false";

        try
        {
            var response = await HttpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(data);

            return await TransformData(jsonDoc.RootElement, curriculumYear, selectedCurriculum);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching Teach Table: {ex.Message}");
        }
    }

    private static async Task<JsonElement> TransformData(JsonElement root, string curriculumYear, string curriculum)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartArray();

        foreach (var item in root.EnumerateArray())
        {
            writer.WriteStartObject();

            foreach (var property in item.EnumerateObject())
            {
                if (property.Name == "teachtable")
                {
                    writer.WritePropertyName("teachtable");
                    writer.WriteStartArray();

                    foreach (var teachTable in property.Value.EnumerateArray())
                    {
                        writer.WriteStartObject();

                        foreach (var teachProperty in teachTable.EnumerateObject())
                        {
                            if (teachProperty.Name == "data")
                            {
                                writer.WritePropertyName("data");
                                writer.WriteStartArray();

                                foreach (var entry in teachProperty.Value.EnumerateArray())
                                {
                                    writer.WriteStartObject();

                                    var subjectId = entry.GetProperty("subject_id").GetString();


                                    writer.WriteString("subject_id", subjectId);
                                    writer.WriteNumber("credit", int.Parse(entry.GetProperty("credit").GetString()));
                                    writer.WriteNumber("section", int.Parse(entry.GetProperty("section").GetString()));
                                    writer.WriteString("subject_name_th", entry.GetProperty("subject_name_th").GetString());
                                    writer.WriteString("subject_name_en", entry.GetProperty("subject_name_en").GetString());
                                    
                                    var uniqueId = curriculum == "06" ? "0132" : "1234";
                                    
                                    // Fetch subject_type_name และ subject_subtype_name
                                    var (subjectTypeName, subjectSubTypeName) = await FetchSubjectDetails(subjectId, uniqueId, curriculumYear);

                                    // เพิ่ม subject_type_name และ subject_subtype_name
                                    writer.WriteString("subject_type_name", subjectTypeName ?? "ไม่ระบุ");
                                    writer.WriteString("subject_subtype_name", subjectSubTypeName ?? "ไม่ระบุ");

                                    // Transform classdatetime
                                    var classdatetime = entry.GetProperty("classdatetime").GetString();
                                    var transformedDatetime = TransformClassDatetime(classdatetime);
                                    writer.WritePropertyName("classdatetime");
                                    writer.WriteStartArray();
                                    foreach (var dt in transformedDatetime)
                                    {
                                        writer.WriteStringValue(dt);
                                    }
                                    writer.WriteEndArray();

                                    // Transform room
                                    var roomNo = TransformRoom(
                                        entry.GetProperty("classbuilding").GetString(),
                                        entry.GetProperty("classroom").GetString()
                                    );
                                    writer.WriteString("room_no", roomNo);

                                    // Transform rule
                                    var rawRule = entry.GetProperty("rule").GetString();
                                    var transformedRule = TransformRule(rawRule);
                                    writer.WritePropertyName("rule");
                                    writer.WriteStartArray();
                                    foreach (var rule in transformedRule)
                                    {
                                        writer.WriteStringValue(rule);
                                    }
                                    writer.WriteEndArray();

                                    // Transform teacher_list
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

                                    // แปลงค่า lect_or_prac
                                    var lectOrPrac = entry.GetProperty("lect_or_prac").GetString() == "ท" ? "ทฤษฎี" :
                                        entry.GetProperty("lect_or_prac").GetString() == "ป" ? "ปฏิบัติ" : entry.GetProperty("lect_or_prac").GetString();

                                    writer.WriteString("lect_or_prac", lectOrPrac);

                                    writer.WriteEndObject();
                                }

                                writer.WriteEndArray();
                            }
                            else
                            {
                                teachProperty.WriteTo(writer);
                            }
                        }

                        writer.WriteEndObject();
                    }

                    writer.WriteEndArray();
                }
                else
                {
                    property.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.Flush();
        var filteredJson = JsonDocument.Parse(stream.ToArray());
        return filteredJson.RootElement.Clone();
    }

    // private static async Task<(string? subjectTypeName, string? subjectSubTypeName)> FetchSubjectDetails(
    //     string subjectId,
    //     string uniqueId,
    //     string curriculumYear)
    // {
    //     string genEdApiUrl = $"https://your-api.com/api/gened-subject/get/{subjectId}";
    //     string subjectGroupApiUrl = $"https://your-api.com/api/subject-group-and-subgroup/{subjectId}/{uniqueId}/{curriculumYear}";
    //
    //     try
    //     {
    //         var genEdResponse = await HttpClient.GetAsync(genEdApiUrl);
    //         if (genEdResponse.IsSuccessStatusCode)
    //         {
    //             var genEdData = await genEdResponse.Content.ReadAsStringAsync();
    //             var genEdJson = JsonDocument.Parse(genEdData);
    //             var groupName = genEdJson.RootElement.GetProperty("group").GetProperty("group_name").GetString();
    //             return (groupName, null);
    //         }
    //
    //         var subjectGroupResponse = await HttpClient.GetAsync(subjectGroupApiUrl);
    //         if (subjectGroupResponse.IsSuccessStatusCode)
    //         {
    //             var subjectGroupData = await subjectGroupResponse.Content.ReadAsStringAsync();
    //             var subjectGroupJson = JsonDocument.Parse(subjectGroupData);
    //             var groupName = subjectGroupJson.RootElement.GetProperty("group_name").GetString();
    //             var subGroupName = subjectGroupJson.RootElement.GetProperty("subgroup_name").GetString();
    //             return (groupName, subGroupName);
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error fetching subject details: {ex.Message}");
    //     }
    //
    //     return (null, null);
    // }
    
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

    private static string TransformRoom(string building, string roomNo)
    {
        return $"{building} {roomNo}";
    }

    private static List<string> TransformRule(string rawRule)
    {
        if (string.IsNullOrWhiteSpace(rawRule))
        {
            return new List<string>();
        }

        var cleanRule = System.Text.RegularExpressions.Regex.Replace(rawRule, "<.*?>", "");
        var splitRules = System.Text.RegularExpressions.Regex.Split(cleanRule, @"(?=เฉพาะ นศ\.)");
        return splitRules.Select(r => r.Trim()).Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
    }
}
