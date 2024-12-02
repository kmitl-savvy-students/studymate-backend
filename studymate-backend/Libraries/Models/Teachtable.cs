namespace studymate_backend.Libraries.Models;

public class Teachtable(
    int academic_year, 
    int academic_term,
    int id = 0
) : IBaseModel
{
    public int id { get; set; } = id;
    public int academic_year { get; set; } = academic_year;
    public int academic_term { get; set; } = academic_term;
}