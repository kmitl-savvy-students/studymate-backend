using System.Text;
using Google.Api.Gax.Grpc;
using Google.Cloud.AIPlatform.V1;

namespace studymate_backend.Libraries.Helper;

public class SdmVertexAi
{
    public async Task<string> GenerateContent(
        string projectId = "savvy-studymate",
        string location = "us-central1",
        string publisher = "google",
        string model = "gemini-1.5-flash-001"
    )
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
                Temperature = 0.4f,
                TopP = 1,
                TopK = 32,
                MaxOutputTokens = 2048
            },
            Contents =
            {
                new Content
                {
                    Role = "USER",
                    Parts =
                    {
                        new Part { Text = "What's in this photo?" },
                        new Part { FileData = new FileData { MimeType = "image/png", FileUri = "gs://generativeai-downloads/images/scones.jpg" } }
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
