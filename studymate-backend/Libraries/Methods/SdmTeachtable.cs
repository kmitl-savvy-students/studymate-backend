using studymate_backend.Libraries.Database;
using studymate_backend.Libraries.Database.QueryBuilders;
using studymate_backend.Libraries.Models;
using studymate_backend.Libraries.Helper;

namespace studymate_backend.Libraries.Methods;

public abstract class SdmTeachtable : ISdmBaseMethod<Teachtable>
{
    public static string TableName => "teachtable";
    public static SdmMysqlQuerySelect GetQueryObj()
    {
        return new SdmMysqlQuerySelect(TableName);
    }
    public static List<Teachtable> ProcessQuery(ISdmMysqlQueryBase queryBuilder, bool isArray = false)
    {
        var query = SdmMysqlQuery.Execute(queryBuilder);

        var result = new List<Teachtable>();
        while (query.Next())
        {
            result.Add(new Teachtable(
                query.ToInt(0),
                query.ToInt(1),
                query.ToInt(2)
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
        return result;
    }
    public static Teachtable? GetBy(int id)
    {
        var select = GetQueryObj();
        select.WhereEqual("tt_id", id.ToString());

        var result = ProcessQuery(select);
        return result.Count == 0 ? null : result[0];
    }
    public static Teachtable? GetBy(int year, int term)
    {
        var select = GetQueryObj();
        select.WhereEqual("tt_year", year.ToString());
        select.WhereEqual("tt_term", term.ToString());

        var result = ProcessQuery(select);
        var teachtable = result.Count == 0 ? null : result[0];

        if (teachtable != null) return teachtable;

        Insert(new Teachtable(-1, year, term));
        teachtable = GetBy(year, term);

        return teachtable;
    }

    public static void Insert(Teachtable teachtable)
    {
        var insert = new SdmMysqlQueryInsert(TableName);

        insert.Insert("tt_year", teachtable.Year.ToString());
        insert.Insert("tt_term", teachtable.Term.ToString());

        var query = SdmMysqlQuery.Execute(insert);
        query.CleanUp();
    }
    
    public static Teachtable CheckOrCreate(int year, int term)
    {
        if (!SdmNumber.IsAcademicTerm(term) ||
            !SdmNumber.IsAcademicYear(year))
            throw new ArgumentException("Invalid academic year or term.");
        
        // สร้าง Query Object พร้อมเงื่อนไข
        var select = new SdmMysqlQuerySelect("teachtable")
            .AddWhereCondition("tt_year", year.ToString())
            .AddWhereCondition("tt_term", term.ToString());

        // ตรวจสอบว่ามี Teachtable อยู่แล้วหรือไม่
        var result = ProcessQuery(select);
        if (result.Count > 0)
        {
            return result[0]; // Return Teachtable ที่มีอยู่
        }

        // ถ้าไม่มี Teachtable ให้สร้างใหม่
        var newTeachtable = new Teachtable(-1, year, term);
        Insert(newTeachtable);

        // Query ใหม่เพื่อดึงข้อมูลที่สร้าง
        var selectAfterInsert = new SdmMysqlQuerySelect("teachtable")
            .AddWhereCondition("tt_year", year.ToString())
            .AddWhereCondition("tt_term", term.ToString());

        var newResult = ProcessQuery(selectAfterInsert);
        if (newResult.Count > 0)
        {
            return newResult[0];
        }

        throw new Exception("Failed to create or retrieve Teachtable.");
    }
    
}