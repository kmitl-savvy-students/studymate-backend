using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Enums;
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
        var users = SdmUser.GetAll();

        if (users.Count == 0)
            return NotFound("User not found.");
        return Ok(users);
    }
    [HttpGet("get/{id}")]
    public ActionResult<User> Get(string id)
    {
        var user = SdmUser.GetById(id);

        if (user == null)
            return NotFound("User not found.");
        return Ok(user);
    }
    [HttpPost("create")]
    public ActionResult<User> Create([FromBody] DtoCreateUser user)
    {
        SdmUser.Insert(new User(
            user.id,
            user.password,
            EnumBase.Get<EnumGender>(user.gender) ?? EnumGender.OTHER,
            user.nameNick,
            user.nameFirst,
            user.nameLast,
            "",
            null
        ));

        return Ok(user);
    }
    [HttpPatch("update")]
    public ActionResult<User> Update([FromBody] DtoUpdateUser user)
    {
        var existingUser = SdmUser.GetById(user.id);
        if (existingUser == null)
            return NotFound("User not found.");

        if (user.curriculumId != null)
        {
            var newCurriculum = SdmCurriculum.GetById(user.curriculumId ?? -1);
            if (newCurriculum == null)
                return NotFound("Curriculum not found.");
            existingUser.curriculum = newCurriculum;
        }

        existingUser.nameNick = user.nameFirst ?? existingUser.nameNick;
        existingUser.nameFirst = user.nameFirst ?? existingUser.nameFirst;
        existingUser.nameLast = user.nameFirst ?? existingUser.nameLast;
        existingUser.profile = user.nameFirst ?? existingUser.profile;

        SdmUser.Update(existingUser);

        return Ok(user);
    }

    public class DtoCreateUser
    {
        public required string id { get; set; }
        public required string password { get; set; }
        public required string passwordConfirm { get; set; }
        public required string gender { get; set; }
        public required string nameNick { get; set; }
        public required string nameFirst { get; set; }
        public required string nameLast { get; set; }
    }

    public class DtoUpdateUser
    {
        public required string id { get; set; }
        public string? nameNick { get; set; }
        public string? nameFirst { get; set; }
        public string? nameLast { get; set; }
        public string? profile { get; set; }
        public int? curriculumId { get; set; }
    }
}
