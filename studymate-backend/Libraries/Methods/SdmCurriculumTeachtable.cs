using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using System.Text.Json;

public class SdmCurriculumTeachtable
{
    private static readonly HttpClient HttpClient = new HttpClient();

    public static async Task<JsonElement> FetchFilteredTeachTableData(
        int selectedYear,
        int selectedSemester,
        string selectedFaculty,
        string selectedDepartment,
        string selectedCurriculum,
        int selectedClassYear)
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
            return FilterJsonData(jsonDoc.RootElement);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching Teach Table: {ex.Message}");
        }
    }

    private static JsonElement FilterJsonData(JsonElement root)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        if (root.ValueKind == JsonValueKind.Array)
        {
            writer.WriteStartArray();

            foreach (var item in root.EnumerateArray())
            {
                FilterJsonObject(item, writer);
            }

            writer.WriteEndArray();
        }
        else if (root.ValueKind == JsonValueKind.Object)
        {
            FilterJsonObject(root, writer);
        }

        writer.Flush();
        var filteredJson = JsonDocument.Parse(stream.ToArray());
        return filteredJson.RootElement.Clone();
    }

    private static void FilterJsonObject(JsonElement element, Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        foreach (var property in element.EnumerateObject())
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

                                foreach (var entryProperty in entry.EnumerateObject())
                                {
                                    if (entryProperty.Name != "pre_count" &&
                                        entryProperty.Name != "queue_left" &&
                                        entryProperty.Name != "count" &&
                                        entryProperty.Name != "closed" &&
                                        entryProperty.Name != "limit" &&
                                        entryProperty.Name != "class_group_display")
                                    {
                                        entryProperty.WriteTo(writer);
                                    }
                                }

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
}