using System.Text;
using Google.Api.Gax.Grpc;
using Google.Cloud.AIPlatform.V1;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using studymate_backend.Controllers.Core;
using studymate_backend.Enums;
using studymate_backend.Helper;
using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Object;
using studymate_backend.Models.StudyMate.Raw.Request.Transcript;
using studymate_backend.Services;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Core;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/transcript")]
public class TranscriptController(
    TranscriptService transcriptService,
    TranscriptDataService transcriptDataService,
    UserService userService,
    UserTokenService userTokenService
) : IController
{
    private struct TranscriptDataStructure(string student_id, List<TransferCredit> transfer_credits, List<SemesterGrade> grades)
    {
        public string student_id { get; } = student_id;
        public List<TransferCredit> transfer_credits { get; } = transfer_credits;
        public List<SemesterGrade> grades { get; } = grades;
    }

    private readonly struct TransferCredit(string subject_id, string grade, int credit)
    {
        public string subject_id { get; } = subject_id;
        public string grade { get; } = grade;
        public int credit { get; } = credit;
    }

    private readonly struct SemesterGrade(int semester, int year, List<CourseGrade> courses)
    {
        public int semester { get; } = semester;
        public int year { get; } = year;
        public List<CourseGrade> courses { get; } = courses;
    }

    private readonly struct CourseGrade(string subject_id, string grade, int credit)
    {
        public string subject_id { get; } = subject_id;
        public string grade { get; } = grade;
        public int credit { get; } = credit;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<BaseResponse> Upload([FromForm] RequestTranscriptUpload requestTranscriptUpload)
    {
        var file = requestTranscriptUpload.File;

        if (!SdmString.IsValid(requestTranscriptUpload.UserTokenId, 64, 64))
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        var userTokenId = SdmString.cleanAndTrim(requestTranscriptUpload.UserTokenId);

        // Verify token
        var userToken = userTokenService.Get(userTokenId);
        if (userToken == null)
            return new BaseResponse(EnumResponseCode.UNAUTHORIZED);

        // Verify Files
        if (file == null || file.Length == 0)
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        const long maxFileSize = 15 * 1024 * 1024;
        if (file.Length > maxFileSize)
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf")
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        if (file.ContentType != "application/pdf")
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        if (!await IsValidPdf(file))
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        // Extract text content from PDF
        var fileContent = ExtractTextFromPdf(file);

        if (string.IsNullOrEmpty(fileContent))
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID);

        // AI prompt to extract data from the PDF content
        const string projectId = "savvy-studymate";
        const string location = "us-central1";
        const string publisher = "google";
        const string model = "gemini-1.5-flash-001";

        var prompt =
            "Extract data as the following JSON structure: { \"student_id\": \"\", \"transfer_credits\": [ // If it exists { \"subject_id\": \"\", \"grade\": \"\", // 0 if blank, A, B, C, D \"credit\": \"\", // 0, 1, 2, 3 } ], \"grades\": [ { \"semester\": \"\", // only number 1, 2, 3 \"year\": \"\", // start year only \"courses\": { [ \"subject_id\": \"\", \"grade\": \"\" // 0 if blank, A, B, C, D \"credit\": \"\", // 0, 1, 2, 3 ] } } ] }: " +
            fileContent;
        var content = await GenerateContentInternal(projectId, location, publisher, model, prompt);
        var cleanedContent = content.Replace("```json", "").Replace("```", "").Trim();

        Console.WriteLine(cleanedContent);

        // Parse JSON content into C# object
        var transcriptData = JsonConvert.DeserializeObject<TranscriptDataStructure>(cleanedContent);

        // Check for user in transcript
        var user = userService.Get(transcriptData.student_id);
        if (user == null)
            return new BaseResponse(EnumResponseCode.NOT_FOUND);

        // Check if user matches
        if (user.Id != userToken.User.Id)
            return new BaseResponse(EnumResponseCode.UNAUTHORIZED);

        // Create a new Transcript entry
        var transcript = new Transcript(0, transcriptData.student_id, 3, DateTime.UtcNow);
        var transcriptId = transcriptService.Add(transcript);

        // Add transfer credits as TranscriptData
        foreach (var transcriptDataEntry in transcriptData.transfer_credits.Select(transferCredit => new TranscriptData(
                     0,
                     transcriptId,
                     transferCredit.subject_id,
                     -1,
                     -1,
                     transferCredit.grade,
                     transferCredit.credit
                 )))
            transcriptDataService.Add(transcriptDataEntry);

        // Add grades for each semester
        foreach (var transcriptDataEntry in from semesterGrade in transcriptData.grades
                 from course in semesterGrade.courses
                 select new TranscriptData(
                     0,
                     transcriptId,
                     course.subject_id,
                     semesterGrade.semester,
                     semesterGrade.year,
                     course.grade,
                     course.credit
                 ))
            transcriptDataService.Add(transcriptDataEntry);

        return new BaseResponse(EnumResponseCode.OK);
    }

    private static async Task<bool> IsValidPdf(IFormFile file)
    {
        var pdfSignature = "%PDF-"u8.ToArray();
        var fileHeader = new byte[5];

        try
        {
            await using (var stream = file.OpenReadStream())
            {
                if (stream.Length < 5)
                    return false;

                _ = await stream.ReadAsync(fileHeader.AsMemory(0, 5));
            }

            return !pdfSignature.Where((t, i) => fileHeader[i] != t).Any();
        }
        catch
        {
            return false;
        }
    }

    private static string ExtractTextFromPdf(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            using var pdfDocument = PdfDocument.Open(stream);
            var text = new StringBuilder();

            foreach (var page in pdfDocument.GetPages())
            {
                var pageWidth = page.Width;
                var pageHeight = page.Height;

                var leftColumnBounds = new PdfRectangle(0, 0, pageWidth / 2, pageHeight);
                var rightColumnBounds = new PdfRectangle(pageWidth / 2, 0, pageWidth, pageHeight);

                var leftText = page.GetWords().Where(word => IsWithinBounds(word.BoundingBox, leftColumnBounds)).Select(word => word.Text).ToArray();
                text.Append(string.Join(" ", leftText)).Append(' ');

                var rightText = page.GetWords().Where(word => IsWithinBounds(word.BoundingBox, rightColumnBounds)).Select(word => word.Text).ToArray();
                text.Append(string.Join(" ", rightText)).Append(' ');
            }

            return text.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting text from PDF: {ex.Message}");
            return string.Empty;
        }
    }

    private static bool IsWithinBounds(PdfRectangle wordBounds, PdfRectangle columnBounds)
    {
        return wordBounds.Left >= columnBounds.Left &&
               wordBounds.Right <= columnBounds.Right &&
               wordBounds.Bottom >= columnBounds.Bottom &&
               wordBounds.Top <= columnBounds.Top;
    }

    private static async Task<string> GenerateContentInternal(string projectId, string location, string publisher, string model, string prompt)
    {
        var predictionServiceClient = await new PredictionServiceClientBuilder
        {
            Endpoint = $"{location}-aiplatform.googleapis.com"
        }.BuildAsync();

        var generateContentRequest = new GenerateContentRequest
        {
            Model = $"projects/{projectId}/locations/{location}/publishers/{publisher}/models/{model}",
            GenerationConfig = new GenerationConfig
            {
                Temperature = 0.1f,
                TopP = 0.5f,
                TopK = 40,
                MaxOutputTokens = 4096
            },
            Contents =
            {
                new Content
                {
                    Role = "USER",
                    Parts =
                    {
                        new Part { Text = prompt }
                    }
                }
            }
        };

        using var response = predictionServiceClient.StreamGenerateContent(generateContentRequest);

        StringBuilder fullText = new();

        AsyncResponseStream<GenerateContentResponse> responseStream = response.GetResponseStream();
        await foreach (var responseItem in responseStream)
        {
            fullText.Append(responseItem.Candidates[0].Content.Parts[0].Text);
        }

        return fullText.ToString();
    }
}