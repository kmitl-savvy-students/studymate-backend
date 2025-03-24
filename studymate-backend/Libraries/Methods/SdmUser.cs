using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmUser : ISdmBaseMethod<User>
{
    public static string TableName => "user";
    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }
    public static List<User> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<User>();
        while (query.Next())
        {
            result.Add(new User(
                query.ToInt(0),
                query.ToString(1),
                query.ToString(2),
                query.ToString(3),
                query.ToString(4),
                query.ToString(5),
                query.ToBool(6),
                SdmCurriculum.GetBy(query.ToInt(7)),
                query.ToInt(8)
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }

    public static User? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("u_id", id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }

    public static void Insert(User user)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("u_id", user.Id.ToString());
        insert.Insert("u_password", user.Password);
        insert.Insert("u_nickname", user.Nickname);
        insert.Insert("u_firstname", user.Firstname);
        insert.Insert("u_lastname", user.Lastname);
        insert.Insert("u_profile_pic", user.ProfilePicture);
        insert.Insert("u_curr_id", user.Curriculum?.Id.ToString());

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    public static void UpdateBy(User user)
    {
        var update = new SdmMysqlQueryUpdate(TableName);

        update.Set("u_nickname", user.Nickname);
        update.Set("u_firstname", user.Firstname);
        update.Set("u_lastname", user.Lastname);
        update.Set("u_profile_pic", user.ProfilePicture);
        update.Set("u_curr_id", user.Curriculum?.Id.ToString());

        update.WhereEqual("u_id", user.Id.ToString());

        var query = SdmMysqlQuery.Execute(update);
        query.CleanUp();
    }

    public static bool Verify(User user, string otpId)
    {
        var otpAuth = SdmOtpAuthentication.GetById(otpId);
        if (otpAuth == null || otpAuth.DateExpired.ToDateTime() < DateTime.UtcNow) 
        { 
            Console.WriteLine("❌ Not found OTP or expired"); 
            return false;
        }

        if (otpAuth.UserId != user.Id)
        {
            Console.WriteLine("❌ Not Your OTP");
            return false;
        }
        
        if (otpAuth.Status != "VERIFIED")
        {
            Console.WriteLine("❌ OTP is not verified");
            return false;
        }
        
        Insert(user);
        return true;
    }

    public static void UpdateViewPolicy(User user)
    {
        var update = new SdmMysqlQueryUpdate(TableName);

        if (user.ViewPolicy == 0)
        {
            update.Set("u_policy_viewed", "1");
            update.WhereEqual("u_id", user.Id.ToString());
            var query = SdmMysqlQuery.Execute(update);
            query.CleanUp();
        }
    }

    public static int GetViewPolicy(int userId)
    {
        var select = GetQueryObj();
        select.WhereEqual("u_id", userId.ToString());
        
        var result = ProcessQuery(select);
        if (result.Count == 0)
        {
            Console.WriteLine("Not user");
            return 0;
        }
        return result[0].ViewPolicy;
    }
    
    public static User? GetUserInfoFromToken(string token)
    {
        // ใช้ SdmUserToken.GetBy เพื่อดึง UserToken จาก Token ID
        var userToken = SdmUserToken.GetBy(token);
        if (userToken == null)
        {
            Console.WriteLine("[Error] Token not found or invalid.");
            return null;
        }

        // ตรวจสอบว่า Token หมดอายุหรือไม่
        if (userToken.DateExpired.ToDateTime() < DateTime.Now)
        {
            Console.WriteLine("[Error] Token has expired.");
            return null;
        }

        // คืนค่า User ที่เชื่อมโยงกับ Token
        return userToken.User;
    }
    
}