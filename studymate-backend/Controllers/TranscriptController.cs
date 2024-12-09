using System.Text;
using System.Text.RegularExpressions;
using Google.Cloud.AIPlatform.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using studymate_backend.Libraries.Helper;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Core;
using System.Linq;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/transcript")]
public partial class TranscriptController : ControllerBase
{
    // AI model constants (not used now, but kept for reference)
    private const string AI_MODEL = "gemini-1.5-flash-002";
    private const float AI_TEMPERATURE = 1.0f;

    // Regex patterns generated
    [GeneratedRegex(@"Checked by\s+[\w\s\(\)]+")]
    private static partial Regex RemoveCheckedByRegex();

    [GeneratedRegex(@"----------------------------- Continue next column -----------------------------|-------------------------------- Transcript Closed --------------------------------", RegexOptions.IgnoreCase)]
    private static partial Regex RemoveTranscriptMarkersRegex();

    [GeneratedRegex(@"GPS\s*:\s*\S+|GPA\s*:\s*\S+", RegexOptions.IgnoreCase)]
    private static partial Regex RemoveGpsGpaRegex();

    [GeneratedRegex(@"\b\d{8}\b")]
    private static partial Regex ExtractStudentIdRegex();

    [GeneratedRegex(@"Total number of credit earned: \d+ Cumulative")]
    private static partial Regex RemoveCreditCumulativeRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex RemoveAccessSpaceRegex();

