using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/teachtable/get")]
public class TeachtableController : ControllerBase
{
    #region [GET] Teachtable
    [AllowAnonymous]
    [HttpGet]
    public ActionResult<Teachtable> GetAll()
    {
        var teachTable = SdmTeachtable.GetAll();

        return Ok(teachTable);
    }
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public ActionResult<Teachtable> GetById(int id)
    {
        var teachTable = SdmTeachtable.GetBy(id);

        if (teachTable == null)
            return NotFound();
        return Ok(teachTable);
    }
    #endregion
}