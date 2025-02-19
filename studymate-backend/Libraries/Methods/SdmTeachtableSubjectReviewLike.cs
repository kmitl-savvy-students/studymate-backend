using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;
public class SdmTeachtableSubjectReviewLike : ISdmBaseMethod<TeachtableSubjectReviewLike>
{
    public static string TableName => "teachtable_subject_review_like";

    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }

    public static List<TeachtableSubjectReviewLike> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);
        var result = new List<TeachtableSubjectReviewLike>();
        while (query.Next())
        {
            result.Add(new TeachtableSubjectReviewLike(
                query.ToString(2),
                SdmTeachtableSubjectReview.GetById(query.ToInt(1)),
                query.ToInt(0)
                ));
        }
        query.CleanUp();
        return result;
    }

    public static TeachtableSubjectReviewLike GetByUserIdAndReviewId(string user_id, string teachtableSubjectReviewId)
    {
        var select = GetQueryObj();
        select.WhereEqual("tsrl_tsr_id", teachtableSubjectReviewId);
        select.WhereEqual("tsrl_user_id", user_id);

        var result = ProcessQuery(select);
        if (result.Count == 0)
        {
            return null;
        }
        return result[0];
    }

    public static List<TeachtableSubjectReviewLike> GetByReviewId(string teachtableSubjectReviewId)
    {
        var select = GetQueryObj();
        select.WhereEqual("tsrl_tsr_id", teachtableSubjectReviewId);

        var result = ProcessQuery(select);
        if (result.Count == 0)
        {
            return null;
        }
        Console.WriteLine($"result of like: {result.Count}");

        return result;
    }

    public static void Insert(TeachtableSubjectReviewLike reviewLike)
    {
        try
        {
            var insert = new SdmMysqlQueryInsert(TableName);
            insert.Insert("tsrl_tsr_id", reviewLike.TeachtableSubjectReview.Id.ToString());
            insert.Insert("tsrl_user_id", reviewLike.UserId);

            var query = SdmMysqlQuery.Execute(insert);
            query.CleanUp();
            Console.WriteLine("Like Successfully!");

            SdmTeachtableSubjectReview.UpdateLikeCount(reviewLike.TeachtableSubjectReview.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Insert: {ex.Message}");
            throw;
        }
    }

    public static void Delete(TeachtableSubjectReviewLike reviewLike)
    {
        try
        {
            var selectReview = new SdmMysqlQuerySelect("teachtable_subject_review");
            selectReview.AddWhereCondition("tsr_id", reviewLike.TeachtableSubjectReview.Id.ToString());

            var reviewResult = SdmTeachtableSubjectReview.ProcessQuery(selectReview, false);
            if (reviewResult.Count == 0)
            {
                throw new Exception("review not found.");
            }

            var delete = new SdmMysqlQueryDelete("teachtable_subject_review_like");
            delete.WhereEqual("tsrl_tsr_id", reviewLike.TeachtableSubjectReview.Id.ToString());
            delete.WhereEqual("tsrl_user_id", reviewLike.UserId);

            var query = SdmMysqlQuery.Execute(delete);
            query.CleanUp();
            Console.WriteLine($"deleted like of review teachtable_subject_review_id={reviewLike.TeachtableSubjectReview.Id}, user_id={reviewLike.UserId}");

            SdmTeachtableSubjectReview.UpdateLikeCount(reviewLike.TeachtableSubjectReview.Id);
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