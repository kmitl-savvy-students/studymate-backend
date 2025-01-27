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
    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> Upload([FromForm] DtoUploadTranscript dtoUploadTranscript)
    {
        Console.WriteLine("=== Starting Transcript Upload Process ===");

        var file = dtoUploadTranscript.file;
        var userId = SdmString.CleanAndTrim(dtoUploadTranscript.id);
        var userIdInt = int.Parse(userId);

        Console.WriteLine($"User ID: {userId}");

        Console.WriteLine("Verifying files and permissions...");

        var user = SdmUser.GetBy(userIdInt);
        if (user?.Curriculum == null)
        {
            Console.WriteLine("User not allowed to upload transcript (no curriculum).");
            return NotFound(new { message = "User is not allow to upload transcript." });
        }

        if (file.Length == 0)
        {
            Console.WriteLine("File is empty.");
            return BadRequest(new { message = "File is empty." });
        }

        const long maxFileSize = 15 * 1024 * 1024; // 15 MB Max
        if (file.Length > maxFileSize)
        {
            Console.WriteLine("File is too large.");
            return BadRequest(new { message = "File is too large." });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf")
        {
            Console.WriteLine("File is not a PDF file (invalid extension).");
            return BadRequest(new { message = "File is not a PDF file." });
        }

        if (file.ContentType != "application/pdf")
        {
            Console.WriteLine("File is not a PDF file (invalid content type).");
            return BadRequest(new { message = "File is not a PDF file." });
        }

        if (!await IsValidPdf(file))
        {
            Console.WriteLine("File is not a valid PDF file (failed PDF signature check).");
            return BadRequest(new { message = "File is not a PDF file." });
        }

        Console.WriteLine("Starting extract string from PDF using PDFPig...");
        Console.WriteLine("=========== " + file.FileName + " ===========");

        ParseData data;
        try
        {
            data = ExtractTextFromPdf(file);

            if (string.IsNullOrWhiteSpace(data.Data))
            {
                Console.WriteLine("Extracted text is empty.");
                return BadRequest(new { message = "File is empty." });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing PDF file: {ex.Message}");
            return BadRequest(new { message = "Cannot process PDF file." });
        }

        Console.WriteLine("==== START PDFPig Transcript result ====");
        Console.WriteLine(data.Data);
        Console.WriteLine("==== END PDFPig Transcript result ====");

        Console.WriteLine("Verifying result and permissions...");

        if (string.IsNullOrEmpty(data.Id))
        {
            Console.WriteLine("No valid student ID found in transcript.");
            return NotFound(new { message = "Not a valid transcript." });
        }

        var dataIdInt = int.Parse(data.Id);

        var userTranscript = SdmUser.GetBy(dataIdInt);
        if (userTranscript == null)
        {
            Console.WriteLine("User in transcript not found in database.");
            return NotFound(new { message = "User doesn't exist." });
        }

        if (userTranscript.Id != user.Id)
        {
            Console.WriteLine("User in transcript does not match the current user (unauthorized).");
            return Unauthorized(new { message = "Unauthorized." });
        }

        Console.WriteLine("User verified successfully.");

        Console.WriteLine("Cleaning transcript text (removing boilerplate text)...");
        var transcriptText = data.Data;
        transcriptText = transcriptText.Replace("Date Issued:", "");
        transcriptText = transcriptText.Replace("This document is", "");

        var semesterHeaderPattern = SemesterHeaderRegex();

        Console.WriteLine("Inserting new lines before Transfer Credit and semester headings...");
        transcriptText = transcriptText.Replace("Transfer Credit", "\nTransfer Credit\n");
        transcriptText = semesterHeaderPattern.Replace(transcriptText, m => "\n" + m.Value + "\n");

        var subjectIdPattern = SubjectIdRegex();
        transcriptText = subjectIdPattern.Replace(transcriptText, m => "\n" + m.Value);

        Console.WriteLine("Splitting transcript text into lines...");
        var lines = transcriptText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var validGrades = new HashSet<string> { "A", "A+", "B", "B+", "C", "C+", "D", "D+", "F", "S" };

        Console.WriteLine("Parsing lines...");

        var currentSemester = -1;
        var currentYear = -1;
        var inTransferCreditSection = false;

        var transferCredits = new List<(string subject_id, string grade)>();
        var gradedCourses = new List<(int semester, int year, string subject_id, string grade)>();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            if (line.Equals("Transfer Credit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Entering Transfer Credit section...");
                inTransferCreditSection = true;
                currentSemester = -1;
                currentYear = -1;
                continue;
            }

            var semesterMatch = semesterHeaderPattern.Match(line);
            if (semesterMatch.Success)
            {
                var semesterStr = semesterMatch.Groups[1].Value;
                var yearRange = semesterMatch.Groups[2].Value;

                var semNumStr = new string(semesterStr.Where(char.IsDigit).ToArray());
                if (int.TryParse(semNumStr, out var semNum))
                    currentSemester = semNum;
                else
                    currentSemester = -1;

                var years = yearRange.Split('-');
                if (years.Length > 0 && int.TryParse(years[0], out var yr))
                    currentYear = yr;
                else
                    currentYear = -1;

                Console.WriteLine($"Semester detected: {currentSemester}, Year: {currentYear}");
                inTransferCreditSection = false;
                continue;
            }

            var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length >= 2 && subjectIdPattern.IsMatch(tokens[0]))
            {
                var subjectId = tokens[0];
                var grade = "X";

                var creditIndex = -1;
                for (var i = 1; i < tokens.Length; i++)
                    if (int.TryParse(tokens[i].TrimEnd(','), out _))
                    {
                        creditIndex = i;
                        break;
                    }

                if (creditIndex != -1)
                    for (var j = creditIndex + 1; j < tokens.Length; j++)
                    {
                        var possibleGrade = tokens[j].TrimEnd(',');
                        if (subjectIdPattern.IsMatch(possibleGrade))
                            break;

                        if (!validGrades.Contains(possibleGrade)) continue;
                        grade = possibleGrade;
                        break;
                    }

                Console.WriteLine($"Found course: {subjectId} with grade: {grade}");

                if (inTransferCreditSection)
                {
                    transferCredits.Add((subjectId, grade));
                }
                else
                {
                    if (currentSemester == -1 || currentYear == -1)
                        Console.WriteLine("Warning: Course found outside known semester/year. Using default -1, -1.");

                    gradedCourses.Add((currentSemester, currentYear, subjectId, grade));
                }

                continue;
            }

            Console.WriteLine($"Unrecognized line, skipping: {line}");
        }

        Console.WriteLine("Saving data to database...");

        try
        {
            var transcript = new Transcript(
                0,
                user,
                user.Curriculum,
                new SdmDateTime(DateTime.UtcNow)
            );

            transcript = SdmTranscript.Insert(transcript);

            foreach (var (subject_id, grade) in transferCredits)
            {
                var subject = SdmSubject.GetBy(subject_id);
                if (subject == null)
                {
                    Console.WriteLine($"Subject {subject_id} not found in DB, skipping...");
                    continue;
                }

                var dataEntry = new TranscriptDetail(
                    0,
                    transcript,
                    subject,
                    null,
                    grade,
                    subject.Credit
                );
                SdmTranscriptDetail.Insert(dataEntry);
                Console.WriteLine($"Inserted Transfer Credit: {subject_id} Grade: {grade} Credit: {subject.Credit}");
            }

            foreach (var (semester, year, subject_id, grade) in gradedCourses)
            {
                var subject = SdmSubject.GetBy(subject_id);
                if (subject == null)
                {
                    Console.WriteLine($"Subject {subject_id} not found in DB, skipping...");
                    continue;
                }

                var dataEntry = new TranscriptDetail(
                    0,
                    transcript,
                    subject,
                    null,
                    grade,
                    subject.Credit
                );
                SdmTranscriptDetail.Insert(dataEntry);
                Console.WriteLine(
                    $"Inserted Course: {subject_id} Semester: {semester}, Year: {year}, Grade: {grade}, Credit: {subject.Credit}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving transcript data: {ex.Message}");
            return BadRequest(new { message = "Error saving data to database." });
        }

        Console.WriteLine("DONE!! Transcript uploaded and data saved successfully.");

        return Ok(new { message = "Upload successfully." });
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

    private static ParseData ExtractTextFromPdf(IFormFile file)
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

            var resultTop = ExtractStudentIdRegex().Match(textTop.ToString()).Value;

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

            return new ParseData(resultTop, (resultLeft + " " + resultRight).Trim());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting text from PDF: {ex.Message}");
            throw;
        }
    }

    private static bool IsWithinBounds(PdfRectangle wordBounds, PdfRectangle columnBounds)
    {
        return wordBounds.Left >= columnBounds.Left &&
               wordBounds.Right <= columnBounds.Right &&
               wordBounds.Bottom >= columnBounds.Bottom &&
               wordBounds.Top <= columnBounds.Top;
    }

    private class ParseData(string id, string data)
    {
        public string Id { get; } = id;
        public string Data { get; } = data;
    }

    public class DtoUploadTranscript(string id, IFormFile file)
    {
        public required string id { get; init; } = id;
        public required IFormFile file { get; init; } = file;
    }

    #region Regex
    [GeneratedRegex(@"Checked by\s+[\w\s\(\)]+")]
    private static partial Regex RemoveCheckedByRegex();
    [GeneratedRegex("----------------------------- Continue next column -----------------------------|-------------------------------- Transcript Closed --------------------------------", RegexOptions.IgnoreCase)]
    private static partial Regex RemoveTranscriptMarkersRegex();
    [GeneratedRegex(@"GPS\s*:\s*\S+|GPA\s*:\s*\S+", RegexOptions.IgnoreCase)]
    private static partial Regex RemoveGpsGpaRegex();
    [GeneratedRegex(@"\b\d{8}\b")]
    private static partial Regex ExtractStudentIdRegex();
    [GeneratedRegex(@"Total number of credit earned: \d+ Cumulative")]
    private static partial Regex RemoveCreditCumulativeRegex();
    [GeneratedRegex(@"\s+")]
    private static partial Regex RemoveAccessSpaceRegex();
    [GeneratedRegex(@"(\d+(?:st|nd|rd|th)) Semester,\s*Year,\s*(\d{4}-\d{4})", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex SemesterHeaderRegex();
    [GeneratedRegex(@"\b\d{8}\b")]
    private static partial Regex SubjectIdRegex();
    #endregion
}