    [Authorize(AuthenticationSchemes = "StudyMateToken")]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> Upload([FromForm] DtoUploadTranscript dtoUploadTranscript)
    {
        Console.WriteLine("=== Starting Transcript Upload Process ===");

        var file = dtoUploadTranscript.file;
        var userId = SdmString.CleanAndTrim(dtoUploadTranscript.id);

        Console.WriteLine($"User ID: {userId}");

        // ====== [SECTION] START VERIFY USER AND FILE ======
        Console.WriteLine("Verifying files and permissions...");

        var user = SdmUser.GetBy(userId);
        if (user?.curriculum == null)
        {
            Console.WriteLine("User not allowed to upload transcript (no curriculum).");
            return NotFound(new { message = "User is not allow to upload transcript." });
        }

        // Check file size and type
        if (file.Length == 0)
        {
            Console.WriteLine("File is empty.");
            return BadRequest(new { message = "File is empty." });
        }

        const long maxFileSize = 15 * 1024 * 1024; // 15 MB max
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

        // ====== [SECTION] END VERIFY USER AND FILE ======

        // ====== [SECTION] EXTRACT PDF TEXT ======
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

        // ====== [SECTION] VERIFY USER ID IN TRANSCRIPT ======
        Console.WriteLine("Verifying result and permissions...");

        if (string.IsNullOrEmpty(data.Id))
        {
            Console.WriteLine("No valid student ID found in transcript.");
            return NotFound(new { message = "Not a valid transcript." });
        }

        var userTranscript = SdmUser.GetBy(data.Id);
        if (userTranscript == null)
        {
            Console.WriteLine("User in transcript not found in database.");
            return NotFound(new { message = "User doesn't exist." });
        }

        if (userTranscript.id != user.id)
        {
            Console.WriteLine("User in transcript does not match the current user (unauthorized).");
            return Unauthorized(new { message = "Unauthorized." });
        }

        Console.WriteLine("User verified successfully.");

        // ====== [SECTION] PARSING THE TRANSCRIPT WITH REGEX ======

        Console.WriteLine("Cleaning transcript text (removing boilerplate text)...");
        string transcriptText = data.Data;
        transcriptText = transcriptText.Replace("Date Issued:", "");
        transcriptText = transcriptText.Replace("This document is", "");

        // Regex for semester headings
        var semesterHeaderPattern = new Regex(@"(\d+(?:st|nd|rd|th)) Semester,\s*Year,\s*(\d{4}-\d{4})", RegexOptions.IgnoreCase);

        // Insert new lines before these keywords/patterns
        Console.WriteLine("Inserting new lines before Transfer Credit and semester headings...");
        transcriptText = transcriptText.Replace("Transfer Credit", "\nTransfer Credit\n");
        transcriptText = semesterHeaderPattern.Replace(transcriptText, m => "\n" + m.Value + "\n");

        // Before each subject ID (8 digits)
        var subjectIdPattern = new Regex(@"\b\d{8}\b");
        transcriptText = subjectIdPattern.Replace(transcriptText, m => "\n" + m.Value);

        Console.WriteLine("Splitting transcript text into lines...");
        var lines = transcriptText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Known valid grades
        var validGrades = new HashSet<string> { "A","A+","B","B+","C","C+","D","D+","F","S" };

        Console.WriteLine("Parsing lines...");

        // Variables to track current semester and year
        int currentSemester = -1;
        int currentYear = -1;
        bool inTransferCreditSection = false;

        // We'll store the parsed results (transfer credits and graded courses)
        var transferCredits = new List<(string subject_id, string grade)>();
        var gradedCourses = new List<(int semester, int year, string subject_id, string grade)>();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Check if line is "Transfer Credit"
            if (line.Equals("Transfer Credit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Entering Transfer Credit section...");
                inTransferCreditSection = true;
                currentSemester = -1;
                currentYear = -1;
                continue;
            }

            // Check if line is a semester header
            var semesterMatch = semesterHeaderPattern.Match(line);
            if (semesterMatch.Success)
            {
                // Parse semester and year from the line
                // e.g. "1st Semester 2021-2022"
                var semesterStr = semesterMatch.Groups[1].Value; // "1st"
                var yearRange = semesterMatch.Groups[2].Value;   // "2021-2022"

                // Extract semester number from "1st", "2nd", etc.
                // We'll just remove non-digits and parse
                var semNumStr = new string(semesterStr.Where(char.IsDigit).ToArray());
                if (int.TryParse(semNumStr, out var semNum))
                {
                    currentSemester = semNum;
                }
                else
                {
                    currentSemester = -1; // if parse fails
                }

                // Extract just the first year from something like "2021-2022"
                // We can split by '-' and take the first as "start year"
                var years = yearRange.Split('-');
                if (years.Length > 0 && int.TryParse(years[0], out var yr))
                {
                    currentYear = yr;
                }
                else
                {
                    currentYear = -1;
                }

                Console.WriteLine($"Semester detected: {currentSemester}, Year: {currentYear}");
                inTransferCreditSection = false; // Not in transfer credit now
                continue;
            }

            // If this is a subject line (start with 8-digit subject code)
            var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length >= 2 && subjectIdPattern.IsMatch(tokens[0]))
            {
                string subjectId = tokens[0];
                string grade = "X"; // default if not found

                // Find the first integer token after subjectId -> credit
                // (We won't use this credit from the line, but we must at least identify the position)
                int creditIndex = -1;
                for (int i = 1; i < tokens.Length; i++)
                {
                    if (int.TryParse(tokens[i].TrimEnd(','), out _))
                    {
                        creditIndex = i;
                        break;
                    }
                }

                // After finding credit index, look for a grade
                if (creditIndex != -1)
                {
                    for (int j = creditIndex + 1; j < tokens.Length; j++)
                    {
                        var possibleGrade = tokens[j].TrimEnd(',');
                        // If we see another subject ID, stop
                        if (subjectIdPattern.IsMatch(possibleGrade))
                            break;

                        if (validGrades.Contains(possibleGrade))
                        {
                            grade = possibleGrade;
                            break;
                        }
                    }
                }

                Console.WriteLine($"Found course: {subjectId} with grade: {grade}");

                // If we're in transfer credit section, semester/year = -1, -1
                if (inTransferCreditSection)
                {
                    transferCredits.Add((subjectId, grade));
                }
                else
                {
                    // Normal graded course
                    if (currentSemester == -1 || currentYear == -1)
                    {
                        Console.WriteLine("Warning: Course found outside known semester/year. Using default -1, -1.");
                    }

                    gradedCourses.Add((currentSemester, currentYear, subjectId, grade));
                }

                continue;
            }

