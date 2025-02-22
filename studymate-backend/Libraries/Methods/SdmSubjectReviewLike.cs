using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;
public class SdmSubjectReviewLike : ISdmBaseMethod<SubjectReviewLike>
{
    public static string TableName => "subject_review_like";

    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }

    public static List<SubjectReviewLike> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);
        var result = new List<SubjectReviewLike>();
        while (query.Next())
        {
            result.Add(new SubjectReviewLike(
                query.ToInt(2),
                SdmSubjectReview.GetById(query.ToInt(1)),
                query.ToInt(0)
                ));
        }
        query.CleanUp();
        return result;
    }

    public static SubjectReviewLike GetByUserIdAndReviewId(int user_id, string subjectReviewId)
    {
        var select = GetQueryObj();
        select.WhereEqual("srl_sbjr_id", subjectReviewId);
        select.WhereEqual("srl_user_id", user_id.ToString());

        var result = ProcessQuery(select);
        if (result.Count == 0)
        {
            return null;
        }
        return result[0];
    }

    public static List<SubjectReviewLike> GetByReviewId(string subjectReviewId)
    {
        var select = GetQueryObj();
        select.WhereEqual("srl_sbjr_id", subjectReviewId);

        var result = ProcessQuery(select);
        if (result.Count == 0)
        {
            return null;
        }
        Console.WriteLine($"result of like: {result.Count}");

        return result;
    }

    public static void Insert(SubjectReviewLike reviewLike)
    {
        try
        {
            var insert = new SdmMysqlQueryInsert(TableName);
            insert.Insert("srl_sbjr_id", reviewLike.SubjectReview.Id.ToString());
            insert.Insert("srl_user_id", reviewLike.UserId.ToString());

            var query = SdmMysqlQuery.Execute(insert);
            query.CleanUp();
            Console.WriteLine("Like Successfully!");

            SdmSubjectReview.UpdateLikeCount(reviewLike.SubjectReview.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Insert: {ex.Message}");
            throw;
        }
    }

    public static void Delete(SubjectReviewLike reviewLike)
    {
        try
        {
            var selectReview = new SdmMysqlQuerySelect("subject_review");
            selectReview.AddWhereCondition("sbjr_id", reviewLike.SubjectReview.Id.ToString());

            var reviewResult = SdmSubjectReview.ProcessQuery(selectReview, false);
            if (reviewResult.Count == 0)
            {
                throw new Exception("review not found.");
            }

            var delete = new SdmMysqlQueryDelete("subject_review_like");
            delete.WhereEqual("srl_sbjr_id", reviewLike.SubjectReview.Id.ToString());
            delete.WhereEqual("srl_user_id", reviewLike.UserId.ToString());

            var query = SdmMysqlQuery.Execute(delete);
            query.CleanUp();
            Console.WriteLine($"deleted like of review subject_review_id={reviewLike.SubjectReview.Id}, user_id={reviewLike.UserId}");

            SdmSubjectReview.UpdateLikeCount(reviewLike.SubjectReview.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Delete: {ex.Message}");
            throw;
        }
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