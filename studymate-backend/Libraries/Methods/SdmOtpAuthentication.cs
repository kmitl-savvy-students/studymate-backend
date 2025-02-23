using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;
using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Helper;
using System.Globalization;
using System.Text;

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
                query.ToString(4),
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
        insert.Insert("otpa_status", otpAuthentication.Status);
        insert.Insert("otpa_date_created", otpAuthentication.DateCreated.ToString());
        insert.Insert("otpa_date_expired", otpAuthentication.DateExpired.ToString());
        
        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    
    private static string GenerateId(int length)
    {
        if (length <= 0) throw new ArgumentException("Length must be greater than 0");

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        string id;
    
        do
        {
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[RandomNumberGenerator.GetInt32(chars.Length)]);
            }
            id = sb.ToString();

            Console.WriteLine($"🔍 Generated ID: {id} (Length: {id.Length})"); // ✅ Debug เพื่อเช็คความยาว

        } while (CheckExists(id)); 

        return id;
    }
    
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
        if (SdmUser.GetBy(userId) != null)
        {
            throw new Exception("Cannot request OTP. User already exists.");
        }
        
        // ดึง OTP ล่าสุดของ userId
        var lastOtp = GetLastOtpForUser(userId);
    
        if (lastOtp != null)
        {
            var timeElapsed = (DateTime.UtcNow - lastOtp.DateCreated.ToDateTime()).TotalSeconds;
            if (timeElapsed < 60)
            {
                throw new Exception("คุณขอ OTP บ่อยเกินไป กรุณารอสักครู่ก่อนลองใหม่อีกครั้งค่ะ");
            }
        }
        
        string otpaId = GenerateId(64); // Unique 64 char
        string otpaCode = GenerateOTPCode(); // 6-digit OTP
        string otpaReferer = GenerateUniqueReferer(); // Unique referer
        string otpaStatus = "UNVERIFIED";
        
        SdmDateTime dateCreated = new SdmDateTime(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        SdmDateTime dateExpired = new SdmDateTime(DateTime.UtcNow.AddMinutes(5).ToString("yyyy-MM-dd HH:mm:ss"));
        
        var otpAuthentication = new OtpAuthentication(
            otpaId, userId, int.Parse(otpaCode), otpaReferer, otpaStatus, dateCreated, dateExpired
        );
        
        Insert(otpAuthentication);
        
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
            
            string logoUrl = "https://drive.google.com/uc?export=view&id=1Huf2Jjo_YgbWa1FiEZUPofFpCVA6ruUn"; 

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = "Your StudyMate OTP Code",
                IsBodyHtml = true,
                Body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 480px; margin: auto; padding: 20px; text-align: center; background: #F5F5F5; border-radius: 8px;'>
                        <img src='{logoUrl}' alt='StudyMate' style='width: 100px; margin-bottom: 10px;'>
                        <h2 style='color: #333;'>Let's sign you up</h2>
                        <p style='color: #666;'>Use this code to sign up to StudyMate. This code will expire in 5 minutes.</p>
                        <h1 style='font-size: 32px; letter-spacing: 5px;'>{otpCode}</h1>
                        <p style='color: #666;'>Referer: <b>{referer}</b></p>
                        <p style='color: #666;'>This code will securely sign you up using <b>{toEmail}</b></p>
                        <p style='font-size: 12px; color: #999;'>If you didn’t request this email, you can safely ignore it.</p>
                    </div>"
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
        string now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        
        select.WhereRaw($"WHERE otpa_date_expired >= '{now}'");

        var otps = ProcessQuery(select, true);
    
        foreach (var otp in otps)
        {
            Console.WriteLine($"[DEBUG] OTP Referer: {otp.Referer} | Expired At: {otp.DateExpired.ToDateTime():yyyy-MM-dd HH:mm:ss}");
        }

        return otps;
    }
    
    public static OtpAuthentication? VerifyOTP(string id, string otpaCode)
    {
        var select = GetQueryObj();
        select.WhereEqual("otpa_id", id);

        var results = ProcessQuery(select);

        // ✅ ป้องกัน `results` เป็น `null`
        if (results == null || results.Count == 0)
        {
            Console.WriteLine("❌ Id not found.");
            return null;
        }

        var result = results.FirstOrDefault();

        // ✅ ป้องกัน `result` เป็น `null`
        if (result == null)
        {
            Console.WriteLine("❌ OTP record is missing.");
            return null;
        }

        var expiredDateTime = result.DateExpired.ToDateTime();
        var nowDateTime = DateTime.UtcNow;

        if (expiredDateTime < nowDateTime || result.Code.ToString() != otpaCode)
        {
            Console.WriteLine("❌ Invalid OTP or expired.");
            return null;
        }

        Console.WriteLine($"✅ Final result.Status = '{result.Status}'");

        if (result.Status == "VERIFIED")
        {
            Console.WriteLine("❌ OTP status is VERIFIED already.");
            return result;
        }

        Console.WriteLine($"🔍 result.DateExpired (converted) = {expiredDateTime:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"🔍 nowDateTime = {nowDateTime:yyyy-MM-dd HH:mm:ss}");

        Console.WriteLine("🔍 OTP Record Found:");
        Console.WriteLine($"   ✅ otpa_id = {result.Id}");
        Console.WriteLine($"   ✅ otpa_code = {result.Code}");
        Console.WriteLine($"   ✅ otpa_date_expired = {expiredDateTime:yyyy-MM-dd HH:mm:ss}");

        var update = new SdmMysqlQueryUpdate("otp_authentication");
        update.Set("otpa_status", "VERIFIED");
        update.WhereEqual("otpa_id", result.Id);

        var query = SdmMysqlQuery.Execute(update);
        query.CleanUp();
        Console.WriteLine("✅ OTP Verified Successfully!");
        return result;
    }
    
    public static OtpAuthentication? GetLastOtpForUser(int userId)
    {
        var select = GetQueryObj();
    
        // ✅ ใช้ WhereEqual() เพื่อป้องกัน SQL Syntax Error
        select.WhereEqual("otpa_user_id", userId.ToString());
    
        var results = ProcessQuery(select, true);

        // ✅ เรียงลำดับข้อมูลใน C# แทน ORDER BY ใน SQL
        var lastOtp = results.OrderByDescending(o => o.DateCreated.ToDateTime()).FirstOrDefault();

        return lastOtp;
    }

    public static OtpAuthentication? GetById(string id)
    {
        var select = GetQueryObj();
        select.WhereEqual("otpa_id", id);
        
        var results = ProcessQuery(select);
        return results.Count == 0 ? null : results[0];
    }

}