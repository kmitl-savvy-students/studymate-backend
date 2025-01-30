using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;
using Newtonsoft.Json.Linq;

namespace studymate_backend.Libraries.Methods;
public class SdmTeachtableSubjectReview
{
    private static int? LatestYear { get; set; }
    private static int? LatestTerm { get; set; }

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
            // ตรวจสอบและแปลงค่าของ created
            DateOnly createdValue;

            try
            {
                var createdDateTime = DateTime.Parse(query.ToString(6)); // แปลงจาก string เป็น DateTime
                createdValue = DateOnly.FromDateTime(createdDateTime);  // แปลงจาก DateTime เป็น DateOnly
            }
            catch
            {
                createdValue = DateOnly.FromDateTime(DateTime.Now);
            }

            // ดึง subject_name_en จากตาราง subject
            string subjectNameEn = "";
            if (query.ToInt(1) > 0)
            {
                var teachtableSubject = SdmTeachtableSubject.GetById(query.ToInt(1));
                if (teachtableSubject != null)
                {
                    var subject = SdmSubject.GetBy(teachtableSubject.subject_id); // ดึง Subject โดยใช้ subject_id
                    if (subject != null)
                    {
                        subjectNameEn = subject.subject_ename; // ดึง subject_name_en
                    }
                }
            }

            result.Add(new TeachtableSubjectReview(
                SdmTeachtableSubject.GetById(query.ToInt(1)), // Foreign Key: teachtable_subject_id
                query.ToString(2),             
                query.ToString(3),                          
                query.ToFloat(4),                          
                query.ToInt(5),                           
                createdValue,
                query.ToInt(0)                            
            )
            {
                subject_name_en = subjectNameEn // Assign dynamically
            });

            // if (!isArray) break;
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
            
            if (review.teachtable_subject == null || review.teachtable_subject.id == 0)
            {
                throw new Exception("teachtable_subject is null or has invalid id.");
            }

            if (string.IsNullOrEmpty(review.user_id))
            {
                throw new Exception("User is null or has invalid id.");
            }

            var insert = new SdmPgsqlQueryInsert(TableName);

            insert.Insert("teachtable_subject_id", review.teachtable_subject.id.ToString());
            insert.Insert("user_id", review.user_id);
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
    
    public static List<TeachtableSubjectReview> GetBySubject(string subjectId)
    {
        try
        {
            // 1. ดึง teachtable_subject_id ทั้งหมดที่ตรงกับ subjectId
            var selectSubject = new SdmPgsqlQuerySelect("teachtable_subject");
            selectSubject.AddWhereCondition("subject_id", subjectId);
        
            var subjectResult = SdmTeachtableSubject.ProcessQuery(selectSubject, true); // ตั้งค่า isArray เป็น true
            if (subjectResult.Count == 0)
            {
                return new List<TeachtableSubjectReview>(); // คืนค่าเป็นลิสต์ว่างถ้าไม่พบข้อมูล
            }

            var allReviews = new List<TeachtableSubjectReview>();

            // 2. ดึงข้อมูลรีวิวจาก teachtable_subject_review ทั้งหมด
            foreach (var subject in subjectResult)
            {
                var selectReview = GetQueryObj();
                selectReview.AddWhereCondition("teachtable_subject_id", subject.id.ToString());

                var reviewResult = ProcessQuery(selectReview, true);
                if (reviewResult.Count > 0)
                {
                    allReviews.AddRange(reviewResult); // รวมข้อมูลทั้งหมดในลิสต์เดียว
                }
            }

            return allReviews; // คืนค่าลิสต์ของรีวิวทั้งหมด
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetBySubject: {ex.Message}");
            throw;
        }
    }
    
    public static TeachtableSubjectReview? GetBySubjectAndStudent(string subjectId, string studentId)
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
                var selectReview = GetQueryObj();
                selectReview.AddWhereCondition("teachtable_subject_id", teachtableSubject.id.ToString());
                selectReview.AddWhereCondition("user_id", studentId);

