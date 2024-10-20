using System.Text;
using Google.Api.Gax.Grpc;
using Google.Cloud.AIPlatform.V1;
using Microsoft.AspNetCore.Mvc;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VertexAIController : ControllerBase
{
    [HttpGet("generate-content")]
    public async Task<IActionResult> GenerateContent()
    {
        const string projectId = "savvy-studymate";
        const string location = "us-central1";
        const string publisher = "google";
        const string model = "gemini-1.5-flash-001";

        var content = await GenerateContentInternal(projectId, location, publisher, model);

        return Ok(new { content });
    }

    private static async Task<string> GenerateContentInternal(
        string projectId, string location, string publisher, string model)
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
                Temperature = 0.2f,
                TopP = 0.5f,
                TopK = 40,
                MaxOutputTokens = 2048
            },
            Contents =
            {
                new Content
                {
                    Role = "USER",
                    Parts =
                    {
                        new Part { Text = "Test API: Please respond with OK" }
                    }
                }
            }
        };

        using var response = predictionServiceClient.StreamGenerateContent(generateContentRequest);

        StringBuilder fullText = new();

        AsyncResponseStream<GenerateContentResponse> responseStream = response.GetResponseStream();
        await foreach (var responseItem in responseStream) fullText.Append(responseItem.Candidates[0].Content.Parts[0].Text);

        return fullText.ToString();
    }
}