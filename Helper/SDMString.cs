using System.Text;
using System.Text.RegularExpressions;

namespace studymate_backend.Helper;

public static partial class SDMString
{
    public static bool IsValid(string? input, int maxLength = -1, int minLength = -1)
    {
        if (input == null)
            return false;

        if (maxLength != -1 && input.Length > maxLength)
            return false;

        input = cleanAndTrim(input);
        return minLength == -1 || input.Length >= minLength;
    }


    [GeneratedRegex(@"\s+")]
    private static partial Regex RegexCleanString();

    public static string cleanAndTrim(string? input, int limit = -1)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        var normalized = RegexCleanString().Replace(input, " ");

        var utf8Bytes = Encoding.UTF8.GetBytes(normalized);
        var utf8String = Encoding.UTF8.GetString(utf8Bytes);

        if (limit != -1 && utf8String.Length > limit)
            utf8String = utf8String[..limit];

        return utf8String;
    }
}