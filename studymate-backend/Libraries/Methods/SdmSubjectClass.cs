using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmSubjectClass
{
    private const string KmitlPublicApiUrl = "https://k8s.reg.kmitl.ac.th/reg/api";

    private static readonly HttpClient HttpClient = new();

    public static async Task<List<SubjectClass>> GetAllBy(
        Teachtable? teachtable,
        Models.Program? program,
        string year
    )
    {
        if (teachtable == null || program == null)
            return [];

        var apiUrl = $"{KmitlPublicApiUrl}?" +
                     $"function=get-teach-table-show" +
                     $"&mode=by_class" +
                     $"&selected_year={teachtable.Year}" +
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
            Console.WriteLine($"Calling Public API: {apiUrl}");
            var response = await HttpClient.GetAsync(apiUrl);
            var data = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Public API Response: {data}");
            return [];
        }
        catch (Exception)
        {
            // ignored
        }

        return [];
    }
}