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
    
    public static TeachtableSubjectReview? GetById(int id)
    {
        var select = GetQueryObj();
        select.AddWhereCondition("id", id.ToString());

        var result = ProcessQuery(select);
        if (result.Count == 0) 
            return null;
        return result[0];
    }

    // public static void Insert(TeachtableSubjectReview review)
    // {
    //     var insert = new SdmPgsqlQueryInsert(TableName);
    //
    //     insert.Insert("teachtable_subject_id", review.teachtable_subject?.id.ToString());
    //     insert.Insert("user_id", review.user?.id);
    //     insert.Insert("review", review.review);
    //     insert.Insert("rating", review.rating.ToString());
    //     insert.Insert("like", review.like.ToString());
    //
    //     var query = SdmPgsqlQuery.Execute(insert);
    //     query.CleanUp();
    // }
    
    // public static void Insert(TeachtableSubjectReview review)
    // {
    //     try
    //     {
    //         if (review.teachtable_subject == null || review.teachtable_subject.id == 0)
    //         {
    //             throw new Exception("teachtable_subject is null or has invalid id.");
    //         }
    //
    //         if (review.user == null || string.IsNullOrEmpty(review.user.id))
    //         {
    //             throw new Exception("User is null or has invalid id.");
    //         }
    //
    //         var insert = new SdmPgsqlQueryInsert(TableName);
    //
    //         Console.WriteLine($"Insert Review Details: teachtable_subject_id={review.teachtable_subject.id}, user_id={review.user.id}, review={review.review}, rating={review.rating}, like={review.like}");
    //
    //         insert.Insert("teachtable_subject_id", review.teachtable_subject.id.ToString());
    //         insert.Insert("user_id", review.user.id);
    //         insert.Insert("review", review.review);
    //         insert.Insert("rating", review.rating.ToString());
    //         insert.Insert("like", review.like.ToString());
    //
    //         var query = SdmPgsqlQuery.Execute(insert);
    //         query.CleanUp();
    //
    //         Console.WriteLine("TeachtableSubjectReview Inserted Successfully!");
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error in Insert1: {review.teachtable_subject.id.ToString()}");
    //         Console.WriteLine($"Error in Insert2: {ex.Message}");
    //         throw;
    //     }
    // }
    
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

    
    public static void Update(TeachtableSubjectReview review)
    {
        var update = new SdmPgsqlQueryUpdate(TableName);

        update.Set("teachtable_subject_id", review.teachtable_subject?.id.ToString() ?? "NULL");
        update.Set("user_id", review.user?.id);
        update.Set("review", review.review);
        update.Set("rating", review.rating.ToString());
        update.Set("like", review.like.ToString());

        update.WhereEqual("id", review.id.ToString());

        var query = SdmPgsqlQuery.Execute(update);
        query.CleanUp();
    }

    public static void Delete(TeachtableSubjectReview review)
    {
        var delete = new SdmPgsqlQueryDelete(TableName);
        
        delete.WhereEqual("id", review.id.ToString());

        var query = SdmPgsqlQuery.Execute(delete);
        query.CleanUp();
    }
    
    // public static void CreateReview(string studentId, int year, int term, string subjectId, string review, float rating)
    // {
    //     try
    //     {
    //         // 1. ตรวจสอบหรือสร้าง Teachtable
    //         var teachtable = SdmTeachtable.CheckOrCreate(year, term);
    //
    //         // Log ค่าที่ใช้เรียก CheckOrCreate
    //         Console.WriteLine($"Calling CheckOrCreate with teachtable_id={teachtable.id}, subject_id={subjectId}");
    //
    //         // 2. ตรวจสอบหรือสร้าง TeachableSubject
    //         var teachableSubject = SdmTeachtableSubject.CheckOrCreate(
    //             teachtableId: teachtable.id,
    //             subjectId: subjectId
    //         );
    //
    //         if (teachableSubject == null || teachableSubject.id == 0)
    //         {
    //             Console.WriteLine($"TeachtableSubject Check Failed: {teachableSubject.id}");
    //             throw new Exception("TeachtableSubject is null or has invalid id.123");
    //         }
    //
    //         // 3. สร้าง TeachtableSubjectReview
    //         var newReview = new TeachtableSubjectReview(
    //             teachtable_subject: teachableSubject,
    //             user_id: SdmUser.GetBy(studentId),
    //             review: review,
    //             rating: rating,
    //             like: 0 // Default value for "like"
    //         );
    //
    //         Console.WriteLine($"Inserting Review: teachtable_subject_id={teachableSubject.id}, user_id={studentId}, review={review}, rating={rating}");
    //         Insert(newReview);
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error in CreateReview: {ex.Message}");
    //         throw;
    //     }
    // }
