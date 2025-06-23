using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/ask", async (HttpRequest request) =>
{
    // Get the OpenAI API key from environment variables
    var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

if (string.IsNullOrEmpty(openAiKey))
{
    return Results.Problem("Missing OpenAI API key.");
}


    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    Dictionary<string, string>? data;
    try
    {
        data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
    }
    catch
    {
        return Results.Problem("Invalid JSON payload.");
    }

    var question = data?["question"] ?? "No question provided.";

    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiKey);

    var payload = new
    {
        model = "gpt-4o",
        messages = new[]
        {
            new { role = "system", content = "You are a helpful legal assistant. Be clear, but do not give real legal advice." },
            new { role = "user", content = question }
        }
    };

    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
    var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
    var responseBody = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem($"OpenAI error: {response.StatusCode}\n{responseBody}");
    }

    try
    {
        using var doc = JsonDocument.Parse(responseBody);
        var reply = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return Results.Json(new { reply });
    }
    catch
    {
        return Results.Problem("Failed to parse OpenAI response.");
    }
});

// Optional root endpoint for testing
app.MapGet("/", () => "Welcome to AskMyLawBotAPI!");

app.Run();
