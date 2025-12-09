using Microsoft.AspNetCore.Http.HttpResults;
using ResumeAtsChecker.Dtos;
using ResumeAtsChecker.Services.Interfaces;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ResumeAtsChecker.Services
{
    public class OpenAiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAiService> _logger;

        public OpenAiService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // OpenAI API base URL
            _httpClient.BaseAddress = new Uri("https://api.openai.com/");

            // Headers
            var apiKey = _configuration["OpenAI:ApiKey"];
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public async Task<AnalysisResponseDto> AnalyzeResumeAsync(string resumeText, string jobDescription)
        {
            try
            {
                var prompt = BuildPrompt(resumeText, jobDescription);

                var requestBody = new
                {
                    model = _configuration["OpenAI:Model"],
                    messages = new[]
                    {
                new
                {
                    role = "system",
                    content = "Sen bir ATS (Başvuru Takip Sistemi) uzmanısın. CV'leri analiz eder ve TÜRKÇE yanıtlar verirsin. JSON formatında score, summary, missingKeywords ve suggestions döndürürsün. Tüm açıklamalar, özetler ve öneriler TÜRKÇE olmalıdır."
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            },
                    max_tokens = int.Parse(_configuration["OpenAI:MaxTokens"] ?? "2000"),
                    temperature = 0.7
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("v1/chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("OpenAI API Error: Status={Status}, Response={Response}",
                        response.StatusCode, errorContent);
                    throw new Exception($"OpenAI API returned {response.StatusCode}: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Raw API Response: {Response}", responseContent);

                var deserializeOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var apiResponse = JsonSerializer.Deserialize<OpenAiApiResponse>(responseContent, deserializeOptions);

                var aiText = apiResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "";

                _logger.LogInformation("AI Response: {Response}", aiText);

                return ParseAiResponse(aiText, resumeText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API");
                throw;
            }
        }

        private string BuildPrompt(string resumeText, string jobDescription)
        {
            return $@"Bu CV'yi iş ilanına göre ATS (Başvuru Takip Sistemi) uyumluluğu açısından analiz et.

İŞ İLANI:
{jobDescription}

CV:
{resumeText}

Analizini aşağıdaki JSON formatında ver (SADECE bu JSON'u döndür, başka hiçbir şey yazma):

{{
    ""score"": 85,
    ""summary"": ""2-3 cümle ile genel değerlendirme ve eşleşme kalitesi açıklaması (TÜRKÇE)"",
    ""missingKeywords"": [""anahtar kelime1"", ""anahtar kelime2"", ""anahtar kelime3""],
    ""suggestions"": [""öneri 1"", ""öneri 2"", ""öneri 3""]
}}

Önemli:
- score: 0-100 arası ATS uyumluluk skoru
- summary: 2-3 cümle genel değerlendirme (TÜRKÇE)
- missingKeywords: İş ilanında olup CV'de eksik olan önemli anahtar kelimeler
- suggestions: CV'yi geliştirmek için 3-5 spesifik, uygulanabilir öneri (TÜRKÇE)

SADECE JSON objesini döndür, başka metin yazma. TÜM AÇIKLAMALAR TÜRKÇE OLMALIDIR.";
        }
private AnalysisResponseDto ParseAiResponse(string aiResponse, string extractedText)
        {
            try
            {
                // JSON temizle (markdown code blocks varsa)
                var cleanedResponse = aiResponse.Trim();

                // ```json ve ``` varsa temizle
                if (cleanedResponse.StartsWith("```json"))
                {
                    cleanedResponse = cleanedResponse.Substring(7); // "```json\n" kaldır
                }
                if (cleanedResponse.StartsWith("```"))
                {
                    cleanedResponse = cleanedResponse.Substring(3); // "```" kaldır
                }
                if (cleanedResponse.EndsWith("```"))
                {
                    cleanedResponse = cleanedResponse.Substring(0, cleanedResponse.Length - 3);
                }

                cleanedResponse = cleanedResponse.Trim();

                _logger.LogInformation("Cleaned JSON: {Json}", cleanedResponse);

                // JSON parse et
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var jsonResponse = JsonSerializer.Deserialize<AiJsonResponse>(cleanedResponse, options);

                if (jsonResponse == null)
                {
                    _logger.LogWarning("Deserialized JSON is null");
                    throw new Exception("Failed to parse AI response");
                }

                return new AnalysisResponseDto
                {
                    Score = jsonResponse.Score,
                    Summary = jsonResponse.Summary ?? "Analysis completed",
                    MissingKeywords = jsonResponse.MissingKeywords ?? new List<string>(),
                    Suggestions = jsonResponse.Suggestions ?? new List<string>(),
                    ExtractedText = extractedText
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing AI response. Raw response: {Response}", aiResponse);

                // Fallback: JSON parse başarısız olursa
                return new AnalysisResponseDto
                {
                    Score = 50,
                    Summary = "AI analizi tamamlandı ancak yanıt formatı beklenmedik",
                    MissingKeywords = new List<string> { "Anahtar kelimeler çıkarılamadı" },
                    Suggestions = new List<string> { "Lütfen farklı bir CV veya iş ilanı ile tekrar deneyin" },
                };
            }
        }
    }

    // OpenAI API Response models
    public class OpenAiApiResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public Message? Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    public class AiJsonResponse
    {
        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("missingKeywords")]
        public List<string>? MissingKeywords { get; set; }

        [JsonPropertyName("suggestions")]
        public List<string>? Suggestions { get; set; }
    }
}
