using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Helper;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/teachtable")]
public class TeachtableController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public ActionResult<Teachtable> GetAll()
    {
        var teachTable = SdmTeachtable.GetAll();
        
        if (teachTable.Count == 0)
            return NotFound(new { message = "Teachtable not found." });
        return Ok(teachTable);        
    }

    [AllowAnonymous]
    [HttpGet("{id}")]

    public ActionResult<Teachtable> GetById(int id)
    {
        var teachTable = SdmTeachtable.GetById(id);
        
        if (teachTable == null)
            return NotFound(new { message = "Teachtable not found." });
        return Ok(teachTable);
    }

    [AllowAnonymous]
    [HttpPost]
    public ActionResult Teachtable([FromBody] DtoCreateTeachtable teachtable)
    {
        if (!teachtable.academic_year.HasValue || !teachtable.academic_term.HasValue)
            return BadRequest(new { message = "Academic year and term are required." });

        if (!SdmNumber.IsValid(teachtable.academic_term.ToString()) ||
            !SdmNumber.IsValid(teachtable.academic_year.ToString()) ||
            !SdmNumber.IsAcademicTerm(teachtable.academic_term) ||
            !SdmNumber.IsAcademicYear(teachtable.academic_year))
            return BadRequest(new { message = "Invalid request data." });

        // สร้าง Teachtable object
        SdmTeachtable.Insert(new Teachtable(
            teachtable.academic_year.Value, // ใช้ 0 หรือ default เพราะ id จะถูกสร้างโดย Database
            teachtable.academic_term.Value
        ));
        return Ok(new { message = "Teachtable created." });
    }
    
    [AllowAnonymous]
    [HttpPatch("update")]
    public ActionResult<Teachtable> Update([FromBody] DtoUpdateTeachtable teachtable)
    {
        if (!teachtable.id.HasValue)
            return BadRequest(new { message = "Id is required for update." });
        
        if (!SdmNumber.IsAcademicTerm(teachtable.academic_term) ||
            !SdmNumber.IsAcademicYear(teachtable.academic_year))
            return BadRequest(new { message = "Invalid request data." });
        
        var existingTeachtable = SdmTeachtable.GetById(teachtable.id.Value);
        if (existingTeachtable == null)
            return NotFound(new { message = "Teachtable not found." });

        existingTeachtable.academic_term = teachtable.academic_term ?? existingTeachtable.academic_term;
        existingTeachtable.academic_year = teachtable.academic_year ?? existingTeachtable.academic_year;
        
        SdmTeachtable.Update(existingTeachtable);

        return Ok(teachtable);
    }
    public class DtoCreateTeachtable
    {
        public int? academic_year { get; set; }
        public int? academic_term { get; set; }
    }
    
    public class DtoUpdateTeachtable
    {
        public int? id { get; set; }
        public int? academic_year { get; set; }
        public int? academic_term { get; set; }
    }
}