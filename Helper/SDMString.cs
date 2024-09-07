namespace studymate_backend.Helper
{
    public class SDMString
    {
        public static bool IsValidNumber(string input)
        {
            return int.TryParse(input, out int _);
        }
    }
}
