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
        var randomNumber = new byte[4];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        return (BitConverter.ToUInt32(randomNumber, 0) % 1000000).ToString("D6");
    }
    
    // ฟังก์ชัน Generate Unique Referer โดยเช็คว่าซ้ำในของ userId หรือไม่
    //  private static string GenerateUniqueReferer(int userId)
    //  {
    //      string referer;
    //      var existingReferers = new HashSet<string>();
    //
    //      // ใช้ GetQueryObj() ตามโครงสร้างของ Query Builder
    //      var selectQuery = SdmOtpAuthentication.GetQueryObj();
    //      selectQuery.Where("otpa_user_id", "=", userId.ToString()); // ตรวจสอบ user_id
    // /////////////////// น่าจะผิดตรงนี้ไม่ควรใช้ now
    //      selectQuery.Where("otpa_date_expired", "<", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")); // ตรวจสอบวันหมดอายุ
    //      // selectQuery.Where("otpa_date_expired", "<", DateTime.UtcNow.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss"));
    //      
    //      var expiryThreshold = DateTime.UtcNow.AddMinutes(-5);
    //      selectQuery.Where("otpa_date_expired", ">", expiryThreshold.ToString("yyyy-MM-dd HH:mm:ss"));
    //      
    //      Console.WriteLine($"Expiry Threshold: {expiryThreshold:yyyy-MM-dd HH:mm:ss} | Now: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
    //
    //
    //      
    //      var queryResult = SdmOtpAuthentication.ProcessQuery(selectQuery, true);
    //
    //      foreach (var otp in queryResult)
    //      {
    //          existingReferers.Add(otp.Referer);
    //          Console.WriteLine($"DB Referer: {otp.Referer} | OTP: {otp.Code} | Expired At: {otp.DateExpired} | Now: {DateTime.UtcNow}");
    //      }
    //
    //      // วนลูปจนกว่าจะได้ referer ที่ไม่ซ้ำ
    //      do
    //      {
    //          referer = GenerateId(4);
    //      } while (existingReferers.Contains(referer));
    //
    //      return referer;
    //  }
    
    
    
    
    
    private static string GenerateUniqueReferer(int userId)
    {
        string referer;
        var existingReferers = new HashSet<string>();

        // ใช้ DateTime ปัจจุบันเป็น UTC
        var expiryThreshold = DateTime.UtcNow;

        // สร้าง Query เพื่อดึงเฉพาะ OTP ที่ยังไม่หมดอายุ
        var selectQuery = SdmOtpAuthentication.GetQueryObj();
        selectQuery.Where("otpa_user_id", "=", userId.ToString());
        selectQuery.Where("otpa_date_expired", ">=", expiryThreshold); // เปรียบเทียบเป็น DateTime

        Console.WriteLine($"Expiry Threshold: {expiryThreshold:yyyy-MM-dd HH:mm:ss}");

        // ประมวลผล Query
        var queryResult = SdmOtpAuthentication.ProcessQuery(selectQuery, true);

        // เก็บ Referer ที่มีอยู่แล้ว
        foreach (var otp in queryResult)
        {
            // แปลง SdmDateTime เป็น System.DateTime
            DateTime dateExpired = otp.DateExpired.ToDateTime(); // ถ้าใช้ไม่ได้ให้ลองวิธีอื่นด้านล่าง

            Console.WriteLine($"DB Referer: {otp.Referer} | OTP: {otp.Code} | Expired At: {dateExpired:yyyy-MM-dd HH:mm:ss} | Now At: {expiryThreshold}");

            if (dateExpired >= expiryThreshold) // เปรียบเทียบได้แล้ว
            {
                Console.WriteLine($"Add: {otp.Referer} | OTP: {otp.Code} | Expired At: {dateExpired:yyyy-MM-dd HH:mm:ss}");
                existingReferers.Add(otp.Referer);
            }
        }

        // วนลูปจนกว่าจะได้ referer ที่ไม่ซ้ำ
        do
        {
            referer = GenerateId(4);
        } while (existingReferers.Contains(referer));

        return referer;
    }

    
    
    
    
    
    
    // private static string GenerateUniqueReferer(int userId)
    // {
    //     string referer;
    //     var existingReferers = new HashSet<string>();
    //
    //     // ใช้ DateTime ปัจจุบัน แปลงเป็น string ตามรูปแบบ MySQL (YYYY-MM-DD HH:mm:ss)
    //     var expiryThreshold = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    //
    //     // สร้าง Query เพื่อดึงเฉพาะ OTP ที่ยังไม่หมดอายุ
    //     var selectQuery = GetQueryObj();
    //     selectQuery.Where("otpa_user_id", "=", userId.ToString());
    //     selectQuery.Where("otpa_date_expired", ">=", expiryThreshold); // ใช้ Where() แทน WhereRaw()
    //     
    //     // Console.WriteLine($"Expiry Threshold: {expiryThreshold} | Now: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}");
    //
    //     // ประมวลผล Query
    //     var queryResult = ProcessQuery(selectQuery, true);
    //
    //     Console.WriteLine($"Expiry Threshold: {expiryThreshold}");
    //     // เก็บ Referer ที่มีอยู่แล้ว
    //     foreach (var otp in queryResult)
    //     {
    //         Console.WriteLine($"DB Referer: {otp.Referer} | OTP: {otp.Code} | Expired At: {otp.DateExpired} | Now: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}");
    //         existingReferers.Add(otp.Referer);
    //     }
    //
    //     // วนลูปจนกว่าจะได้ referer ที่ไม่ซ้ำ
    //     do
    //     {
    //         referer = GenerateId(4);
    //     } while (existingReferers.Contains(referer));
    //
    //     return referer;
    // }

    
    // ฟังก์ชันสร้าง OTP Request และบันทึกลง DB
    public static OtpAuthentication RequestOtp(int userId)
    {
        string otpaId = GenerateId(64); // Unique 64 char
        string otpaCode = GenerateOTPCode(); // 6-digit OTP
        string otpaReferer = GenerateUniqueReferer(userId); // Unique referer
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
    
    // public static List<OtpAuthentication> GetActiveOtps()
    // {
    //     var select = GetQueryObj();
    //
    //     // ใช้ DateTime.UtcNow เป็นค่าตั้งต้น
    //     DateTime expiryThreshold = DateTime.UtcNow;
    //
    //     // ดึงเฉพาะ OTP ที่ยังไม่หมดอายุ
    //     select.Where("otpa_date_expired", ">=", expiryThreshold.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
    //
    //     var otps = ProcessQuery(select, true);
    //
    //     // ตรวจสอบว่า OTP ที่ได้มีวันหมดอายุที่ถูกต้อง
    //     List<OtpAuthentication> activeOtps = new List<OtpAuthentication>();
    //
    //     Console.WriteLine($"[DEBUG] Now UTC: {expiryThreshold:yyyy-MM-dd HH:mm:ss}");
    //
    //     foreach (var otp in otps)
    //     {
    //         DateTime otpExpiry = otp.DateExpired.ToDateTime(); // ✅ แปลงจาก SdmDateTime เป็น DateTime
    //
    //         Console.WriteLine($"[DEBUG] OTP: {otp.Code} | Expired At: {otpExpiry:yyyy-MM-dd HH:mm:ss} | Now: {expiryThreshold:yyyy-MM-dd HH:mm:ss}");
    //
    //         // ตรวจสอบว่าค่าที่ได้ถูกต้อง
    //         if (otpExpiry >= expiryThreshold)
    //         {
    //             activeOtps.Add(otp);
    //         }
    //         else
    //         {
    //             Console.WriteLine($"[ERROR] ❌ OTP นี้หมดอายุแล้ว แต่ยังถูกดึงมา: {otp.Code}");
    //         }
    //     }
    //
    //     return activeOtps;
    // }
    
    public static List<OtpAuthentication> GetActiveOtps()
    {
        var select = GetQueryObj();

        // ใช้ DateTime.UtcNow และให้ MySQL จัดการเปรียบเทียบโดยตรง
        string now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    
        // ใช้ WhereRaw() เพื่อให้ MySQL เปรียบเทียบค่าโดยตรง
        select.WhereRaw($"otpa_date_expired >= STR_TO_DATE('{now}', '%Y-%m-%d %H:%i:%s')");

        var otps = ProcessQuery(select, true);

        Console.WriteLine($"[DEBUG] Now UTC: {now}");
    
        foreach (var otp in otps)
        {
            Console.WriteLine($"[DEBUG] OTP Referer: {otp.Referer} | Expired At: {otp.DateExpired.ToDateTime():yyyy-MM-dd HH:mm:ss}");
        }

        return otps;
    }

    
}

