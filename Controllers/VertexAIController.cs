using System.Text;
using Google.Api.Gax.Grpc;
using Google.Cloud.AIPlatform.V1;
using Microsoft.AspNetCore.Mvc;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VertexAIController : ControllerBase
{
    // This method generates content by calling Vertex AI's API
    [HttpGet("generate-content")]
    public async Task<IActionResult> GenerateContent()
    {
        // Default parameters for Vertex AI content generation
        const string projectId = "savvy-studymate";
        const string location = "us-central1";
        const string publisher = "google";
        const string model = "gemini-1.5-flash-001";

        // Generate content from the Vertex AI API
        var content = await GenerateContentInternal(projectId, location, publisher, model);

        // Return the generated content as a JSON response
        return Ok(new { content });
    }

    // The private method that makes the call to the Vertex AI API
    private async Task<string> GenerateContentInternal(
        string projectId, string location, string publisher, string model)
    {
        // Create client
        var predictionServiceClient = await new PredictionServiceClientBuilder
        {
            Endpoint = $"{location}-aiplatform.googleapis.com"
        }.BuildAsync();

        // Initialize content request
        var generateContentRequest = new GenerateContentRequest
        {
            Model = $"projects/{projectId}/locations/{location}/publishers/{publisher}/models/{model}",
            GenerationConfig = new GenerationConfig
            {
                Temperature = 0.2f,
                TopP = 0.5f,
                TopK = 40,
                MaxOutputTokens = 512
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

        // Make the request, returning a streaming response
        using var response = predictionServiceClient.StreamGenerateContent(generateContentRequest);

        StringBuilder fullText = new();

        // Read streaming responses from server until complete
        AsyncResponseStream<GenerateContentResponse> responseStream = response.GetResponseStream();
        await foreach (var responseItem in responseStream) fullText.Append(responseItem.Candidates[0].Content.Parts[0].Text);

        return fullText.ToString();
    }
}