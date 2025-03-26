using studymate_backend.Libraries.Helper;

namespace studymate_backend.Libraries.Models;

public class OtpAuthentication(
    string id,
    int userId,
    int code,
    string referer,
    string status,
    SdmDateTime dateCreated,
    SdmDateTime dateExpired
) : IBaseModel
{
    public string Id { get; set; } = id;
    public int UserId { get; set; } = userId;
    public int Code { get; set; } = code;
    public string Referer { get; set; } = referer;
    public string Status { get; set; } = status;
    public SdmDateTime DateCreated { get; set; } = dateCreated;
    public SdmDateTime DateExpired { get; set; } = dateExpired;
}