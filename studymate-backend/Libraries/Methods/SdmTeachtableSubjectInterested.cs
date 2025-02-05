using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;
public class SdmTeachtableSubjectInterested
{
    public static string TableName => "teachtable_subject_interested";
    
    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }
    
    public static List<TeachtableSubjectInterested> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);

        var result = new List<TeachtableSubjectInterested>();

        while (query.Next())
        {
            result.Add(new TeachtableSubjectInterested(
         
                SdmTeachtableSubject.GetById(query.ToInt(1)),
                query.ToString(0),
                query.ToInt(2)
            ));
       
        }

        query.CleanUp();
        return result;
    }
    
    public static TeachtableSubjectInterested? GetBySubjectAndStudent(string subjectId, string studentId)
    {
        try
        {
            // ดึง teachtable_subject_id ทั้งหมดที่เกี่ยวข้องกับ subjectId
            var selectSubject = new SdmPgsqlQuerySelect("teachtable_subject");
            selectSubject.AddWhereCondition("subject_id", subjectId);

            var subjectResult = SdmTeachtableSubject.ProcessQuery(selectSubject, true);
            if (subjectResult.Count == 0)
            {
                return null; // ไม่มี teachtable_subject_id ที่เกี่ยวข้อง
            }

            // ตรวจสอบ teachtable_subject_review สำหรับ user_id และ teachtable_subject_id
            foreach (var teachtableSubject in subjectResult)
            {
                var selectInterested = GetQueryObj();
                selectInterested.AddWhereCondition("teachtable_subject_id", teachtableSubject.id.ToString());
                selectInterested.AddWhereCondition("user_id", studentId);

                var interestedResult = ProcessQuery(selectInterested, true);
                if (interestedResult.Count > 0)
                {
                    return interestedResult[0]; // คืนค่าหากมี interested ที่ตรงกัน
                }
            }

            return null; // ไม่มี interested ่ตรงกัน
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetBySubjectAndStudent: {ex.Message}");
            throw;
        }
    }

    public static void Insert(TeachtableSubjectInterested interested)
    {
        try
        {
            
            if (interested.teachtable_subject == null || interested.teachtable_subject.id == 0)
            {
                throw new Exception("teachtable_subject is null or has invalid id.");
            }

            if (string.IsNullOrEmpty(interested.user_id))
            {
                throw new Exception("User is null or has invalid id.");
            }

            var insert = new SdmPgsqlQueryInsert(TableName);
            
            insert.Insert("teachtable_subject_id", interested.teachtable_subject.id.ToString());
            insert.Insert("user_id", interested.user_id);

            var query = SdmPgsqlQuery.Execute(insert);
            query.CleanUp();

            Console.WriteLine("TeachtableSubjectInterested Inserted Successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Insert: {ex.Message}");
            throw;
        }
    }
    
    public static void CreateInterested(string studentId, int year, int term, string subjectId)
    {
        Console.WriteLine($"Received Data: studentId={studentId}, year={year}, term={term}, subjectId={subjectId}");
    
        try
        {
            // ตรวจสอบว่าผู้ใช้ได้รีวิววิชานี้ไปแล้วหรือไม่
            var existingReview = GetBySubjectAndStudent(subjectId, studentId);
            if (existingReview != null)
            {
                // throw new Exception("You have already reviewed this subject.");
                throw new InvalidOperationException("User has already reviewed this subject.");
            }
            
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
    
            var newReview = new TeachtableSubjectInterested(
                teachtable_subject: teachableSubject,
                user_id: user.id
            );
            Console.WriteLine($"TeachtableSubject interested Created test: teachtable_subject_id={newReview.teachtable_subject?.id}, user_id={newReview.user_id}");
            Insert(newReview);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Conflict: {ex.Message}");
            throw; // ข้อผิดพลาดนี้จะถูกจัดการใน Controller
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateReview: {ex.Message}");
            throw;
        }
    }
}