            // If we reach here, it's a line we don't recognize (e.g. course name line break, we skip)
            Console.WriteLine($"Unrecognized line, skipping: {line}");
        }

        // ====== [SECTION] SAVE DATA TO DATABASE ======
        Console.WriteLine("Saving data to database...");

        try
        {
            // Create a new Transcript entry
            var transcript = new Transcript(
                0,
                user,
                user.curriculum,
                new SdmDateTime(DateTime.UtcNow)
            );

            // Insert the transcript record
            transcript = SdmTranscript.Insert(transcript);

            // Insert transfer credits
            foreach (var (subject_id, grade) in transferCredits)
            {
                // Get subject record from DB to get credit
                var subject = SdmSubject.GetBy(subject_id);
                if (subject == null)
                {
                    Console.WriteLine($"Subject {subject_id} not found in DB, skipping...");
                    continue;
                }

                // Semester = -1, year = -1 for transfer credits
                var dataEntry = new TranscriptData(
                    0,
                    transcript,
                    subject,
                    -1,
                    -1,
                    grade,
                    subject.credit // get credit from subject, not from line
                );
                SdmTranscriptData.Insert(dataEntry);
                Console.WriteLine($"Inserted Transfer Credit: {subject_id} Grade: {grade} Credit: {subject.credit}");
            }

            // Insert graded courses
            foreach (var (semester, year, subject_id, grade) in gradedCourses)
            {
                var subject = SdmSubject.GetBy(subject_id);
                if (subject == null)
                {
                    Console.WriteLine($"Subject {subject_id} not found in DB, skipping...");
                    continue;
                }

                var dataEntry = new TranscriptData(
                    0,
                    transcript,
                    subject,
                    semester,
                    year,
                    grade,
                    subject.credit
                );
                SdmTranscriptData.Insert(dataEntry);
                Console.WriteLine($"Inserted Course: {subject_id} Semester: {semester}, Year: {year}, Grade: {grade}, Credit: {subject.credit}");
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

    /// <summary>
    /// Check if the PDF is valid by reading its signature.
    /// </summary>
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

            // Check if the header matches "%PDF-"
            return !pdfSignature.Where((t, i) => fileHeader[i] != t).Any();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extract text from PDF using PDFPig, separated into top, left, right sections, then cleaned up.
    /// </summary>
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

                // Define regions for top, left, right text extraction
                const double topPercent = 0.22;
                const double topIgnorePercent = 0.03;
                var topBoxHeight = pageHeight * (topPercent - topIgnorePercent);
                var topBoxBounds = new PdfRectangle(0, pageHeight - topBoxHeight, pageWidth, pageHeight);

                const double bottomIgnorePercent = 0.1;
                var leftColumnBounds = new PdfRectangle(0, pageHeight * bottomIgnorePercent, pageWidth / 2, pageHeight - pageHeight * topPercent);
                var rightColumnBounds = new PdfRectangle(pageWidth / 2, pageHeight * bottomIgnorePercent, pageWidth, pageHeight - pageHeight * topPercent);

                // Extract words from each region
                var topBoxText = page.GetWords().Where(word => IsWithinBounds(word.BoundingBox, topBoxBounds)).Select(word => word.Text).ToArray();
                textTop.Append(string.Join(" ", topBoxText));

                var leftText = page.GetWords().Where(word => IsWithinBounds(word.BoundingBox, leftColumnBounds)).Select(word => word.Text).ToArray();
                textLeft.Append(string.Join(" ", leftText));

                var rightText = page.GetWords().Where(word => IsWithinBounds(word.BoundingBox, rightColumnBounds)).Select(word => word.Text).ToArray();
                textRight.Append(string.Join(" ", rightText)).Append(' ');
            }

            // Extract student ID from top section
            var resultTop = ExtractStudentIdRegex().Match(textTop.ToString()).Value;

            // Clean left and right text
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

    /// <summary>
    /// Check if a word is within specified bounding box (for PDF text extraction).
    /// </summary>
    private static bool IsWithinBounds(PdfRectangle wordBounds, PdfRectangle columnBounds)
    {
        return wordBounds.Left >= columnBounds.Left &&
               wordBounds.Right <= columnBounds.Right &&
               wordBounds.Bottom >= columnBounds.Bottom &&
               wordBounds.Top <= columnBounds.Top;
    }

    private class ParseData
    {
        public string Id { get; }
        public string Data { get; }

        public ParseData(string id, string data)
        {
            Id = id;
            Data = data;
        }
    }

    public class DtoUploadTranscript
    {
        public required string id { get; set; }
        public required IFormFile file { get; set; }
    }
}
