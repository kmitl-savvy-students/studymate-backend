namespace studymate_backend.Libraries.Helper;

public static class SdmNumber
{
    public static bool IsValid(string? input)
    {
        return input != null && int.TryParse(input, out _);
    }
    
    public static bool IsAcademicYear(string? input)
    {
        if (input != null && int.TryParse(input, out int year))
        {
            return year >= 2000 && year <= 9999;
        }
        return false;
    }
    
    public static bool IsAcademicTerm(string? input)
    {
        if (input != null && int.TryParse(input, out int term))
        {
            return term >= 1 && term <= 3;
        }
        return false;
    }
    
}
