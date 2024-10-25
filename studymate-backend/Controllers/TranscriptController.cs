using System.Text;
using System.Text.RegularExpressions;
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
public partial class TranscriptController(
    TranscriptService transcriptService,
    TranscriptDataService transcriptDataService,
    UserService userService,
    UserTokenService userTokenService
) : IController
{
    private class ParseData(string id, string data)
    {
        public string Id { get; } = id;
        public string Data { get; } = data;
    }

    private class GenerateContentResult(string content, int promptTokenCount, int outputTokenCount, int totalTokenCount)
    {
        public string Content { get; } = content;
        public int PromptTokenCount { get; } = promptTokenCount;
        public int OutputTokenCount { get; } = outputTokenCount;
        public int TotalTokenCount { get; } = totalTokenCount;
    }

    private struct TranscriptDataStructure(List<TransferCredit> transfer_credits, List<SemesterGrade> grades)
    {
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

    [GeneratedRegex(@"Checked by\s+[\w\s\(\)]+")]
    private static partial Regex RemoveCheckedByRegex();

    [GeneratedRegex(
        @"----------------------------- Continue next column -----------------------------|-------------------------------- Transcript Closed --------------------------------",
        RegexOptions.IgnoreCase)]
    private static partial Regex RemoveTranscriptMarkersRegex();

    [GeneratedRegex(@"GPS\s*:\s*\S+|GPA\s*:\s*\S+", RegexOptions.IgnoreCase)]
    private static partial Regex RemoveGpsGpaRegex();

    [GeneratedRegex(@"\b\d{8}\b")]
    private static partial Regex ExtractStudentIdRegex();

    [GeneratedRegex(@"Total number of credit earned: \d+ Cumulative")]
    private static partial Regex RemoveCreditCumulativeRegex();

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<BaseResponse> Upload([FromForm] RequestTranscriptUpload requestTranscriptUpload)
    {
        var file = requestTranscriptUpload.File;

        if (!SdmString.IsValid(requestTranscriptUpload.UserTokenId, 64, 64))
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID, "Invalid User Token ID");

        var userTokenId = SdmString.cleanAndTrim(requestTranscriptUpload.UserTokenId);

        // Verify token
        var userToken = userTokenService.Get(userTokenId);
        if (userToken == null)
            return new BaseResponse(EnumResponseCode.UNAUTHORIZED, "ไม่อนุญาตให้อัปโหลด");

        // Verify Files
        if (file == null || file.Length == 0)
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID, "ไม่มีไฟล์");

        const long maxFileSize = 15 * 1024 * 1024;
        if (file.Length > maxFileSize)
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID, "ขนาดไฟล์ใหญ่เกิน 15MB");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf")
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID, "ระบบตรวจพบว่านี่ไม่ใช่ไฟล์นามสกุล .pdf");

        if (file.ContentType != "application/pdf")
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID, "ระบบตรวจพบว่านี่ไม่ใช่ไฟล์ PDF");

        if (!await IsValidPdf(file))
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID, "ระบบตรวจพบว่านี่ไม่ใช่ไฟล์ PDF");

        Console.WriteLine("=========== " + file.FileName + " ===========");
        Console.WriteLine("Starting extract PDF using PDFPig...");

        ParseData data;
        try
        {
            data = ExtractTextFromPdf(file);

            if (data.Data == "")
                return new BaseResponse(EnumResponseCode.FIELDS_INVALID, "ไม่พบข้อมูลใน PDF");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing PDF file: {ex.Message}");
            return new BaseResponse(EnumResponseCode.INTERNAL_SERVER_ERROR, "เกิดปัญหาระหว่างประมวลผล PDF");
        }

        Console.WriteLine("==== START PDFPig Transcript result ====");
        Console.WriteLine(data.Data);
        Console.WriteLine("==== END PDFPig Transcript result ====");

        Console.WriteLine("Checking Student ID...");

        if (data.Id == "")
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID, "ระบบพบว่านี่ไม่ใช่ Transcript");

        // Check for user in transcript
        var user = userService.Get(data.Id);
        if (user == null)
            return new BaseResponse(EnumResponseCode.NOT_FOUND, "ระบบพบว่านี่ไม่ใช่ Transcript ของคุณ");
        
        // Check if user matches
        if (user.Id != userToken.User.Id)
            return new BaseResponse(EnumResponseCode.UNAUTHORIZED, "ระบบพบว่านี่ไม่ใช่ Transcript ของคุณ");

        Console.WriteLine("Start extract data using AI...");

        // AI prompt to extract data from the PDF content
        const string projectId = "savvy-studymate";
        const string location = "us-central1";
        const string publisher = "google";
        const string model = "gemini-1.5-flash-002";
        
        var prompt = """
                     - Example: {"transfer_credits":[{"subject_id":"str","grade":"str","credit":"int"}],"grades":[{"semester":"int","year":"int","courses":[{"subject_id":"str","grade":"str","credit":"int"}]}]}
                     - Use 0 for empty values; if no transfer_credits, use []
                     - Grade is "0" if "X" or empty
                     - Format: "courseTitle credit grade"
                     - Ignore numbers in courseTitle, credit and grade are always last
                     - Return compact JSON with no extra whitespace
                     """ + "\nTranscript:" + data.Data;


        Console.WriteLine("==== START AI Prompt result ====");
        Console.WriteLine(prompt);
        Console.WriteLine("==== END AI Prompt result ====");

        GenerateContentResult generateResult;

        try
        {
            generateResult = await GenerateContentInternal(projectId, location, publisher, model, prompt);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating content with AI: {ex.Message}");
            return new BaseResponse(EnumResponseCode.INTERNAL_SERVER_ERROR, "ระบบ Quota เต็มกรุณารอ 5 นาทีก่อนอัปโหลดอีกครั้ง");
        }

        var cleanedContent = generateResult.Content.Replace("```json", "").Replace("```", "").Trim();

        Console.WriteLine("==== START AI Transcript result ====");
        Console.WriteLine(cleanedContent);
        Console.WriteLine("==== END AI Transcript result ====");

        if (cleanedContent == "")
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID, "ผลลัพธ์ว่างเปล่า");

        Console.WriteLine("Parsing data...");

        TranscriptDataStructure transcriptData;

        try
        {
            transcriptData = JsonConvert.DeserializeObject<TranscriptDataStructure>(cleanedContent);
        }
        catch (JsonReaderException ex)
        {
            Console.WriteLine($"Error parsing JSON: {ex.Message}");
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID, "AI เอ๋อ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error: {ex.Message}");
            return new BaseResponse(EnumResponseCode.FIELDS_INVALID, "AI เอ๋อ");
        }

        Console.WriteLine("Saving data to database...");

        try
        {
            // Create a new Transcript entry
            var transcript = new Transcript(0, userToken.User.Id, 3, DateTime.UtcNow);
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
            {
                transcriptDataService.Add(transcriptDataEntry);
            }

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
            {
                transcriptDataService.Add(transcriptDataEntry);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving transcript data: {ex.Message}");
            return new BaseResponse(EnumResponseCode.INTERNAL_SERVER_ERROR, "ไม่สามารถบันทึกข้อมูลลงฐานข้อมูลได้");
        }

        const decimal textInputCostPerCountBaht = 0.18m / 291268m;
        const decimal textOutputCostPerCountBaht = 0.53m / 217365m;

        var inputCostBaht = generateResult.PromptTokenCount * textInputCostPerCountBaht;
        var outputCostBaht = generateResult.OutputTokenCount * textOutputCostPerCountBaht;

        var totalCostBaht = inputCostBaht + outputCostBaht;

        Console.WriteLine($"Tokens used - Input: {generateResult.PromptTokenCount}, Output: {generateResult.OutputTokenCount}, Total: {generateResult.TotalTokenCount}");
        Console.WriteLine($"Estimated Cost: {totalCostBaht} Baht");

        Console.WriteLine("DONE!!");
        Console.WriteLine("");

        return new BaseResponse(EnumResponseCode.OK, "Upload Success");
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
                var leftColumnBounds = new PdfRectangle(0, pageHeight * bottomIgnorePercent, pageWidth / 2, pageHeight - (pageHeight * topPercent));
                var rightColumnBounds = new PdfRectangle(pageWidth / 2, pageHeight * bottomIgnorePercent, pageWidth, pageHeight - (pageHeight * topPercent));

                var topBoxText = page.GetWords().Where(word => IsWithinBounds(word.BoundingBox, topBoxBounds)).Select(word => word.Text).ToArray();
                textTop.Append(string.Join(" ", topBoxText));

                var leftText = page.GetWords().Where(word => IsWithinBounds(word.BoundingBox, leftColumnBounds)).Select(word => word.Text).ToArray();
                textLeft.Append(string.Join(" ", leftText));

                var rightText = page.GetWords().Where(word => IsWithinBounds(word.BoundingBox, rightColumnBounds)).Select(word => word.Text).ToArray();
                textRight.Append(string.Join(" ", rightText)).Append(' ');
            }

            var resultTop = ExtractStudentIdRegex().Match(textTop.ToString()).Value;

            var resultLeft = RemoveGpsGpaRegex().Replace(textLeft.ToString(), "");
            resultLeft = RemoveTranscriptMarkersRegex().Replace(resultLeft, "");
            resultLeft = RemoveAccessSpaceRegex().Replace(resultLeft, " ");

            var resultRight = RemoveGpsGpaRegex().Replace(textRight.ToString(), "");
            resultRight = RemoveCheckedByRegex().Replace(resultRight, "");
            resultRight = RemoveTranscriptMarkersRegex().Replace(resultRight, "");
            resultRight = RemoveCreditCumulativeRegex().Replace(resultRight, "");
            resultRight = RemoveAccessSpaceRegex().Replace(resultRight, " ");

            return new ParseData(resultTop, resultLeft + resultRight);
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

    private static async Task<GenerateContentResult> GenerateContentInternal(string projectId, string location, string publisher, string model, string prompt)
    {
        try
        {
            Console.WriteLine("Building PredictionServiceClient...");
            var predictionServiceClient = await new PredictionServiceClientBuilder
            {
                Endpoint = $"{location}-aiplatform.googleapis.com"
            }.BuildAsync();

            if (predictionServiceClient == null)
            {
                Console.WriteLine("PredictionServiceClient is null.");
                throw new Exception("Failed to build PredictionServiceClient.");
            }

            Console.WriteLine("Creating GenerateContentRequest...");
            var generateContentRequest = new GenerateContentRequest
            {
                Model = $"projects/{projectId}/locations/{location}/publishers/{publisher}/models/{model}",
                GenerationConfig = new GenerationConfig
                {
                    Temperature = 0.1f,
                    TopP = 0.5f,
                    TopK = 40,
                    MaxOutputTokens = 8192
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

            Console.WriteLine("Sending request to AI service...");
            using var response = predictionServiceClient.StreamGenerateContent(generateContentRequest);

            if (response == null)
            {
                Console.WriteLine("Response is null.");
                throw new Exception("Failed to receive response from AI service.");
            }

            StringBuilder fullText = new();

            Console.WriteLine("Getting response stream...");
            var responseStream = response.GetResponseStream();

            if (responseStream == null)
            {
                Console.WriteLine("Response stream is null.");
                throw new Exception("Failed to get response stream from AI service.");
            }

            var promptTokenCount = 0;
            var outputTokenCount = 0;

            Console.WriteLine("Processing response stream...");
            await foreach (var responseItem in responseStream)
            {
                foreach (var candidate in responseItem.Candidates)
                {
                    if (candidate.Content?.Parts == null || candidate.Content.Parts.Count == 0)
                    {
                        Console.WriteLine("No content parts found in candidate.");
                        continue;
                    }

                    foreach (var part in candidate.Content.Parts)
                        fullText.Append(part.Text);
                }

                if (responseItem.UsageMetadata == null) continue;
                promptTokenCount += responseItem.UsageMetadata.PromptTokenCount;
                outputTokenCount += responseItem.UsageMetadata.CandidatesTokenCount;
            }

            Console.WriteLine("AI content generation completed.");
            return new GenerateContentResult
            (
                fullText.ToString(),
                promptTokenCount,
                outputTokenCount,
                promptTokenCount + outputTokenCount
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during AI content generation: {ex}");
            throw;
        }
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex RemoveAccessSpaceRegex();
}