namespace studymate_backend.Enums.Core
{
    public class BaseEnum
    {
        private readonly string _name;
        private readonly string[] _values;

        private static readonly List<BaseEnum> _all = [];

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
        public string GetValue(int index)
        {
            if (index < 0 || index >= _values.Length)
                throw new IndexOutOfRangeException("Invalid index in Enum");

            return _values[index];
        }

        public static BaseEnum Get(string name)
        {
            return _all.Find(e => e.Name() == name) ?? throw new InvalidOperationException("Enum not found");
        }
        public static IEnumerable<BaseEnum> GetAll()
        {
            return _all;
        }
    }
}