//     public static void CreateReview(string studentId, int year, int term, string subjectId, string review, float rating)
// {
//     Console.WriteLine($"student_id = {studentId}");
//     Console.WriteLine($"subject_id = {subjectId}");
//     try
//     {
//         // 1. ตรวจสอบหรือสร้าง Teachtable
//         var teachtable = SdmTeachtable.CheckOrCreate(year, term);
//
//         // Log ค่าที่ใช้เรียก CheckOrCreate
//         Console.WriteLine($"Calling CheckOrCreate with teachtable_id={teachtable.id}, subject_id={subjectId}");
//
//         // 2. ตรวจสอบหรือสร้าง TeachableSubject
//         var teachableSubject = SdmTeachtableSubject.CheckOrCreate(
//             teachtableId: teachtable.id,
//             subjectId: subjectId
//         );
//
//         if (teachableSubject == null || teachableSubject.id == 0)
//         {
//             Console.WriteLine($"TeachtableSubject Check Failed: teachtable_subject_id={teachableSubject?.id}");
//             throw new Exception("TeachtableSubject is null or has invalid id.");
//         }
//
//         // Log TeachtableSubject Object
//         Console.WriteLine($"TeachtableSubject Object: id={teachableSubject.id}, teachtable_id={teachableSubject.teachtable?.id}, subject_id={teachableSubject.subject_id}");
//
//         // Log User Object
//         var user = SdmUser.GetBy(studentId);
//         if (user == null || string.IsNullOrEmpty(user.id))
//         {
//             Console.WriteLine($"User Check Failed: student_id={studentId}");
//             throw new Exception("User is null or has invalid id.");
//         }
//         Console.WriteLine($"User Object: id={user.id}");
//
//         // 3. สร้าง TeachtableSubjectReview
//         var newReview = new TeachtableSubjectReview(
//             teachtable_subject: teachableSubject,
//             user_id: user,
//             review: review,
//             rating: rating,
//             like: 0 // Default value for "like"
//         );
//
//         Console.WriteLine($"Inserting Review: teachtable_subject_id={teachableSubject.id}, user_id={studentId}, review={review}, rating={rating}");
//         Insert(newReview);
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Error in CreateReview: {ex.Message}");
//         throw;
//     }
// }

//     public static void CreateReview(string studentId, int year, int term, string subjectId, string review, float rating)
// {
//     Console.WriteLine($"Received Data: studentId={studentId}, year={year}, term={term}, subjectId={subjectId}, review={review}, rating={rating}");
//
//     try
//     {
//         // 1. ตรวจสอบหรือสร้าง Teachtable
//         var teachtable = SdmTeachtable.CheckOrCreate(year, term);
//
//         Console.WriteLine($"Teachtable: id={teachtable?.id}, academic_year={teachtable?.academic_year}, academic_term={teachtable?.academic_term}");
//
//         // 2. ตรวจสอบหรือสร้าง TeachtableSubject
//         var teachableSubject = SdmTeachtableSubject.CheckOrCreate(
//             teachtableId: teachtable.id,
//             subjectId: subjectId
//         );
//
//         if (teachableSubject == null || teachableSubject.id == 0)
//         {
//             Console.WriteLine($"TeachtableSubject Check Failed: id={teachableSubject?.id}");
//             throw new Exception("TeachtableSubject is null or has invalid id.");
//         }
//
//         Console.WriteLine($"TeachtableSubject: id={teachableSubject.id}, subject_id={teachableSubject.subject_id}");
//
//         // 3. ดึง User
//         var user = SdmUser.GetBy(studentId);
//
//         if (user == null || string.IsNullOrEmpty(user.id))
//         {
//             Console.WriteLine($"User Check Failed: student_id={studentId}");
//             throw new Exception("User is null or has invalid id.");
//         }
//
//         Console.WriteLine($"User: id={user.id}");
//
//         // 4. สร้าง TeachtableSubjectReview
//         var newReview = new TeachtableSubjectReview(
//             teachtable_subject: teachableSubject,
//             user_id: user,
//             review: review,
//             rating: rating,
//             like: 0
//         );
//
//         Console.WriteLine($"Inserting Review: teachtable_subject_id={teachableSubject.id}, user_id={user.id}, review={review}, rating={rating}");
//
//         Insert(newReview);
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Error in CreateReview: {ex.Message}");
//         throw;
//     }
// }

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