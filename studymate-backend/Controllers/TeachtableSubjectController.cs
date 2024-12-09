// using Microsoft.AspNetCore.Mvc;
// using studymate_backend.Libraries.Methods;
// using studymate_backend.Libraries.Models;
// using System.Text.Json;
//
// namespace studymate_backend.Controllers;
//
// [ApiController]
// [Route("api/teachtable-subject")]
// public class TeachtableSubjectController : ControllerBase
// {
//     [HttpPost("{year}/{semester}/{faculty}/{department}/{curriculum}/{classYear}")]
// public async Task<IActionResult> AddTeachtableSubjects(
//     [FromRoute] int year,
//     [FromRoute] int semester,
//     [FromRoute] string faculty,
//     [FromRoute] string department,
//     [FromRoute] string curriculum,
//     [FromRoute] int classYear)
// {
//     try
//     {
//         // ค้นหา Teachtable ที่ตรงกับ academic_year และ academic_term
//         var teachtable = SdmTeachtable.GetBy(year, semester);
//         if (teachtable == null)
//             return NotFound(new { message = "No matching Teachtable found." });
//
//         // ดึงข้อมูลจาก Public API
//         var dataFromApi = await SdmCurriculumTeachtable.FetchFilteredTeachTableData(
//             year, semester, faculty, department, curriculum, classYear);
//
//         // // ตรวจสอบประเภทของ "data"
//         // var dataProperty = dataFromApi.GetProperty("data");
//         //
//         // if (dataProperty.ValueKind == JsonValueKind.Array)
//         // {
//         //     // จัดการกรณีที่ "data" เป็น Array
//         //     foreach (var teachTable in dataProperty.EnumerateArray())
//         //     {
//         //         foreach (var subject in teachTable.GetProperty("teachtable").EnumerateArray())
//         //         {
//         //             foreach (var entry in subject.GetProperty("data").EnumerateArray())
//         //             {
//         //                 var teachtableSubject = new TeachtableSubject(
//         //                     0,
//         //                     teachtable,
//         //                     entry.GetProperty("teach_table_id").GetString(), // เก็บ teach_table_id ใน public_id
//         //                     entry.GetProperty("subject_id").GetString(),
//         //                     0, // Interested ค่าเริ่มต้น
//         //                     0.0f // Rating ค่าเริ่มต้น
//         //                 );
//         //
//         //                 // เพิ่มข้อมูล TeachtableSubject
//         //                 SdmTeachtableSubject.Insert(teachtableSubject);
//         //             }
//         //         }
//         //     }
//         // }
//         // else
//         // {
//         //     // จัดการกรณีที่ "data" ไม่ใช่ Array
//         //     return BadRequest(new { message = "The 'data' property is not an array." });
//         // }
//         
//         if (dataFromApi.ValueKind == JsonValueKind.Array)
//         {
//             foreach (var teachTable in dataFromApi.EnumerateArray())
//             {
//                 foreach (var subject in teachTable.GetProperty("teachtable").EnumerateArray())
//                 {
//                     foreach (var entry in subject.GetProperty("data").EnumerateArray())
//                     {
//                         var teachtableSubject = new TeachtableSubject(
//                             0,
//                             teachtable,
//                             entry.GetProperty("teach_table_id").GetString(),
//                             entry.GetProperty("subject_id").GetString(),
//                             0, // Interested ค่าเริ่มต้น
//                             0.0f // Rating ค่าเริ่มต้น
//                         );
//
//                         SdmTeachtableSubject.Insert(teachtableSubject);
//                     }
//                 }
//             }
//         }
//         else if (dataFromApi.ValueKind == JsonValueKind.Object)
//         {
//             var dataProperty = dataFromApi.GetProperty("data");
//             if (dataProperty.ValueKind == JsonValueKind.Array)
//             {
//                 foreach (var teachTable in dataProperty.EnumerateArray())
//                 {
//                     foreach (var subject in teachTable.GetProperty("teachtable").EnumerateArray())
//                     {
//                         foreach (var entry in subject.GetProperty("data").EnumerateArray())
//                         {
//                             var teachtableSubject = new TeachtableSubject(
//                                 0,
//                                 teachtable,
//                                 entry.GetProperty("teach_table_id").GetString(),
//                                 entry.GetProperty("subject_id").GetString(),
//                                 0,
//                                 0.0f
//                             );
//
//                             SdmTeachtableSubject.Insert(teachtableSubject);
//                         }
//                     }
//                 }
//             }
//             else
//             {
//                 return BadRequest(new { message = "The 'data' property is not an array." });
//             }
//         }
//         else
//         {
//             return BadRequest(new { message = "Unexpected JSON structure." });
//         }
//
//
//         return Ok(new { message = "TeachtableSubjects added successfully." });
//     }
//     catch (Exception ex)
//     {
//         return StatusCode(500, new { message = ex.Message });
//     }
// }
// }
