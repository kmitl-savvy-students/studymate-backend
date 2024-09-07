using studymate_backend.Enums.Core;

namespace studymate_backend.Enums
{
    public class EnumResponseCode : BaseEnum
    {
        private EnumResponseCode(string name, string[] values) : base(name, values) { }

        public static readonly EnumResponseCode OK = new("OK", ["Ok", "200"]);
        public static readonly EnumResponseCode NOT_FOUND = new("NOT_FOUND", ["Not found", "404"]);
        public static readonly EnumResponseCode BAD_REQUEST = new("BAD_REQUEST", ["Bad request", "400"]);
        public static readonly EnumResponseCode INTERNAL_SERVER_ERROR = new("INTERNAL_SERVER_ERROR", ["Internal Server Error", "500"]);
        public static readonly EnumResponseCode UNAUTHORIZED = new("AUTHORIZED", ["Unauthorized", "401"]);
        public static readonly EnumResponseCode FORBIDDEN = new("FORBIDDEN", ["Forbidden", "403"]);

        public override string ToString()
        {
            return GetValue(0);
        }
        public string GetCode()
        {
            return GetValue(1);
        }
    }
}
