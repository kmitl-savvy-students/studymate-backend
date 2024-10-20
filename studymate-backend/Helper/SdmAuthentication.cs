using System.Text.RegularExpressions;

namespace studymate_backend.Helper;

public static partial class SdmAuthentication
{
    [GeneratedRegex(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$")]
    private static partial Regex RegexPasswordStrong();

    public static bool isPasswordStrong(string password)
    {
        return RegexPasswordStrong().IsMatch(password);
    }

    public static string passwordHash(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);
    }

    public static bool passwordVerify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
    }
}