namespace studymate_backend.Enums.Core;

public abstract class BaseEnum
{
    private static readonly List<BaseEnum> _all = [];
    private readonly string _name;
    private readonly string[] _values;

    protected BaseEnum(string name, string[] values)
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

    public static T? Get<T>(string name) where T : BaseEnum
    {
        return _all.Find(e => e.GetName() == name) as T;
    }

    public static IEnumerable<BaseEnum> GetAll()
    {
        return _all;
    }
}