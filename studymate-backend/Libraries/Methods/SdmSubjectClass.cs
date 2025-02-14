using System.Text.Json;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmSubjectClass
{
    private const string KmitlPublicApiUrl = "https://regis.reg.kmitl.ac.th/api/";

    private static readonly HttpClient HttpClient = new();

    public static async Task<List<SubjectClass>> GetAllBy(
        Teachtable? teachtable,
        Models.Program? program,
        string year
    )
    {
        if (teachtable == null || program == null)
            return new List<SubjectClass>();

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

        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
        var response = await client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        var kmitlResponses = JsonSerializer.Deserialize<List<DtoKmitlResponse>>(
            responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (kmitlResponses == null)
            return new List<SubjectClass>();

        foreach (var kmitlResponse in kmitlResponses)
            Console.WriteLine(kmitlResponse.FacultyId);

        return new List<SubjectClass>();
    }

    public class DtoKmitlResponse
    {
        public string? FacultyId { get; init; }
        public string? DepartmentId { get; init; }
        public string? Curriculum2Id { get; init; }
        public string? Class { get; init; }
        public string? FacultyNameTh { get; init; }
        public string? FacultyNameEn { get; init; }
        public string? DepartmentNameTh { get; init; }
        public string? DepartmentNameEn { get; init; }
        public string? CurriculumNameTh { get; init; }
        public string? CurriculumNameEn { get; init; }
        public List<DtoKmitlTeachtable>? Teachtables { get; init; }
    }

    public class DtoKmitlTeachtable
    {
        public string? SubjectTypeNameTh { get; init; }
        public string? SubjectTypeNameEn { get; init; }
        public List<DtoKmitlTeachtableSubject>? Data { get; init; }
    }

    public class DtoKmitlTeachtableSubject
    {
        public string? TeachTableId { get; init; }
        public string? SubjectId { get; init; }
        public string? SubjectNameTh { get; init; }
        public string? SubjectNameEn { get; init; }
        public string? Credit { get; init; }
        public string? CreditLps { get; init; }
        public string? CreditStr { get; init; }
        public string? Section { get; init; }
        public string? SecPair { get; init; }
        public string? LectOrPrac { get; init; }
        public string? ClassDatetime { get; init; }
        public string? Classroom { get; init; }
        public string? RoomNo { get; init; }
        public string? Classbuilding { get; init; }
        public string? BuildingNo { get; init; }
        public string? TeacherListTh { get; init; }
        public string? TeacherListEn { get; init; }
        public string? MidtermStartDateTime { get; init; }
        public string? MidtermEndDateTime { get; init; }
        public string? FinalStartDateTime { get; init; }
        public string? FinalEndDateTime { get; init; }
        public string? ExamTextDetail { get; init; }
        public string? Rule { get; init; }
        public string? Remark { get; init; }
        public string? Closed { get; init; }
        public string? Limit { get; init; }
        public int PreCount { get; init; }
        public int QueueLeft { get; init; }
        public int Count { get; init; }
        public int ClassGroupDisplay { get; init; }
    }
}