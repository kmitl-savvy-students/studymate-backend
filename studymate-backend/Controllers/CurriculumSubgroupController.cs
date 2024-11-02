using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculumsubgroup/get")]
public class CurriculumSubgroupController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<CurriculumSubgroup>> Get()
    {
        var curriculumSubgroups = SdmCurriculumSubgroup.GetAll();

        if (curriculumSubgroups.Count == 0)
            return NotFound("Curriculum subgroup not found.");
        return Ok(curriculumSubgroups);
    }

    [HttpGet("{c_cat_id:int}/{c_group_id:int}/{c_subgroup_id}")]
    public ActionResult<CurriculumSubgroup> Get(int c_cat_id, int c_group_id, int c_subgroup_id)
    {
        var curriculumSubgroup = SdmCurriculumSubgroup.GetByCatIdAndGroupIdAndSubgroupId(c_cat_id, c_group_id, c_subgroup_id);

        if (curriculumSubgroup == null)
            return NotFound("Curriculum subgroup not found.");
        return Ok(curriculumSubgroup);
    }
}