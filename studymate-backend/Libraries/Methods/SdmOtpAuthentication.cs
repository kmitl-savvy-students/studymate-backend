using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;
using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Helper;
using System.Globalization;

namespace studymate_backend.Libraries.Methods;

public class SdmOtpAuthentication
{
    public static string TableName => "otp_authentication";

    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }

    public static List<OtpAuthentication> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<OtpAuthentication>();
        while (query.Next())
        {
            result.Add(new OtpAuthentication(
                query.ToString(0),
                query.ToInt(1),
                query.ToInt(2),
                query.ToString(3),
                query.ToDateTime(4),
                new SdmDateTime(query.ToDateTime(5)),
                new SdmDateTime(query.ToDateTime(6))
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static void Insert(OtpAuthentication otpAuthentication)
    {
        var insert = new SdmMysqlQueryInsert(TableName);
        
        insert.Insert("otpa_id", otpAuthentication.Id);
        insert.Insert("otpa_user_id", otpAuthentication.UserId.ToString());
        insert.Insert("otpa_code", otpAuthentication.Code.ToString());
        insert.Insert("otpa_referer", otpAuthentication.Referer);
        insert.Insert("otpa_status", otpAuthentication.Status.ToString());
        insert.Insert("otpa_date_created", otpAuthentication.DateCreated.ToString());
        insert.Insert("otpa_date_expired", otpAuthentication.DateExpired.ToString());
        
        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    
    // ฟังก์ชัน Generate Unique Random String ที่ไม่ซ้ำใน Table otp_authentication
    private static string GenerateId(int length)
    {
        string id;
        do
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            id = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[RandomNumberGenerator.GetInt32(s.Length)]).ToArray());
        } 
        while (CheckExists(id)); // ตรวจสอบว่ามีค่า otpa_id ใน table แล้วหรือไม่
        
        return id;
    }
    
    // ฟังก์ชันตรวจสอบว่า ค่ามีอยู่ใน Table otp_authentication แล้วหรือไม่
    public static bool CheckExists(string id)
    {
        var select = GetQueryObj();
        select.WhereEqual("otpa_id", id);
        
        var result = ProcessQuery(select);

        if (result.Count == 0)
        {
            return false;
        }
        return true;
    }
    
    // ฟังก์ชัน Generate OTP 6 หลัก
    public static string GenerateOTPCode()
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            var randomNumber = new byte[4];
            rng.GetBytes(randomNumber);
            uint otp = BitConverter.ToUInt32(randomNumber, 0) % 1000000;
            return otp.ToString().PadRight(6, '0'); // เติม '0' ด้านหลังให้ครบ 6 หลัก
        }
    }
    
    private static string GenerateUniqueReferer()
    {
        var select = GetQueryObj();
        string referer;
        
        var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        Console.WriteLine($"Now Datetime: {now}");
        
        select.WhereRaw($"WHERE otpa_date_expired >= '{now}'");
        
        // ดึงเฉพาะค่าที่ต้องการจาก OtpAuthentication
        var otpNotExpired = ProcessQuery(select, true);
        
        Console.WriteLine("🔍 Checking otpNotExpired contents:");
        foreach (var otp in otpNotExpired)
        {
            Console.WriteLine($"✅ Referer: {otp.Referer} | Expired At: {otp.DateExpired:yyyy-MM-dd HH:mm:ss}");
        }
        
        // วนลูปจนกว่าจะได้ referer ที่ไม่ซ้ำ
        do
        {
            referer = GenerateId(4);
        } while (otpNotExpired.Any(otp => otp.Referer == referer));

        return referer;
    }
    
    // ฟังก์ชันสร้าง OTP Request และบันทึกลง DB
    public static OtpAuthentication RequestOtp(int userId)
    {
        string otpaId = GenerateId(64); // Unique 64 char
        string otpaCode = GenerateOTPCode(); // 6-digit OTP
        string otpaReferer = GenerateUniqueReferer(); // Unique referer
        string otpaStatus = "UNVERIFIED";

        // ใช้ DateTime.UtcNow + ToString()
        SdmDateTime dateCreated = new SdmDateTime(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        SdmDateTime dateExpired = new SdmDateTime(DateTime.UtcNow.AddMinutes(5).ToString("yyyy-MM-dd HH:mm:ss"));

        // สร้าง Object OtpAuthentication
        var otpAuthentication = new OtpAuthentication(
            otpaId, userId, int.Parse(otpaCode), otpaReferer, otpaStatus, dateCreated, dateExpired
        );

        // ใช้เมธอด Insert() เพื่อบันทึกข้อมูลลงฐานข้อมูล
        Insert(otpAuthentication);

        // ส่งอีเมลไปที่ user_id@kmitl.ac.th
        string email = $"{userId}@kmitl.ac.th";
        SendEmail(email, otpaCode, otpaReferer);

        return otpAuthentication;
    }
    
    public static List<OtpAuthentication> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);
        return result;
    }
    
    // ฟังก์ชันส่งอีเมล
    public static void SendEmail(string toEmail, string otpCode, string referer)
    {
        var fromEmail = Environment.GetEnvironmentVariable("SMTP_EMAIL");
        var fromPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

        if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromPassword))
        {
            throw new Exception("❌ SMTP credentials are missing. Please set SMTP_EMAIL and SMTP_PASSWORD.");
        }

        try
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = "Your StudyMate OTP Code",
                Body = $@"
                    <h3>Your OTP Code</h3>
                    <p><b>OTP:</b> {otpCode}</p>
                    <p><b>Referer:</b> {referer}</p>
                    <p>This code is valid for 5 minutes.</p>",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            smtpClient.Send(mailMessage);
            Console.WriteLine("✅ OTP email sent successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send OTP email: {ex.Message}");
            throw new Exception("Failed to send OTP email.", ex);
        }
    }
    
    public static List<OtpAuthentication> GetActiveOtps()
    {
        var select = GetQueryObj();

        // ใช้ DateTime.UtcNow และให้ MySQL จัดการเปรียบเทียบโดยตรง
        string now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    
        // ใช้ WhereRaw() เพื่อให้ MySQL เปรียบเทียบค่าโดยตรง
        // select.WhereRaw($"otpa_date_expired >= STR_TO_DATE('{now}', '%Y-%m-%d %H:%i:%s')");
        select.WhereRaw($"WHERE otpa_date_expired >= '{now}'");

        var otps = ProcessQuery(select, true);

        // Console.WriteLine($"[DEBUG] Now UTC: {now}");
    
        foreach (var otp in otps)
        {
            Console.WriteLine($"[DEBUG] OTP Referer: {otp.Referer} | Expired At: {otp.DateExpired.ToDateTime():yyyy-MM-dd HH:mm:ss}");
        }

        return otps;
    }
    
}