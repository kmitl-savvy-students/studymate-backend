using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;
public class SdmTeachtableSubjectReview
{
    public static string TableName => "teachtable_subject_review";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<TeachtableSubjectReview> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);

        var result = new List<TeachtableSubjectReview>();

        while (query.Next())
        {
            result.Add(new TeachtableSubjectReview(
                SdmTeachtableSubject.GetById(query.ToInt(1)), // Foreign Key: teachtable_subject_id
                SdmUser.GetBy(query.ToString(2)),             // Foreign Key: user_id
                query.ToString(3),                           // Review
                query.ToFloat(4),                            // Rating
                query.ToInt(5),                              // Like
                query.ToInt(0)                               // Primary Key: id
            ));

            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }
    
    public static List<TeachtableSubjectReview> GetAll()
    {
        var select = GetQueryObj();
        
        var result = ProcessQuery(select, true);
        return result;
    }
    
    public static void Insert(TeachtableSubjectReview review)
    {
        try
        {
            Console.WriteLine($"Insert Review Debug: teachtable_subject_id={review.teachtable_subject?.id}, user_id={review.user?.id}, review={review.review}, rating={review.rating}, like={review.like}");

            if (review.teachtable_subject == null || review.teachtable_subject.id == 0)
            {
                throw new Exception("teachtable_subject is null or has invalid id.");
            }

            if (review.user == null || string.IsNullOrEmpty(review.user.id))
            {
                throw new Exception("User is null or has invalid id.");
            }

            var insert = new SdmPgsqlQueryInsert(TableName);

            insert.Insert("teachtable_subject_id", review.teachtable_subject.id.ToString());
            insert.Insert("user_id", review.user.id);
            insert.Insert("review", review.review);
            insert.Insert("rating", review.rating.ToString());
            insert.Insert("like", review.like.ToString());

            var query = SdmPgsqlQuery.Execute(insert);
            query.CleanUp();

            Console.WriteLine("TeachtableSubjectReview Inserted Successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Insert: {ex.Message}");
            throw;
        }
    }
    
    public static TeachtableSubjectReview? GetBySubjectAndStudent(string subjectId, string studentId)
    {
        try
        {
            // ดึง teachtable_subject_id จาก subjectId
            var selectSubject = new SdmPgsqlQuerySelect("teachtable_subject");
            selectSubject.AddWhereCondition("subject_id", subjectId);

            var subjectResult = SdmTeachtableSubject.ProcessQuery(selectSubject);
            if (subjectResult.Count == 0)
            {
                // Console.WriteLine($"TeachtableSubject not found for subjectId={subjectId}");
                return null;
            }

            var teachtableSubjectId = subjectResult[0].id;

            // Query teachtable_subject_review โดยใช้ teachtable_subject_id และ user_id
            var selectReview = GetQueryObj();
            selectReview.AddWhereCondition("teachtable_subject_id", teachtableSubjectId.ToString());
            selectReview.AddWhereCondition("user_id", studentId);

            var reviewResult = ProcessQuery(selectReview);
            if (reviewResult.Count == 0)
            {
                Console.WriteLine($"TeachtableSubjectReview not found for subjectId={subjectId}, studentId={studentId}");
                return null;
            }

            return reviewResult[0];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetBySubjectAndStudent: {ex.Message}");
            throw;
        }
    }
    
    public static void Update(string studentId, int year, int term, string subjectId, string review, float rating)
    {
        try
        {
            // 1. ตรวจสอบหรือสร้าง Teachtable
            var teachtable = SdmTeachtable.CheckOrCreate(year, term);
            Console.WriteLine($"Teachtable: id={teachtable?.id}, academic_year={teachtable?.academic_year}, academic_term={teachtable?.academic_term}");

            // 2. ตรวจสอบหรือสร้าง TeachtableSubject
            var teachableSubject = SdmTeachtableSubject.CheckOrCreate(teachtable.id, subjectId);
            if (teachableSubject == null || teachableSubject.id == 0)
            {
                throw new Exception("TeachtableSubject is null or has invalid id.");
            }
            Console.WriteLine($"TeachtableSubject: id={teachableSubject.id}, subject_id={teachableSubject.subject_id}");

            // 3. ดึง Review ที่ต้องการอัปเดต
            var reviewToUpdate = GetBySubjectAndStudent(subjectId, studentId);
            if (reviewToUpdate == null)
            {
                throw new Exception("TeachtableSubjectReview not found.");
            }

            // 4. อัปเดตข้อมูล
            reviewToUpdate.teachtable_subject = teachableSubject;
            reviewToUpdate.review = review;
            reviewToUpdate.rating = rating;

            var update = new SdmPgsqlQueryUpdate(TableName);

            update.Set("teachtable_subject_id", reviewToUpdate.teachtable_subject?.id.ToString() ?? "NULL");
            update.Set("user_id", reviewToUpdate.user?.id);
            update.Set("review", reviewToUpdate.review);
            update.Set("rating", reviewToUpdate.rating.ToString());
            update.Set("like", reviewToUpdate.like.ToString());

            update.WhereEqual("id", reviewToUpdate.id.ToString());

            var query = SdmPgsqlQuery.Execute(update);
            query.CleanUp();

            Console.WriteLine("TeachtableSubjectReview Updated Successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Update: {ex.Message}");
            throw;
        }
    }
    
    public static void Delete(string subjectId, string studentId)
    {
        try
        {
            // ดึง teachtable_subject_id จาก subjectId
            var selectSubject = new SdmPgsqlQuerySelect("teachtable_subject");
            selectSubject.AddWhereCondition("subject_id", subjectId);

            var subjectResult = SdmTeachtableSubject.ProcessQuery(selectSubject);
            if (subjectResult.Count == 0)
            {
                throw new Exception("TeachtableSubject not found.");
            }

            var teachtableSubjectId = subjectResult[0].id;

            // ลบข้อมูลใน teachtable_subject_review โดยใช้ teachtable_subject_id และ user_id
            var delete = new SdmPgsqlQueryDelete(TableName);
            delete.WhereEqual("teachtable_subject_id", teachtableSubjectId.ToString());
            delete.WhereEqual("user_id", studentId);

            var query = SdmPgsqlQuery.Execute(delete);
            query.CleanUp();

            Console.WriteLine("TeachtableSubjectReview Deleted Successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Delete: {ex.Message}");
            throw;
        }
    }
    
    public static void CreateReview(string studentId, int year, int term, string subjectId, string review, float rating)
    {
        Console.WriteLine($"Received Data: studentId={studentId}, year={year}, term={term}, subjectId={subjectId}, review={review}, rating={rating}");

        try
        {
            var teachtable = SdmTeachtable.CheckOrCreate(year, term);
            Console.WriteLine($"Teachtable: id={teachtable?.id}, academic_year={teachtable?.academic_year}, academic_term={teachtable?.academic_term}");

            var teachableSubject = SdmTeachtableSubject.CheckOrCreate(teachtable.id, subjectId);
            if (teachableSubject == null || teachableSubject.id == 0)
            {
                throw new Exception("TeachtableSubject is null or has invalid id.");
            }
            Console.WriteLine($"TeachtableSubject: id={teachableSubject.id}, subject_id={teachableSubject.subject_id}");

            var user = SdmUser.GetBy(studentId);
            if (user == null || string.IsNullOrEmpty(user.id))
            {
                throw new Exception("User is null or has invalid id.");
            }
            Console.WriteLine($"User: id={user.id}");

            var newReview = new TeachtableSubjectReview(
                teachtable_subject: teachableSubject,
                user: user,
                review: review,
                rating: rating,
                like: 0
            );
            Console.WriteLine($"TeachtableSubjectReview Created: teachtable_subject_id={newReview.teachtable_subject?.id}, user_id={newReview.user?.id}, review={newReview.review}, rating={newReview.rating}, like={newReview.like}");
            Insert(newReview);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateReview: {ex.Message}");
            throw;
        }
    }
    
}