namespace studymate_backend.Enums.Core;

public abstract class BaseEnum
{
    private static readonly List<BaseEnum> All = [];
    private readonly string _name;
    private readonly string[] _values;

    protected BaseEnum(string name, string[] values)
    {
        _name = name;
        _values = values;

        All.Add(this);
    }

    public string GetName()
    {
        return _name;
    }

    protected string GetValue(int index)
    {
        if (index < 0 || index >= _values.Length)
            throw new IndexOutOfRangeException("Invalid index in Enum");

        return _values[index];
    }

    public static T? Get<T>(string name) where T : BaseEnum
    {
        return All.Find(e => e.GetName() == name) as T;
    }

    public static IEnumerable<BaseEnum> GetAll()
    {
        return All;
    }
}