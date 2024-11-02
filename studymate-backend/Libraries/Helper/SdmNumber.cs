namespace studymate_backend.Libraries.Helper;

public static class SdmNumber
{
    public static bool IsValid(string? input)
    {
        return input != null && int.TryParse(input, out _);
    }
}
