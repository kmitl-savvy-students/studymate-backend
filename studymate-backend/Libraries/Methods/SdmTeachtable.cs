using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;
using studymate_backend.Libraries.Helper;

namespace studymate_backend.Libraries.Methods;

public class SdmTeachtable : ISdmBaseMethod<Teachtable>
{
    public static string TableName => "teachtable";

    public static SdmPgsqlQuerySelect GetQueryObj()
    {
        return new SdmPgsqlQuerySelect(TableName);
    }

    public static List<Teachtable> ProcessQuery(ISdmPgsqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmPgsqlQuery.Execute(queryBuilder);

        var result = new List<Teachtable>();

        while (query.Next())
        {
            result.Add(new Teachtable(
                query.ToInt(1),
                query.ToInt(2),
                query.ToInt(0)
            ));
            if (!isArray) break;
        }

        query.CleanUp();
        return result;
    }
    
    public static List<Teachtable> GetAll()
    {
        var select = GetQueryObj();

        var result = ProcessQuery(select, true);

        // ใช้ LINQ เพื่อเรียงลำดับตาม id
        return result.OrderBy(t => t.id).ToList();
    }


    public static Teachtable? GetById(int id)
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

    public static void Insert(Teachtable teachtable)
    {
        var insert = new SdmPgsqlQueryInsert(TableName);
        
        insert.Insert("academic_year", teachtable.academic_year.ToString());
        insert.Insert("academic_term", teachtable.academic_term.ToString());
        
        var query = SdmPgsqlQuery.Execute(insert);
        query.CleanUp();
    }

    public static void Update(Teachtable teachtable)
    {
        var update = new SdmPgsqlQueryUpdate(TableName);
        
        update.Set("academic_year", teachtable.academic_year.ToString());
        update.Set("academic_term", teachtable.academic_term.ToString());
        
        update.WhereEqual("id", teachtable.id.ToString());
        
        var query = SdmPgsqlQuery.Execute(update);
        query.CleanUp();
    }
    
    public static Teachtable CheckOrCreate(int year, int term)
    {
        if (!SdmNumber.IsAcademicTerm(term) ||
            !SdmNumber.IsAcademicYear(year))
            throw new ArgumentException("Invalid academic year or term.");
        // สร้าง Query Object พร้อมเงื่อนไข
        var select = new SdmPgsqlQuerySelect("teachtable")
            .AddWhereCondition("academic_year", year.ToString())
            .AddWhereCondition("academic_term", term.ToString());

        // ตรวจสอบผลลัพธ์
        var result = ProcessQuery(select);
        if (result.Count > 0)
        {
            return result[0]; // Return Teachtable ที่มีอยู่
        }

        // ถ้าไม่มี Teachtable ให้สร้างใหม่
        var newTeachtable = new Teachtable(year, term);
        Insert(newTeachtable);

        // Query ใหม่เพื่อดึงข้อมูลที่สร้าง
        var selectAfterInsert = new SdmPgsqlQuerySelect("teachtable")
            .AddWhereCondition("academic_year", year.ToString())
            .AddWhereCondition("academic_term", term.ToString());

        var newResult = ProcessQuery(selectAfterInsert);
        if (newResult.Count > 0)
        {
            return newResult[0];
        }

        throw new Exception("Failed to create or retrieve Teachtable.");
    }

}