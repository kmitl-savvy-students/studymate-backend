using System.Text;
using Google.Api.Gax.Grpc;
using Google.Cloud.AIPlatform.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Controllers.Core;
using studymate_backend.Enums;
using studymate_backend.Helper;
using studymate_backend.Models.Core;
using studymate_backend.Models.StudyMate.Raw.Request.Transcript;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Geometry;

namespace studymate_backend.Controllers
{
    [ApiController]
    [Route("api/transcript")]
    public class TranscriptController : IController
    {
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<BaseResponse> Upload([FromForm] RequestTranscriptUpload requestTranscriptUpload)
        {
            var file = requestTranscriptUpload.File;
            
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

            try
            {
                Console.WriteLine("----- Uploaded Transcript Details -----");
                Console.WriteLine($"File Name: {file.FileName}");
                Console.WriteLine($"File Size: {file.Length} bytes");
                Console.WriteLine($"Content Type: {file.ContentType}");
                Console.WriteLine("----- End of Details -----");

                const string projectId = "savvy-studymate";
                const string location = "us-central1";
                const string publisher = "google";
                const string model = "gemini-1.5-flash-001";

                var fileContent = ExtractTextFromPdf(file);

                if (string.IsNullOrEmpty(fileContent))
                {
                    Console.WriteLine("Error: Unable to extract text from the PDF.");
                    return new BaseResponse(EnumResponseCode.FIELDS_INVALID);
                }
                
                Console.WriteLine(fileContent);

                var prompt = "Extract data as the following JSON structure: { \"student_id\": \"\", \"transfer_credits\": { // If it exists [ \"subject_id\": \"\", \"grade\": \"\", // A, B, C, D ] }, \"grades\": [ { \"semester\": \"\", // only number 1, 2, 3 \"year\": \"\", // start year only \"courses\": { [ \"subject_id\": \"\", \"grade\": \"\" // A, B, C, D ] } } ] }: " + fileContent;

                var content = await GenerateContentInternal(projectId, location, publisher, model, prompt);

                Console.WriteLine("----- Generated Content -----");
                Console.WriteLine(content);
                Console.WriteLine("----- End of Generated Content -----");

                return new BaseResponse(EnumResponseCode.OK);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                return new BaseResponse(EnumResponseCode.INTERNAL_SERVER_ERROR);
            }
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
                    text.Append(string.Join(" ", leftText)).Append(" ");

                    var rightText = page.GetWords().Where(word => IsWithinBounds(word.BoundingBox, rightColumnBounds)).Select(word => word.Text).ToArray();
                    text.Append(string.Join(" ", rightText)).Append(" ");
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

        private static async Task<string> GenerateContentInternal(
            string projectId, string location, string publisher, string model, string prompt)
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
}
