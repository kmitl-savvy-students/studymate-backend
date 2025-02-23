using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/subject/get")]
public class SubjectController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public ActionResult<IEnumerable<Subject>> Get()
    {
        var subjects = SdmSubject.GetAll();

        if (subjects.Count == 0)
            return NotFound(new { message = "Subject not found." });
        return Ok(subjects);
    }
    
    [AllowAnonymous]
    [HttpGet("{subjectId}")]
    public ActionResult<DtoSubject> Get(string subjectId)
    {
        var subject = SdmSubject.GetBy(subjectId);

        if (subject == null)
            return NotFound(new { message = "Subject not found." });
        
        double rating = SdmSubjectReview.GetAverageRatingOfReview(subjectId);
        int reviewCount = SdmSubjectReview.GetCountOfReview(subjectId);
        
        var response = new DtoSubject(subject, rating, reviewCount);

        return Ok(response);
    }
    
    public class DtoSubject
    {
        public string Id { get; init; } = string.Empty;
        public string NameTh { get; init; } = string.Empty;
        public string NameEn { get; init; } = string.Empty;
        public int Credit { get; init; }
        public string Detail { get; init; } = string.Empty;
        public double Rating { get; init; }
        public int Review { get; init; }
        
        public DtoSubject(Subject subject, double rating, int review)
        {
            Id = subject.Id;
            NameTh = subject.NameTh;
            NameEn = subject.NameEn;
            Credit = subject.Credit;
            Detail = subject.Detail;
            Rating = rating;
            Review = review;
        }
    }
}