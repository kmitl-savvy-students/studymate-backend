namespace studymate_backend.Helper;

public static class SdmNumber
{
    public static bool IsValid(string? input)
    {
        return input != null && int.TryParse(input, out _);
    }
}