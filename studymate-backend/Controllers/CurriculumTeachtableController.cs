using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Helper;
using System.Text.Json;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum-teachtable-subject")]
public class CurriculumTeachtableController : ControllerBase
{
    
    [AllowAnonymous]
    [HttpGet("{year}/{semester}/{faculty}/{department}/{curriculum}/{classYear}/{curriculumYear?}/{uniqueId?}")]
    public async Task<IActionResult> Get(
        [FromRoute] int year,
        [FromRoute] int semester,
        [FromRoute] string faculty,
        [FromRoute] string department,
        [FromRoute] string curriculum,
        [FromRoute] int classYear,
        string? curriculumYear,
        string? uniqueId) // เพิ่ม uniqueId)
    {

        // ตรวจสอบค่า curriculumYear และ uniqueId ถ้ามีการกำหนด
        if (!string.IsNullOrEmpty(curriculumYear) && (curriculumYear != "2560" && curriculumYear != "2564"))
        {
            return BadRequest(new { message = "curriculumYear must be either 2560 or 2564." });
        }
        
        if (!SdmNumber.IsAcademicYear(year) || !SdmNumber.IsAcademicTerm(semester) || !SdmNumber.IsClassYear(classYear))
        {
            return BadRequest(new { message = "Invalid request data." });
        }

        try
        {
            // var filteredData = await SdmCurriculumTeachtable.FetchFilteredTeachTableData(
            //     year, semester, faculty, department, curriculum, classYear, curriculumYear, uniqueId);
            // Pass ค่า null ไปให้ service layer ถ้า parameter ไม่ถูกส่งมา
            var filteredData = await SdmCurriculumTeachtable.FetchFilteredTeachTableData(
                year, semester, faculty, department, curriculum, classYear, 
                curriculumYear ?? string.Empty, 
                uniqueId ?? string.Empty);

            return Ok(filteredData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
    
    [AllowAnonymous]
    [HttpGet("status/{year}/{semester}/{faculty}/{department}/{curriculum}/{classYear}/{subjectId?}/{section?}/{curriculumYear?}/{uniqueId?}")]
    public async Task<IActionResult> GetBySubjectId(
        [FromRoute] int year,
        [FromRoute] int semester,
        [FromRoute] string faculty,
        [FromRoute] string department,
        [FromRoute] string curriculum,
        [FromRoute] int classYear,
        [FromRoute] string? subjectId,  // เปลี่ยนเป็น nullable
        [FromRoute] string? section,   // เปลี่ยนเป็น nullable
        string? curriculumYear,
        string? uniqueId)
    {
        // ตรวจสอบค่าที่จำเป็นอื่น ๆ
                if (!SdmNumber.IsAcademicYear(year) || 
                    !SdmNumber.IsAcademicTerm(semester) || 
                    !SdmNumber.IsClassYear(classYear))
                {
                    return BadRequest(new { message = "Invalid request data." });
                }
                
        // ตรวจสอบว่า subjectId และ section มีค่าหรือไม่
        if (string.IsNullOrWhiteSpace(subjectId) || string.IsNullOrWhiteSpace(section))
        {
            return BadRequest(new { message = "subjectId or section are required." });
        }
    
        
        try
        {
            // เรียก Service Layer
            var filteredData = await SdmCurriculumTeachtable.FetchFilteredTeachTableSubjectData(
                year, semester, faculty, department, curriculum, classYear, subjectId, curriculumYear, uniqueId, section);
    
            if (filteredData.GetArrayLength() == 0)
            {
                return Ok(new JsonElement[0]);
            }
    
            return Ok(filteredData[0]);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }


}