                var reviewResult = ProcessQuery(selectReview, true);
                if (reviewResult.Count > 0)
                {
                    return reviewResult[0]; // คืนค่าหากมีรีวิวที่ตรงกัน
                }
            }

            return null; // ไม่มีรีวิวที่ตรงกัน
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetBySubjectAndStudent: {ex.Message}");
            throw;
        }
    }
    
    public static void Delete(string subjectId, string studentId)
    {
        try
        {
            // ดึง teachtable_subject_id ทั้งหมดที่ตรงกับ subjectId
            var selectSubject = new SdmPgsqlQuerySelect("teachtable_subject");
            selectSubject.AddWhereCondition("subject_id", subjectId);

            var subjectResult = SdmTeachtableSubject.ProcessQuery(selectSubject, true); // ใช้ isArray = true เพื่อดึงข้อมูลทั้งหมด
            if (subjectResult.Count == 0)
            {
                throw new Exception("TeachtableSubject not found.");
            }

            // ลบข้อมูล teachtable_subject_review ทั้งหมดที่เกี่ยวข้องกับ user_id
            foreach (var subject in subjectResult)
            {
                var delete = new SdmPgsqlQueryDelete(TableName);
                delete.WhereEqual("teachtable_subject_id", subject.id.ToString());
                delete.WhereEqual("user_id", studentId);

                var query = SdmPgsqlQuery.Execute(delete);
                query.CleanUp();
                Console.WriteLine($"Deleted review for teachtable_subject_id={subject.id}, user_id={studentId}");
            }
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
    
            var newReview = new TeachtableSubjectReview(
                teachtable_subject: teachableSubject,
                user_id: studentId,
                review: review,
                rating: rating,
                like: 0
            );
            Console.WriteLine($"TeachtableSubjectReview Created: teachtable_subject_id={newReview.teachtable_subject?.id}, user_id={newReview.user_id}, review={newReview.review}, rating={newReview.rating}, like={newReview.like}");
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
    
    public static List<string> ExtractSubjectIdsFromApiResponse(string apiResponse)
    {
        var allSubjects = new List<string>();

        try
        {
            // แปลง JSON string เป็น JArray
            var jsonArray = JArray.Parse(apiResponse);

            foreach (var faculty in jsonArray)
            {
                var teachTableArray = faculty["teachtable"];
                if (teachTableArray == null) continue;

                foreach (var teachTable in teachTableArray)
                {
                    var dataArray = teachTable["data"];
                    if (dataArray == null) continue;

                    foreach (var data in dataArray)
                    {
                        var subjectId = data["subject_id"]?.ToString();
                        if (!string.IsNullOrEmpty(subjectId) && !allSubjects.Contains(subjectId))
                        {
                            allSubjects.Add(subjectId); // เพิ่ม subject_id ถ้ายังไม่มีใน List
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] Failed to extract subject IDs: {ex.Message}");
        }

        return allSubjects;
    }
    
    public static async Task<List<string>> GetAllSubjectInFacultyAndGened(string curriculum)
    {
        int currentYear = DateTime.Now.Year + 543; // คำนวณปี พ.ศ.
        int[] terms = { 3, 2, 1 };
    
        using var client = new HttpClient();
    
        while (currentYear >= 2560) // กำหนดปีต่ำสุด
        {
            foreach (var currentTerm in terms)
            {
                var responseFaculty = await client.GetAsync($"https://regis.reg.kmitl.ac.th/api/?function=get-teach-table-show&mode=by_class" +
                                                     $"&selected_year={currentYear}" +
                                                     $"&selected_semester={currentTerm}" +
                                                     $"&selected_faculty=01" +
                                                     $"&selected_department=05" +
                                                     $"&selected_curriculum={curriculum}" +
                                                     $"&selected_class_year=0" +
                                                     $"&search_all_faculty=false" +
                                                     $"&search_all_department=false" +
                                                     $"&search_all_curriculum=false" +
                                                     $"&search_all_class_year=true");
    
                if (responseFaculty.IsSuccessStatusCode)
                {
                    var dataOfFaculty = await responseFaculty.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(dataOfFaculty) && dataOfFaculty != "[]")
                    {
                        // ดึง subject_id จาก response
                        var allSubjectsOfFaculty = ExtractSubjectIdsFromApiResponse(dataOfFaculty);
                        
                        // บันทึกค่าในตัวแปร
                        LatestYear = currentYear;
                        LatestTerm = currentTerm;
                        
                        var responseGened = await client.GetAsync($"https://regis.reg.kmitl.ac.th/api/?function=get-teach-table-show&mode=by_class" +
                                                                  $"&selected_year={currentYear}" +
                                                                  $"&selected_semester={currentTerm}" +
                                                                  $"&selected_faculty=90" +
                                                                  $"&selected_department=90" +
                                                                  $"&selected_curriculum=x" +
                                                                  $"&selected_class_year=0" +
                                                                  $"&search_all_faculty=false" +
                                                                  $"&search_all_department=false" +
                                                                  $"&search_all_curriculum=true" +
                                                                  $"&search_all_class_year=true");
                        
                        var allSubjectsOfGened = new List<string>();
                        if (responseGened.IsSuccessStatusCode)
                        {
                            var dataOfGened = await responseGened.Content.ReadAsStringAsync();
                            if (!string.IsNullOrWhiteSpace(dataOfGened) && dataOfGened != "[]")
                            {
                                // ดึง subject_id จาก response
                                allSubjectsOfGened = ExtractSubjectIdsFromApiResponse(dataOfGened);
                            }
                        }

                        var allSubjects = allSubjectsOfFaculty.Concat(allSubjectsOfGened).Distinct().ToList();
                        // แสดงผลใน Console
                        Console.WriteLine($"[GetAllSubjectInFacultyAndGenedTest] CurrentYear: {currentYear}");
                        Console.WriteLine($"[GetAllSubjectInFacultyAndGenedTest] CurrentTerm: {currentTerm}");
                        Console.WriteLine("[GetAllSubjectInFacultyAndGenedTest] AllSubjectsOfFaculty:");
                        foreach (var subjectId in allSubjectsOfFaculty)
                        {
                            Console.WriteLine($"- {subjectId}");
                        }
                        Console.WriteLine("[GetAllSubjectInFacultyAndGenedTest] AllSubjectsOfGened:");
                        foreach (var subjectId in allSubjectsOfGened)
                        {
                            Console.WriteLine($"- {subjectId}");
                        }
                        Console.WriteLine("[GetAllSubjectInFacultyAndGenedTest] AllSubjects:");
                        foreach (var subjectId in allSubjects)
                        {
                            Console.WriteLine($"- {subjectId}");
                        }
    
                        return (allSubjects);
                    }
                }
            }
            currentYear--;
        }
    
        throw new Exception("Cannot find valid academic year and term.");
    }
    
    public static List<TeachtableSubjectReview> GetReviewsBySubjects(List<string> subjectIds)
    {
        var allReviews = new List<TeachtableSubjectReview>();

        foreach (var subjectId in subjectIds)
        {
            var reviews = GetBySubject(subjectId); // ใช้ฟังก์ชันที่มีอยู่แล้วเพื่อดึงรีวิวของแต่ละ subjectId
            if (reviews.Count > 0)
            {
                allReviews.AddRange(reviews); // รวมรีวิวทั้งหมดในลิสต์เดียว
            }
        }

        // จัดเรียงรีวิวจากวันที่ `created` ล่าสุดก่อน แล้วจัดเรียงสำรองตาม `id`
        return allReviews
            .OrderByDescending(review => review.created) // จัดเรียงวันที่จากล่าสุดไปเก่า
            .ThenByDescending(review => review.id)      // จัดเรียง id จากมากไปน้อย (สำรอง)
            .ToList();
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
        if (userToken.expired.ToDateTime() < DateTime.Now)
        {
            Console.WriteLine("[Error] Token has expired.");
            return null;
        }

        // คืนค่า User ที่เชื่อมโยงกับ Token
        return userToken.user;
    }
    
}