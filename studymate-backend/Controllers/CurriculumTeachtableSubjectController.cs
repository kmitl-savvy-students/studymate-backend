using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace studymate_backend.Controllers
{
    [ApiController]
    [Route("api/curriculum-teachtable-subject-get")]
    public partial class CurriculumTeachtableSubjectController : ControllerBase
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
                    return StatusCode((int)response.StatusCode, new { message = "Error fetching subject data." });

                var json = await response.Content.ReadAsStringAsync();
                var array = JsonSerializer.Deserialize<List<JsonElement>>(json, JsonOptions);
                if (array == null || array.Count == 0)
                    return NotFound(new { message = "Subject not found." });

                var singleObject = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    array[0].ToString(), JsonOptions
                );
                if (singleObject == null)
                    return NotFound(new { message = "Subject not found." });

                singleObject.TryGetValue("detail", out var rawDetailObj);
                var rawDetail = rawDetailObj?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(rawDetail))
                {
                    singleObject["detail_th"] = "";
                    singleObject["detail_en"] = "";
                    singleObject.Remove("detail");
                    return Ok(singleObject);
                }

                var matches = ParagraphRegex().Matches(rawDetail);
                var paragraphs = new List<string>();

                foreach (Match match in matches)
                {
                    var textInsideTag = match.Groups[2].Value;
                    var noHtmlTags = HtmlTagRegex().Replace(textInsideTag, "");
                    var cleaned = InvalidCharRegex().Replace(noHtmlTags, "");
                    cleaned = cleaned.Replace("nbsp;", " ");
                    cleaned = WhitespaceRegex().Replace(cleaned, " ").Trim();
                    paragraphs.Add(cleaned);
                }

                singleObject["detail_th"] = paragraphs.Count > 0 ? paragraphs[0] : "";
                singleObject["detail_en"] = paragraphs.Count > 1 ? paragraphs[1] : "";
                singleObject.Remove("detail");

                return Ok(singleObject);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [GeneratedRegex("<.*?>")]
        private static partial Regex HtmlTagRegex();

        [GeneratedRegex(@"[^\u0E00-\u0E7Fa-zA-Z0-9\s\.,;:\!\?'\-()\""\u2013\u2014\u2026]+")]
        private static partial Regex InvalidCharRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceRegex();

        [GeneratedRegex(@"<(p|div)\b[^>]*>(.*?)(?=(<(p|div)\b[^>]*>)|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline, "th-TH")]
        private static partial Regex ParagraphRegex();
    }
}
