namespace studymate_backend.Libraries.Models;

public class Teachtable(
    int id,
    int year,
    int term
) : IBaseModel
{
    public int Id { get; set; } = id;
    public int Year { get; set; } = year;
    public int Term { get; set; } = term;
}