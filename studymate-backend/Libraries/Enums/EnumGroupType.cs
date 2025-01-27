namespace studymate_backend.Libraries.Enums;

public class EnumGroupType : EnumBase
{
    public static readonly EnumGroupType REQUIRED_ALL = new("REQUIRED_ALL", ["Required All"]);
    public static readonly EnumGroupType REQUIRED_CREDIT = new("REQUIRED_CREDIT", ["Required Credit"]);
    public static readonly EnumGroupType REQUIRED_BRANCH = new("REQUIRED_BRANCH", ["Required Branch"]);
    public static readonly EnumGroupType FREE = new("FREE", ["Free"]);

    private EnumGroupType(string name, string[] values) : base(name, values)
    {
    }

    public override string ToString()
    {
        return GetValue(0);
    }
}