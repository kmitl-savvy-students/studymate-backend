using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace studymate_backend.Controllers
{
    [ApiController]
    [Route("api/curriculum-teachtable-subject-get")]
    public class CurriculumTeachtableSubjectController : ControllerBase
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        [AllowAnonymous]
        [HttpGet("{subjectId}")]
        public async Task<IActionResult> GetSubjectById([FromRoute] string subjectId)
        {
            try
            {
                var url = $"https://k8s.reg.kmitl.ac.th/api/subject/?function=get-registrar-subject&level_id=1&subject_id={Uri.EscapeDataString(subjectId)}";

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { message = "Error fetching subject data." });
                }

                var json = await response.Content.ReadAsStringAsync();

                var array = JsonSerializer.Deserialize<List<object>>(json, JsonOptions);

                if (array == null || array.Count == 0)
                {
                    return NotFound(new { message = "Subject not found." });
                }

                var singleObject = array[0];

                return Ok(singleObject);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}