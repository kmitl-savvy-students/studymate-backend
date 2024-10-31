using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    [HttpGet("get")]
    public ActionResult<IEnumerable<User>> Get()
    {
        var users = SdmUser.getAll();

        if (users.Count == 0)
            return NotFound("User not found.");
        return Ok(users);
    }
    [HttpGet("get/{id}")]
    public ActionResult<User> Get(string id)
    {
        var user = SdmUser.getById(id);

        if (user == null)
            return NotFound("User not found.");
        return Ok(user);
    }
}
