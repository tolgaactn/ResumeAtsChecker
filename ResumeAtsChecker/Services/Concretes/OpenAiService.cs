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
                    content = "You are an ATS expert. You analyze resumes and provide JSON responses with score, summary, missingKeywords, and suggestions."
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
                        return $@"Analyze this resume against the job description for ATS (Applicant Tracking System) compatibility.

            JOB DESCRIPTION:
            {jobDescription}

            RESUME:
            {resumeText}

            Provide your analysis in the following JSON format (respond with ONLY this JSON, nothing else):

            {{
                ""score"": 85,
                ""summary"": ""Brief overall assessment in 2-3 sentences explaining the match quality"",
                ""missingKeywords"": [""keyword1"", ""keyword2"", ""keyword3""],
                ""suggestions"": [""specific suggestion 1"", ""specific suggestion 2"", ""specific suggestion 3""]
            }}

            Important:
            - score: integer from 0-100 representing ATS compatibility
            - summary: 2-3 sentence overall assessment
            - missingKeywords: list of important keywords from job description that are missing in resume
            - suggestions: list of 3-5 specific, actionable suggestions to improve the resume

            Respond with ONLY the JSON object, no other text.";
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
                    Summary = "AI analysis completed but response format was unexpected",
                    MissingKeywords = new List<string> { "Unable to extract keywords" },
                    Suggestions = new List<string> { "Please try again with a different resume or job description" },
                    ExtractedText = extractedText
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
