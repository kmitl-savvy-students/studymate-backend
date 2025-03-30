using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;
using Newtonsoft.Json.Linq;

namespace studymate_backend.Libraries.Methods;
public class SdmSubjectReview
{
    private static int? LatestYear { get; set; }
    private static int? LatestTerm { get; set; }

    public static string TableName => "subject_review";

    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }
    
    public static List<SubjectReview> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);
    
        var result = new List<SubjectReview>();
    
        while (query.Next())
        {
            // ตรวจสอบและแปลงค่าของ created
            DateOnly createdValue;
    
            try
            {
                var createdDateTime = DateTime.Parse(query.ToString(7)); // แปลงจาก string เป็น DateTime
                createdValue = DateOnly.FromDateTime(createdDateTime);  // แปลงจาก DateTime เป็น DateOnly
            }
            catch
            {
                createdValue = DateOnly.FromDateTime(DateTime.Now);
            }
    
            // ดึง subject_name_en จากตาราง subject
            string subjectNameEn = "";
            string subjectId = query.ToString(6);
            
            if (!string.IsNullOrEmpty(subjectId))
            {
                // ดึงข้อมูลวิชาจาก SdmSubject
                var subject = SdmSubject.GetBy(subjectId);
                if (subject != null)
                {
                    subjectNameEn = subject.NameEn;
                }
                else
                {
                    Console.WriteLine($"[DEBUG] ไม่พบข้อมูลวิชาสำหรับ subjectId={subjectId}");
                }
            }
            else
            {
                Console.WriteLine("[DEBUG] subjectId เป็นค่าว่างหรือ null");
            }
            
            result.Add(new SubjectReview(
                SdmTeachtable.GetBy(query.ToInt(1)), // Foreign Key: teachtable
                query.ToInt(2),             
                query.ToString(3),                          
                query.ToFloat(4),                          
                query.ToInt(5),
                query.ToString(6),
                createdValue,
                query.ToInt(0)                            
            )
            {
                SubjectNameEn = subjectNameEn // Assign dynamically
            });
            
        }
    
        query.CleanUp();
        return result;
    }
    
    public static List<SubjectReview> GetAll()
    {
        var select = GetQueryObj();
        
        var result = ProcessQuery(select, true);
        return result;
    }

    public static SubjectReview GetById(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("sbjr_id", id.ToString()); //✅
        
        var  result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }

    public static List<SubjectReview> GetBySubjectId(string subjectId)
    {
        var select = GetQueryObj();
        select.WhereEqual("sbjr_sbj_id", subjectId);
        
        var result = ProcessQuery(select, true);
        return result;
    }
    
    public static void Insert(SubjectReview review)
    {
        try
        {
            
            if (review.Teachtable == null || review.Teachtable.Id == 0)
            {
                throw new Exception("teachtable is null or has invalid id.");
            }

            if (string.IsNullOrEmpty(review.UserId.ToString()))
            {
                throw new Exception("User is null or has invalid id.");
            }

            var insert = new SdmMysqlQueryInsert(TableName);
            
            insert.Insert("sbjr_tt_id", review.Teachtable.Id.ToString());  //✅
            insert.Insert("sbjr_u_id", review.UserId.ToString());  //✅
            insert.Insert("sbjr_rev", review.Review);  //✅
            insert.Insert("sbjr_rat", review.Rating.ToString());  //✅
            insert.Insert("sbjr_like", review.Like.ToString());  //✅
            insert.Insert("sbjr_date_created", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            insert.Insert("sbjr_sbj_id", review.SubjectId);

            var query = SdmMysqlQuery.Execute(insert);
            query.CleanUp();
            
            Console.WriteLine("Subject Review Inserted Successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Insert: {ex.Message}");
            throw;
        }
    }
    
    public static SubjectReview? GetBySubjectAndStudent(string subjectId, string studentId)
    {
        try
        {
            var select = GetQueryObj();
            select.WhereEqual("sbjr_sbj_id", subjectId);
            select.WhereEqual("sbjr_u_id", studentId);
            
            var result = ProcessQuery(select);
            if (result.Count > 0)
            {
                return result[0];
            }
            return null;
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
            // ดึง reviewId ก่อนลบ
            var selectReview = GetQueryObj();
            selectReview.WhereEqual("sbjr_sbj_id", subjectId);
            selectReview.WhereEqual("sbjr_u_id", studentId);
        
            var reviews = ProcessQuery(selectReview, true);

            // ลบ Like ที่เกี่ยวข้อง
            foreach (var review in reviews)
            {
                var deleteLikeReview = new SdmMysqlQueryDelete("subject_review_like");
                deleteLikeReview.WhereEqual("srl_sbjr_id", review.Id.ToString());
                var queryLike = SdmMysqlQuery.Execute(deleteLikeReview);
                queryLike.CleanUp();
            }

            // ลบ Review
            var deleteReview = new SdmMysqlQueryDelete("subject_review");
            deleteReview.WhereEqual("sbjr_sbj_id", subjectId);
            deleteReview.WhereEqual("sbjr_u_id", studentId);
            var queryReview = SdmMysqlQuery.Execute(deleteReview);
            queryReview.CleanUp();

            Console.WriteLine($"Deleted review and likes for subject_id={subjectId}, user_id={studentId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Delete: {ex.Message}");
            throw;
        }
    }
    
    public static void CreateReview(int studentId, int year, int term, string subjectId, string review, float rating)
    {
        Console.WriteLine($"Received Data: studentId={studentId}, year={year}, term={term}, subjectId={subjectId}, review={review}, rating={rating}");
    
        try
        {
            // ตรวจสอบว่าผู้ใช้ได้รีวิววิชานี้ไปแล้วหรือไม่
            var existingReview = GetBySubjectAndStudent(subjectId, studentId.ToString());
            if (existingReview != null)
            {
                // throw new Exception("You have already reviewed this subject.");
                throw new InvalidOperationException("User has already reviewed this subject.");
            }
            
            var teachtable = SdmTeachtable.CheckOrCreate(year, term);
            Console.WriteLine($"Teachtable: id={teachtable?.Id}, academic_year={teachtable?.Year}, academic_term={teachtable?.Term}");
    
            var user = SdmUser.GetBy(studentId);
            if (user == null || string.IsNullOrEmpty(user.Id.ToString()))
            {
                throw new Exception("User is null or has invalid id.");
            }
            Console.WriteLine($"User: id={user.Id}");
    
            var newReview = new SubjectReview(
                teachtable: teachtable,
                subjectId: subjectId,
                userId: studentId,
                review: review,
                rating: rating,
                like: 0
            );
            
            Console.WriteLine($"SubjectReview Created: teachtable_id={newReview.Teachtable?.Id}, user_id={newReview.UserId}, review={newReview.Review}, rating={newReview.Rating}, like={newReview.Like}");
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
    
    public static async Task<List<string>> GetAllSubjectInFacultyAndGened(User user)
    {
        int currentYear = DateTime.Now.Year + 543; // คำนวณปี พ.ศ.
        int[] terms = { 3, 2, 1 };
    
        using var client = new HttpClient();
        Console.WriteLine($"faculty: {user.Curriculum.Program.Department.Faculty.KmitlId}, department: {user.Curriculum.Program.Department.KmitlId}, curriculum: {user.Curriculum.Program.KmitlId}");
    
        while (currentYear >= 2560) // กำหนดปีต่ำสุด
        {
            foreach (var currentTerm in terms)
            {
                var responseFaculty = await client.GetAsync($"https://regis.reg.kmitl.ac.th/api/?function=get-teach-table-show&mode=by_class" +
                                                     $"&selected_year={currentYear}" +
                                                     $"&selected_semester={currentTerm}" +
                                                     $"&selected_faculty={user.Curriculum.Program.Department.Faculty.KmitlId}"+
                                                     $"&selected_department={user.Curriculum.Program.Department.KmitlId}" +
                                                     $"&selected_curriculum={user.Curriculum.Program.KmitlId}" +
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
    
    public static List<SubjectReview> GetReviewsBySubjects(List<string> subjectIds)
    {
        var allReviews = new List<SubjectReview>();

        foreach (var subjectId in subjectIds)
        {
            var reviews = GetBySubjectId(subjectId); // ใช้ฟังก์ชันที่มีอยู่แล้วเพื่อดึงรีวิวของแต่ละ subjectId
            if (reviews.Count > 0)
            {
                allReviews.AddRange(reviews); // รวมรีวิวทั้งหมดในลิสต์เดียว
            }
        }

        // จัดเรียงรีวิวจากวันที่ `created` ล่าสุดก่อน แล้วจัดเรียงสำรองตาม `id`
        return allReviews
            .OrderByDescending(review => review.Created) // จัดเรียงวันที่จากล่าสุดไปเก่า
            .ThenByDescending(review => review.Id)      // จัดเรียง id จากมากไปน้อย (สำรอง)
            .ToList();
    }
    
    public static void UpdateLikeCount(int reviewId)
    {
        try
        {
            // ดึงจำนวน Like ของรีวิวนี้
            var select = new SdmMysqlQuerySelect("subject_review_like");
            select.WhereEqual("srl_sbjr_id", reviewId.ToString());
        
            // ✅ ใช้ Count() จาก List<TeachtableSubjectReviewLike>
            var countLike = ProcessQuery(select).Count;

            // อัปเดตจำนวนไลค์ใน teachtable_subject_review
            var update = new SdmMysqlQueryUpdate("subject_review");
            update.Set("sbjr_like", countLike.ToString());
            update.WhereEqual("sbjr_id", reviewId.ToString());

            var query = SdmMysqlQuery.Execute(update);
            query.CleanUp();

            Console.WriteLine($"Updated like count for review {reviewId}: {countLike} likes.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateLikeCount: {ex.Message}");
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
    
     public static int GetCountOfReview(string subjectId)
     {
         try
         {
             var reviews = GetBySubjectId(subjectId);
             if (reviews == null)
             {
                 return 0;
             }
             return reviews.Count;
         }
         catch (Exception ex)
         {
             Console.WriteLine($"Error in GetCountOfReview: {ex.Message}");
             return 0;
         }
     }

     public static double GetAverageRatingOfReview(string subjectId)
     {
         try
         {
             var reviews = GetBySubjectId(subjectId);
             if (reviews == null || reviews.Count == 0)
             {
                 return 0;
             }

             double sumOfRating = 0;
             foreach (var review in reviews)
             {
                 var rating = review.Rating;
                 sumOfRating += rating;
             }
             return sumOfRating/reviews.Count;
         }
         catch (Exception ex)
         {
             Console.WriteLine($"Error in GetAverageRatingOfReview: {ex.Message}");
             return 0;
         }
     }
    
}