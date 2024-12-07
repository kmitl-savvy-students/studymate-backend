namespace studymate_backend.Libraries.Helper;

public static class SdmNumber
{
    public static bool IsValid(string? input)
    {
        return input != null && int.TryParse(input, out _);
    }
    
    public static bool IsAcademicYear(int? year)
    {
        return year >= 2000 && year <= 9999;
    }
    
    public static bool IsAcademicTerm(int? term)
    {
        return term >= 1 && term <= 3;
    }

    public static bool IsClassYear(int input)
    {
        return input >= 1 && input <= 6;
    }
    
}
