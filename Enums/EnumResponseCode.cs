using studymate_backend.Enums.Core;

namespace studymate_backend.Enums;

public class EnumResponseCode : BaseEnum
{
    public static readonly EnumResponseCode OK = new("OK", ["Ok", "200"]);
    public static readonly EnumResponseCode CREATED = new("CREATED", ["Object created successfully", "200"]);

    public static readonly EnumResponseCode NOT_FOUND = new("NOT_FOUND", ["Not found", "404"]);
    public static readonly EnumResponseCode DUPLICATE_ID = new("DUPLICATE_ID", ["The ID is already in use", "409"]);

    public static readonly EnumResponseCode BAD_REQUEST = new("BAD_REQUEST", ["Bad request", "400"]);
    public static readonly EnumResponseCode FIELDS_INVALID = new("FIELDS_INVALID", ["One or more fields are invalid", "400"]);
    public static readonly EnumResponseCode PASSWORD_MISMATCH = new("PASSWORD_MISMATCH", ["Password and confirm password do not match", "400"]);
    public static readonly EnumResponseCode PASSWORD_WEAK = new("PASSWORD_WEAK", ["Password is too weak", "400"]);

    public static readonly EnumResponseCode INTERNAL_SERVER_ERROR = new("INTERNAL_SERVER_ERROR", ["Internal Server Error", "500"]);
    public static readonly EnumResponseCode UNAUTHORIZED = new("UNAUTHORIZED", ["Unauthorized", "401"]);
    public static readonly EnumResponseCode FORBIDDEN = new("FORBIDDEN", ["Forbidden", "403"]);

    private EnumResponseCode(string name, string[] values) : base(name, values)
    {
    }

    public override string ToString()
    {
        return GetValue(0);
    }

    public string GetCode()
    {
        return GetValue(1);
    }
}