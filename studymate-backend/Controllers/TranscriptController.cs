using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Helper;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Core;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/transcript")]
public partial class TranscriptController : ControllerBase
{
    #region [GET] Get Transcript
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpGet("get-by-user/{userId:int}")]
    public ActionResult GetTranscript(int userId)
    {
        var user = SdmUser.GetBy(userId);
        if (user == null)
            return Unauthorized();

        var transcript = SdmTranscript.GetBy(user);
        if (transcript == null)
            return Ok(null);

        transcript.User = null;
        return Ok(transcript);
    }
    #endregion
    #region [POST] Upload Transcript
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> UploadTranscript([FromForm] DtoUploadTranscript transcriptData)
    {
        var file = transcriptData.File;
        var userId = transcriptData.Id;

        DeleteTranscriptData(userId);

        var user = SdmUser.GetBy(userId);
        if (user?.Curriculum == null)
        {
            Console.WriteLine("Curriculum not found for user.");
            return BadRequest(new { message = "ไม่พบหลักสูตรผู้ใช้งาน" });
        }

        if (file == null || file.Length == 0)
        {
            Console.WriteLine("File is empty.");
            return BadRequest(new { message = "ไม่พบไฟล์ Transcript" });
        }

        const long maxFileSize = 15 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            Console.WriteLine("File is too large.");
            return BadRequest(new { message = "ขนาดไฟล์ใหญ่เกิน 15 MB" });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf")
        {
            Console.WriteLine("File is not a PDF file (invalid extension).");
            return BadRequest(new { message = "ไฟล์ดังกล่าวไม่ใช่ PDF" });
        }

        if (file.ContentType != "application/pdf")
        {
            Console.WriteLine("File is not a PDF file (invalid content type).");
            return BadRequest(new { message = "ไฟล์ดังกล่าวไม่ใช่ PDF" });
        }

        if (!await IsValidPdf(file))
        {
            Console.WriteLine("File is not a valid PDF file (failed PDF signature check).");
            return BadRequest(new { message = "ไฟล์ดังกล่าวไม่ใช่ PDF" });
        }

        string transcriptText;
        try
        {
            transcriptText = ExtractTextFromPdf(file);
            if (string.IsNullOrWhiteSpace(transcriptText))
            {
                Console.WriteLine("Extracted text is empty.");
                return BadRequest(new { message = "ไฟล์ดังกล่าวไม่ใช่ PDF" });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing PDF file: {ex.Message}");
            return BadRequest(new { message = "ไฟล์ดังกล่าวไม่ใช่ PDF" });
        }

        transcriptText = transcriptText.Replace("Date Issued:", "");
        transcriptText = transcriptText.Replace("This document is", "");
        transcriptText = transcriptText.Replace("Transfer Credit", "\n-1,-1\n");

        var regex = SplitToLineBySubjectIdRegex();
        transcriptText = regex.Replace(transcriptText, m => "\n" + m.Value);
        transcriptText = string.Join("\n", transcriptText.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)));


        regex = CleanTermYearHeader();
        transcriptText = regex.Replace(transcriptText, m =>
        {
            var match = m.Value;
            var semesterMatch = FindTermYearHeader().Match(match);
            if (!semesterMatch.Success) return match;
            var semester = semesterMatch.Groups[1].Value;
            var year = semesterMatch.Groups[2].Value;
            return $"\n{semester},{year}\n";
        });

        var subjectLines = transcriptText.Split('\n')
            .Select(ProcessSubjectLine)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        transcriptText = string.Join("\n", subjectLines);

        var lines = transcriptText.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();

        var extractedTerm = 0;
        var extractedYear = 0;
        var extractedSubjectId = string.Empty;
        var extractedGrade = string.Empty;

        Teachtable? currentTeachtable = null;

        SdmTranscript.Insert(new Transcript(-1, user, SdmDateTime.Now()));
        var transcript = SdmTranscript.GetBy(user);

        if (transcript == null)
        {
            Console.WriteLine("Cannot create transcript.");
            return BadRequest(new { message = "เกิดปัญหาการสร้างข้อมูล Transcript" });
        }

        foreach (var line in lines)
        {
            var semesterMatch = DetectTermYearLineRegex().Match(line);
            if (semesterMatch.Success)
            {
                extractedTerm = int.Parse(semesterMatch.Groups[1].Value);
                extractedYear = int.Parse(semesterMatch.Groups[2].Value);

                continue;
            }

            var subjectMatch = DetectSubjectLineRegex().Match(line);
            if (subjectMatch.Success)
            {
                extractedSubjectId = subjectMatch.Groups[1].Value;
                extractedGrade = subjectMatch.Groups[2].Value;
            }

            if (extractedYear != 0 && extractedTerm != 0) currentTeachtable = SdmTeachtable.GetBy(extractedYear, extractedTerm);
            if (currentTeachtable != null && extractedSubjectId != string.Empty && extractedGrade != string.Empty)
                SdmTranscriptDetail.Insert(new TranscriptDetail(
                    -1,
                    transcript,
                    SdmSubject.GetBy(extractedSubjectId),
                    currentTeachtable,
                    extractedGrade
                ));

            extractedTerm = 0;
            extractedYear = 0;
            extractedSubjectId = string.Empty;
            extractedGrade = string.Empty;
        }

        return Ok();
    }

    public class DtoUploadTranscript
    {
        public required int Id { get; init; } = 1;
        public required IFormFile? File { get; init; } = null;
    }
    #endregion
    #region [DELETE] Delete Transcript
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpDelete("delete/{userId:int}")]
    public ActionResult DeleteTranscript(int userId)
    {
        DeleteTranscriptData(userId);
        return Ok();
    }
    private void DeleteTranscriptData(int userId)
    {
        var user = SdmUser.GetBy(userId);
        if (user == null)
            return;

        var transcript = SdmTranscript.GetBy(user);
        if (transcript == null)
            return;
        SdmTranscriptDetail.DeleteBy(transcript);
        SdmTranscript.DeleteBy(transcript);
    }
    #endregion

    #region Helper Methods
    private static string ProcessSubjectLine(string line)
    {
        var subjectMatch = FindSubjectIdRegex().Match(line);
        if (!subjectMatch.Success)
            return line;

        var subjectId = subjectMatch.Groups[1].Value;

        var gradeMatch = FindGradeRegex().Match(line);

        var grade = "X";
        if (gradeMatch.Success) grade = gradeMatch.Groups[1].Success ? gradeMatch.Groups[1].Value : gradeMatch.Groups[2].Value;

        return $"{subjectId},{grade}";
    }
    private static bool IsWithinBounds(PdfRectangle wordBounds, PdfRectangle columnBounds)
    {
        return wordBounds.Left >= columnBounds.Left &&
               wordBounds.Right <= columnBounds.Right &&
               wordBounds.Bottom >= columnBounds.Bottom &&
               wordBounds.Top <= columnBounds.Top;
    }
    private static string ExtractTextFromPdf(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            using var pdfDocument = PdfDocument.Open(stream);

            var textTop = new StringBuilder();
            var textLeft = new StringBuilder();
            var textRight = new StringBuilder();

            foreach (var page in pdfDocument.GetPages())
            {
                var pageWidth = page.Width;
                var pageHeight = page.Height;

                const double topPercent = 0.22;
                const double topIgnorePercent = 0.03;
                var topBoxHeight = pageHeight * (topPercent - topIgnorePercent);
                var topBoxBounds = new PdfRectangle(0, pageHeight - topBoxHeight, pageWidth, pageHeight);

                const double bottomIgnorePercent = 0.1;
                var leftColumnBounds = new PdfRectangle(0, pageHeight * bottomIgnorePercent, pageWidth / 2,
                    pageHeight - pageHeight * topPercent);
                var rightColumnBounds = new PdfRectangle(pageWidth / 2, pageHeight * bottomIgnorePercent, pageWidth,
                    pageHeight - pageHeight * topPercent);

                var topBoxText = page.GetWords().Where(word => IsWithinBounds(word.BoundingBox, topBoxBounds))
                    .Select(word => word.Text).ToArray();
                textTop.Append(string.Join(" ", topBoxText));

                var leftText = page.GetWords().Where(word => IsWithinBounds(word.BoundingBox, leftColumnBounds))
                    .Select(word => word.Text).ToArray();
                textLeft.Append(string.Join(" ", leftText));

                var rightText = page.GetWords().Where(word => IsWithinBounds(word.BoundingBox, rightColumnBounds))
                    .Select(word => word.Text).ToArray();
                textRight.Append(string.Join(" ", rightText)).Append(' ');
            }

            var resultLeft = RemoveGpsGpaRegex().Replace(textLeft.ToString(), "");
            resultLeft = RemoveCheckedByRegex().Replace(resultLeft, "");
            resultLeft = RemoveTranscriptMarkersRegex().Replace(resultLeft, "");
            resultLeft = RemoveCreditCumulativeRegex().Replace(resultLeft, "");
            resultLeft = RemoveAccessSpaceRegex().Replace(resultLeft, " ");

            var resultRight = RemoveGpsGpaRegex().Replace(textRight.ToString(), "");
            resultRight = RemoveCheckedByRegex().Replace(resultRight, "");
            resultRight = RemoveTranscriptMarkersRegex().Replace(resultRight, "");
            resultRight = RemoveCreditCumulativeRegex().Replace(resultRight, "");
            resultRight = RemoveAccessSpaceRegex().Replace(resultRight, " ");

            return (resultLeft + " " + resultRight).Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting text from PDF: {ex.Message}");
            throw;
        }
    }

    private static async Task<bool> IsValidPdf(IFormFile file)
    {
        var pdfSignature = "%PDF-"u8.ToArray();
        var fileHeader = new byte[5];

        try
        {
            await using var stream = file.OpenReadStream();
            if (stream.Length < 5)
                return false;

            _ = await stream.ReadAsync(fileHeader.AsMemory(0, 5));

            return !pdfSignature.Where((t, i) => fileHeader[i] != t).Any();
        }
        catch
        {
            return false;
        }
    }
    #endregion
    #region Regex
    [GeneratedRegex(@"Checked by\s+[\w\s\(\)]+")]
    private static partial Regex RemoveCheckedByRegex();
    [GeneratedRegex("----------------------------- Continue next column -----------------------------|-------------------------------- Transcript Closed --------------------------------", RegexOptions.IgnoreCase)]
    private static partial Regex RemoveTranscriptMarkersRegex();
    [GeneratedRegex(@"GPS\s*:\s*\S+|GPA\s*:\s*\S+", RegexOptions.IgnoreCase)]
    private static partial Regex RemoveGpsGpaRegex();
    [GeneratedRegex(@"Total number of credit earned: \d+ Cumulative")]
    private static partial Regex RemoveCreditCumulativeRegex();
    [GeneratedRegex(@"\s+")]
    private static partial Regex RemoveAccessSpaceRegex();
    [GeneratedRegex(@"(\d+(?:st|nd|rd|th)) Semester,\s*Year,\s*(\d{4}-\d{4})", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex CleanTermYearHeader();
    [GeneratedRegex(@"\b\d{8}\b")]
    private static partial Regex SplitToLineBySubjectIdRegex();
    [GeneratedRegex(@"(\d+)(?:st|nd|rd|th) Semester,\s*Year,\s*(\d{4})-\d{4}")]
    private static partial Regex FindTermYearHeader();
    [GeneratedRegex(@"\b(\d{8})\b")]
    private static partial Regex FindSubjectIdRegex();
    [GeneratedRegex(@"\s(A\+?|B\+?|C\+?|D\+?|F|S)\s|\s(A\+?|B\+?|C\+?|D\+?|F|S)$", RegexOptions.RightToLeft)]
    private static partial Regex FindGradeRegex();
    [GeneratedRegex(@"^(\d{8}),(A\+?|B\+?|C\+?|D\+?|F|S|X)$")]
    private static partial Regex DetectSubjectLineRegex();
    [GeneratedRegex(@"^(\d),(\d{4})$")]
    private static partial Regex DetectTermYearLineRegex();
    #endregion
}