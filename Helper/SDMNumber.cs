namespace studymate_backend.Helper;

public static class SDMNumber
{
    public static bool IsValid(string? input)
    {
        return input != null && int.TryParse(input, out _);
    }
}