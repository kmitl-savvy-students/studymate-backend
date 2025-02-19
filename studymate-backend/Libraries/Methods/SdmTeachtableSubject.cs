using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmTeachtableSubject : ISdmBaseMethod<TeachtableSubject>
{
    public static string TableName => "teachtable_subject";

    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }

    public static List<TeachtableSubject> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<TeachtableSubject>();

        while (query.Next())
        {
            result.Add(new TeachtableSubject(
                SdmTeachtable.GetBy(query.ToInt(1)),
                query.ToString(2),
                query.ToInt(3),
                query.ToFloat(4),
                query.ToInt(5),
                query.ToInt(0)
                
            ));
            if (!isArray) break;
        }
        query.CleanUp();
        return result;
    }
    
    public static void Insert(TeachtableSubject teachtableSubject)
    {
        var insert = new SdmMysqlQueryInsert("teachtable_subject");

        insert.Insert("tts_tt_id", teachtableSubject.Teachtable?.Id.ToString());
        insert.Insert("tts_sbj_id", teachtableSubject.SubjectId);
        insert.Insert("tts_int", teachtableSubject.Interested.ToString());
        insert.Insert("tts_rat", teachtableSubject.Rating.ToString());
        insert.Insert("tts_cor", teachtableSubject.CountOfReview.ToString());

        Console.WriteLine($"Inserting TeachtableSubject: teachtable_id={teachtableSubject.Teachtable?.Id}, subject_id={teachtableSubject.SubjectId}");
        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    
    public static TeachtableSubject? GetById(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("tts_id", id.ToString());
        
        var  result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }
    
    public static TeachtableSubject CheckOrCreate(int teachtableId, string subjectId)
    {
        
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            throw new ArgumentException("Invalid subjectId. SubjectId cannot be empty or null.");
        }
        
        try
        {
            Console.WriteLine($"CheckOrCreate: Checking teachtable_subject with teachtable_id={teachtableId}, subject_id={subjectId}");

            // Query TeachtableSubject
            var select = new SdmMysqlQuerySelect("teachtable_subject")
                .AddWhereCondition("tts_tt_id", teachtableId.ToString())
                .AddWhereCondition("tts_sbj_id", subjectId);

            var result = ProcessQuery(select);
            if (result.Count > 0)
            {
                Console.WriteLine($"Found TeachtableSubject: id={result[0].Id}");
                return result[0];
            }

            // Create New TeachtableSubject
            Console.WriteLine($"Creating new TeachtableSubject for teachtable_id={teachtableId}, subject_id={subjectId}");
            var newTeachtableSubject = new TeachtableSubject(
                teachtable: SdmTeachtable.GetBy(teachtableId),
                subjectId: subjectId,
                interested: 0,
                rating: 0.0f,
                countOfReview: 0
            );
            Insert(newTeachtableSubject);

            // Re-query after Insert
            var newResult = ProcessQuery(select);
            if (newResult.Count > 0)
            {
                Console.WriteLine($"Created TeachtableSubject: id={newResult[0].Id}");
                return newResult[0];
            }

            throw new Exception("Failed to retrieve TeachtableSubject after Insert.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CheckOrCreate: {ex.Message}");
            throw;
        }
    }
    
    public static void UpdateReviewStats(string subjectId)
    {
        try
        {
            // ดึง teachtable_subject ที่ตรงกับ subjectId
            var selectSubject = new SdmMysqlQuerySelect("teachtable_subject");
            selectSubject.AddWhereCondition("tts_sbj_id", subjectId);
            var subjectList = ProcessQuery(selectSubject, true);

            foreach (var teachtableSubject in subjectList)
            {
                // ดึงจำนวนรีวิวจาก teachtable_subject_review
                var selectReview = new SdmMysqlQuerySelect("teachtable_subject_review");
                selectReview.AddWhereCondition("tsr_tts_id", teachtableSubject.Id.ToString());

                var reviews = SdmTeachtableSubjectReview.ProcessQuery(selectReview, true);
                int countOfReview = reviews.Count;
                float averageRating = countOfReview > 0 ? reviews.Average(r => r.Rating) : 0.0f;

                // ตรวจสอบว่า countOfReview มีค่ามากกว่า 0 หรือไม่
                Console.WriteLine($"Updating SubjectId: {subjectId} -> countOfReview: {countOfReview}, averageRating: {averageRating}");

                // อัปเดตค่าในฐานข้อมูล
                var update = new SdmMysqlQueryUpdate("teachtable_subject");
                update.Set("tts_cor", countOfReview.ToString());
                update.Set("tts_rat", averageRating.ToString("0.00"));
                update.WhereEqual("tts_id", teachtableSubject.Id.ToString());

                var query = SdmMysqlQuery.Execute(update);
                query.CleanUp();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateReviewStats: {ex.Message}");
            throw;
        }
    }

    
    public static (int countOfReview, float averageRating)? GetReviewStats(string subjectId)
    {
        // ดึง teachtable_subject_id ที่ตรงกับ subjectId
        var selectSubject = new SdmMysqlQuerySelect("teachtable_subject");
        selectSubject.AddWhereCondition("tts_sbj_id", subjectId);
        var subjectList = ProcessQuery(selectSubject, true);

        if (subjectList.Count == 0)
            return null;

        // รวมรีวิวทั้งหมดจาก teachtable_subject_review
        int totalReviews = 0;
        float totalRating = 0;

        foreach (var subject in subjectList)
        {
            var selectReview = new SdmMysqlQuerySelect("teachtable_subject_review");
            selectReview.AddWhereCondition("tsr_tts_id", subject.Id.ToString());

            var reviews = SdmTeachtableSubjectReview.ProcessQuery(selectReview, true);
            totalReviews += reviews.Count;
            totalRating += reviews.Sum(r => r.Rating);
        }

        // คำนวณค่าเฉลี่ยเรตติ้ง
        float avgRating = totalReviews > 0 ? totalRating / totalReviews : 0.0f;

        return (totalReviews, avgRating);
    }


}