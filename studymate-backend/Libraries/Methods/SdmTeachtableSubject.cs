using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Libraries.Methods;

public class SdmTeachtableSubject : ISdmBaseMethod<TeachtableSubject>
{
    public static string TableName => "teachtable_subject";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<TeachtableSubject> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);

        var result = new List<TeachtableSubject>();

        while (query.Next())
        {
            result.Add(new TeachtableSubject(
                SdmTeachtable.GetById(query.ToInt(1)),
                query.ToString(2),
                query.ToInt(3),
                query.ToFloat(4),
                query.ToInt(0)
                
            ));
            if (!isArray) break;
        }
        query.CleanUp();
        return result;
    }

    // public static void Insert(TeachtableSubject teachtableSubject)
    // {
    //     var insert = new SdmPgsqlQueryInsert(TableName);
    //     
    //     insert.Insert("teachtable_id", teachtableSubject.teachtable?.id.ToString());
    //     insert.Insert("subject_id", teachtableSubject.subject_id);
    //     insert.Insert("interested", teachtableSubject.interested.ToString());
    //     insert.Insert("rating", teachtableSubject.rating.ToString());
    //
    //     var query = SdmPgsqlQuery.Execute(insert);
    //     query.CleanUp();
    // }
    
    public static void Insert(TeachtableSubject teachtableSubject)
    {
        var insert = new SdmPgsqlQueryInsert("teachtable_subject");

        insert.Insert("teachtable_id", teachtableSubject.teachtable?.id.ToString());
        insert.Insert("subject_id", teachtableSubject.subject_id);
        insert.Insert("interested", teachtableSubject.interested.ToString());
        insert.Insert("rating", teachtableSubject.rating.ToString());

        Console.WriteLine($"Inserting TeachtableSubject: teachtable_id={teachtableSubject.teachtable?.id}, subject_id={teachtableSubject.subject_id}");
        var query = SdmPgsqlQuery.Execute(insert);
        query.CleanUp();
    }

    
    public static TeachtableSubject? GetById(int id)
    {
        // if (id == null)
        //     return null;
        
        var select = GetQueryObj();
        select.WhereEqual("id", id.ToString());
        
        var  result = ProcessQuery(select);
        if (result.Count == 0)
            return null;
        return result[0];
    }

    
    // public static TeachtableSubject CheckOrCreate(int teachtableId, string subjectId)
    // {
    //     try
    //     {
    //         var select = new SdmPgsqlQuerySelect("teachtable_subject")
    //             .AddWhereCondition("teachtable_id", teachtableId.ToString())
    //             .AddWhereCondition("subject_id", subjectId);
    //
    //         var result = ProcessQuery(select);
    //         if (result.Count > 0)
    //         {
    //             return result[0]; // Return TeachtableSubject ที่มีอยู่
    //         }
    //
    //         // ถ้าไม่มี TeachtableSubject ให้สร้างใหม่
    //         var newTeachtableSubject = new TeachtableSubject(
    //             teachtable: new Teachtable(0, 0, teachtableId),
    //             subject_id: subjectId,
    //             interested: 0,
    //             rating: 0.0f
    //         );
    //
    //         Insert(newTeachtableSubject);
    //
    //         // Query ใหม่เพื่อตรวจสอบ
    //         Console.WriteLine("Querying TeachtableSubject after Insert");
    //         var selectAfterInsert = new SdmPgsqlQuerySelect("teachtable_subject")
    //             .AddWhereCondition("teachtable_id", teachtableId.ToString())
    //             .AddWhereCondition("subject_id", subjectId);
    //
    //         var newResult = ProcessQuery(selectAfterInsert);
    //         if (newResult.Count > 0)
    //         {
    //             return newResult[0];
    //         }
    //
    //         throw new Exception("Failed to retrieve TeachtableSubject after Insert.");
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error in CheckOrCreate: {ex.Message}");
    //         throw;
    //     }
    // }
    
    // public static TeachtableSubject CheckOrCreate(int teachtableId, string subjectId)
    // {
    //     try
    //     {
    //         // ตรวจสอบว่ามี TeachtableSubject อยู่หรือไม่
    //         var select = new SdmPgsqlQuerySelect("teachtable_subject")
    //             .AddWhereCondition("teachtable_id", teachtableId.ToString())
    //             .AddWhereCondition("subject_id", subjectId);
    //
    //         var result = ProcessQuery(select);
    //         if (result.Count > 0)
    //         {
    //             return result[0]; // ถ้ามีอยู่แล้ว Return
    //         }
    //
    //         // ถ้าไม่มี ให้สร้าง TeachtableSubject ใหม่
    //         var newTeachtableSubject = new TeachtableSubject(
    //             teachtable: new Teachtable(0, 0, teachtableId),
    //             subject_id: subjectId,
    //             interested: 0,
    //             rating: 0.0f
    //         );
    //
    //         Insert(newTeachtableSubject);
    //
    //         // Query ใหม่หลังจาก Insert เพื่อดึงข้อมูลที่สร้าง
    //         var selectAfterInsert = new SdmPgsqlQuerySelect("teachtable_subject")
    //             .AddWhereCondition("teachtable_id", teachtableId.ToString())
    //             .AddWhereCondition("subject_id", subjectId);
    //
    //         var newResult = ProcessQuery(selectAfterInsert);
    //         if (newResult.Count > 0)
    //         {
    //             return newResult[0];
    //         }
    //
    //         throw new Exception("Failed to retrieve TeachtableSubject after Insert.");
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error in CheckOrCreate: {ex.Message}");
    //         throw;
    //     }
    // }
    
    public static TeachtableSubject CheckOrCreate(int teachtableId, string subjectId)
    {
        try
        {
            Console.WriteLine($"CheckOrCreate: Checking teachtable_subject with teachtable_id={teachtableId}, subject_id={subjectId}");

            // Query TeachtableSubject
            var select = new SdmPgsqlQuerySelect("teachtable_subject")
                .AddWhereCondition("teachtable_id", teachtableId.ToString())
                .AddWhereCondition("subject_id", subjectId);

            var result = ProcessQuery(select);
            if (result.Count > 0)
            {
                Console.WriteLine($"Found TeachtableSubject: id={result[0].id}");
                return result[0];
            }

            // Create New TeachtableSubject
            Console.WriteLine($"Creating new TeachtableSubject for teachtable_id={teachtableId}, subject_id={subjectId}");
            var newTeachtableSubject = new TeachtableSubject(
                teachtable: SdmTeachtable.GetById(teachtableId),
                subject_id: subjectId,
                interested: 0,
                rating: 0.0f
            );
            Insert(newTeachtableSubject);

            // Re-query after Insert
            var newResult = ProcessQuery(select);
            if (newResult.Count > 0)
            {
                Console.WriteLine($"Created TeachtableSubject: id={newResult[0].id}");
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

}