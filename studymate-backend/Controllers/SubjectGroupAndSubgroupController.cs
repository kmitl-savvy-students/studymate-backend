using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/subject-group-and-subgroup")]
public class SubjectGroupAndSubgroupController : ControllerBase
{
    [HttpGet("{subjectId}/{uniqueId}/{year}")]
    public ActionResult Get(string subjectId, string uniqueId, string year)
    {
        try
        {
            var result = SdmSubjectGroupAndSubgroup.GetSubjectGroupAndSubgroupBySubjectId(subjectId, uniqueId, year);

            if (result == null)
            {
                return NotFound(new { message = "No data found for the given subject ID, unique ID, and year." });
            }

            return Ok(new
            {
                groupName = result.Value.groupName,
                subgroupName = result.Value.subgroupName
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            return StatusCode(500, new
            {
                message = "Internal Server Error",
                error = ex.Message
            });
        }
    }
}