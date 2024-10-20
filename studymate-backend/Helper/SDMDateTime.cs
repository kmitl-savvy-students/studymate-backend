namespace studymate_backend.Helper;

public class SDMDateTime
{
    private readonly DateTimeOffset _dateTimeOffset;

    public SDMDateTime(DateTime dateTime)
    {
        _dateTimeOffset = new DateTimeOffset(dateTime);
    }

    public SDMDateTime(DateTimeOffset dateTimeOffset)
    {
        _dateTimeOffset = dateTimeOffset;
    }

    public static SDMDateTime Now()
    {
        return new SDMDateTime(DateTime.Now);
    }

    public SDMDateTime ToThailandTime()
    {
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var thailandTime = TimeZoneInfo.ConvertTime(_dateTimeOffset, timeZoneInfo);
        return new SDMDateTime(thailandTime);
    }

    public DateTime ToUTCDateTime()
    {
        return _dateTimeOffset.UtcDateTime;
    }

    public SDMDateTime AddHours(int hours)
    {
        return new SDMDateTime(_dateTimeOffset.AddHours(hours));
    }

    public override string ToString()
    {
        return _dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss zzz");
    }
}