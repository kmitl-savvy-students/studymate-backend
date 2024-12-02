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
    public ActionResult Teachtable([FromBody] DtoTeachtable dtoTeachtable)
    {
        if (!dtoTeachtable.academic_year.HasValue || !dtoTeachtable.academic_term.HasValue)
            return BadRequest(new { message = "Academic year and term are required." });

        if (!SdmNumber.IsValid(dtoTeachtable.academic_term.ToString()) ||
            !SdmNumber.IsValid(dtoTeachtable.academic_year.ToString()) ||
            !SdmNumber.IsAcademicTerm(dtoTeachtable.academic_term.ToString()) ||
            !SdmNumber.IsAcademicYear(dtoTeachtable.academic_year.ToString()))
            return BadRequest(new { message = "Invalid request data." });

        // สร้าง Teachtable object
        SdmTeachtable.Insert(new Teachtable(
            dtoTeachtable.academic_year.Value, // ใช้ 0 หรือ default เพราะ id จะถูกสร้างโดย Database
            dtoTeachtable.academic_term.Value
        ));
        return Ok(new { message = "Teachtable created." });
    }

    public class DtoTeachtable
    {
        public int? academic_year { get; set; }
        public int? academic_term { get; set; }
    }
}