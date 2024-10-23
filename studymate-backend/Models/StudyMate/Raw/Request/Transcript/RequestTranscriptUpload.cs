namespace studymate_backend.Models.StudyMate.Raw.Request.Transcript;

public class RequestTranscriptUpload
{
    public string? UserTokenId { get; set; }
    public IFormFile? File { get; set; }
}