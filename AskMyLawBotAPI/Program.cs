using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/ask", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

    var question = data?["question"] ?? "No question provided.";
    var openAiKey = "YOUR_OPENAI_API_KEY"; // Replace this

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

    using var doc = JsonDocument.Parse(responseBody);
    var reply = doc.RootElement
        .GetProperty("choices")[0]
        .GetProperty("message")
        .GetProperty("content")
        .GetString();

    return Results.Json(new { reply });
});

app.Run();
