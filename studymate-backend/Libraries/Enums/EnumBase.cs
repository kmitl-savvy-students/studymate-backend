namespace studymate_backend.Libraries.Enums;

public abstract class EnumBase
{
    private static readonly List<EnumBase> _all = [];
    private readonly string _name;
    private readonly string[] _values;

    protected EnumBase(string name, string[] values)
    {
        _name = name;
        _values = values;

        _all.Add(this);
    }

    public string GetName()
    {
        return _name;
    }

    protected string GetValue(int index)
    {
        if (index < 0 || index >= _values.Length)
            return "";

        return _values[index];
    }

    public static T? Get<T>(string name) where T : EnumBase
    {
        return _all.Find(e => e.GetName() == name) as T;
    }

    public static IEnumerable<EnumBase> GetAll()
    {
        return _all;
    }
}
