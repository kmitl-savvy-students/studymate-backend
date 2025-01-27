using System.Globalization;

namespace studymate_backend.Libraries.Helper;

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

    public SdmDateTime(string dateTimeString)
    {
        try
        {
            _dateTimeOffset = DateTimeOffset.ParseExact(
                dateTimeString,
                "yyyy-MM-dd HH:mm:ss zzz",
                CultureInfo.InvariantCulture
            );
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid date format. Expected format: yyyy-MM-dd HH:mm:ss zzz");
        }
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

    public DateTime ToUtcDateTime()
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
    
    public DateTime ToDateTime()
    {
        return _dateTimeOffset.DateTime;
    }

}
