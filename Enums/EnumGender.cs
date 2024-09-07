using studymate_backend.Enums.Core;

namespace studymate_backend.Enums
{
    public class EnumGender : BaseEnum
    {
        private EnumGender(string name, string[] values) : base(name, values) { }

        public static readonly EnumGender OTHER = new("OTHER", ["Other", "อื่น ๆ"]);
        public static readonly EnumGender MALE = new("MALE", ["Male", "ผู้ชาย"]);
        public static readonly EnumGender FEMALE = new("FEMALE", ["Female", "ผู้หญิง"]);

        public override string ToString()
        {
            return GetValue(0);
        }
        public string ToStringThai()
        {
            return GetValue(1);
        }
    }
}
