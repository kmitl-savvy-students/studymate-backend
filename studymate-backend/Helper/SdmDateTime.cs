namespace studymate_backend.Helper;

public class SdmDateTime
{
    private readonly DateTimeOffset _dateTimeOffset;

    public SdmDateTime(DateTime dateTime)
    {
        _dateTimeOffset = new DateTimeOffset(dateTime);
    }

    public SdmDateTime(DateTimeOffset dateTimeOffset)
    {
        _dateTimeOffset = dateTimeOffset;
    }

    public static SdmDateTime Now()
    {
        return new SdmDateTime(DateTime.Now);
    }

    public SdmDateTime ToThailandTime()
    {
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var thailandTime = TimeZoneInfo.ConvertTime(_dateTimeOffset, timeZoneInfo);
        return new SdmDateTime(thailandTime);
    }

    public DateTime ToUTCDateTime()
    {
        return _dateTimeOffset.UtcDateTime;
    }

    public SdmDateTime AddHours(int hours)
    {
        return new SdmDateTime(_dateTimeOffset.AddHours(hours));
    }

    public override string ToString()
    {
        return _dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss zzz");
    }
}