namespace studymate_backend.Libraries.Models;

public class Teachtable(
    int id,
    int academicYear,
    int academicTerm
) : IBaseModel
{
    public int Id { get; set; } = id;
    public int AcademicYear { get; set; } = academicYear;
    public int AcademicTerm { get; set; } = academicTerm;
}