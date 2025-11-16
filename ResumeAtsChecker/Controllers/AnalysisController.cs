using Microsoft.AspNetCore.Mvc;
using ResumeAtsChecker.Data;
using ResumeAtsChecker.Dtos;
using ResumeAtsChecker.Services;
using ResumeAtsChecker.Services.Interfaces;
using System.Text.Json;

namespace ResumeAtsChecker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IPdfService _pdfService;
        private readonly IAiService _aiService;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AnalysisController> _logger;

        // Constructor - DI
        public AnalysisController(
            IPdfService pdfService,
            IAiService aiService,
            AppDbContext dbContext,
            ILogger<AnalysisController> logger)
        {
            _pdfService = pdfService;
            _aiService = aiService;
            _dbContext = dbContext;
            _logger = logger;
        }

        // 🎯 ANA ENDPOINT: Tam Analiz (PDF + Job Description → AI → Database)
        [HttpPost("analyze")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Analyze([FromForm] AnalysisRequestDto request)
        {
            try
            {
                // 1. Validasyon
                if (request.Resume == null || request.Resume.Length == 0)
                    return BadRequest(new { error = "PDF file is required" });

                if (string.IsNullOrWhiteSpace(request.JobDescription))
                    return BadRequest(new { error = "Job description is required" });

                // 2. PDF'den text çıkar
                _logger.LogInformation("Extracting text from PDF...");
                var extractedText = await _pdfService.ExtractTextFromPdfAsync(request.Resume);

                if (string.IsNullOrWhiteSpace(extractedText))
                    return BadRequest(new { error = "Could not extract text from PDF. Please ensure it's a valid PDF with text content." });

                // 3. AI analizi yap
                _logger.LogInformation("Analyzing with AI...");
                var analysisResult = await _aiService.AnalyzeResumeAsync(extractedText, request.JobDescription);

                // 4. Database'e kaydet
                _logger.LogInformation("Saving to database...");
                var analysis = new Analysis
                {
                    UserId = "guest", // Şimdilik guest (ileride auth ekleyeceğiz)
                    ExtractedText = extractedText,
                    JobDescription = request.JobDescription,
                    Score = analysisResult.Score,
                    Summary = analysisResult.Summary,
                    MissingKeywords = JsonSerializer.Serialize(analysisResult.MissingKeywords),
                    Suggestions = JsonSerializer.Serialize(analysisResult.Suggestions),
                    IsPremium = false,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Analyses.Add(analysis);
                await _dbContext.SaveChangesAsync();

                // 5. Response dön
                return Ok(new
                {
                    success = true,
                    analysisId = analysis.Id,
                    score = analysisResult.Score,
                    summary = analysisResult.Summary,
                    missingKeywords = analysisResult.MissingKeywords,
                    suggestions = analysisResult.Suggestions,
                    message = "Analysis completed successfully!"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during analysis");
                return StatusCode(500, new { error = "An error occurred during analysis", details = ex.Message });
            }
        }

        // 📄 YARDIMCI ENDPOINT: Sadece PDF Text Çıkar (test için)
        [HttpPost("extract-text")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ExtractText(IFormFile resume)
        {
            try
            {
                if (resume == null || resume.Length == 0)
                    return BadRequest(new { error = "PDF file is required" });

                var extractedText = await _pdfService.ExtractTextFromPdfAsync(resume);

                return Ok(new
                {
                    success = true,
                    text = extractedText,
                    fileName = resume.FileName,
                    fileSize = resume.Length
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while processing the PDF", details = ex.Message });
            }
        }

        // 🗄️ TEST ENDPOINT: Database test (opsiyonel - silebilirsin)
        [HttpPost("test-db")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var analysis = new Analysis
                {
                    UserId = "test-user-123",
                    ExtractedText = "Sample CV text",
                    JobDescription = "Sample job description",
                    Score = 85,
                    Summary = "Good match",
                    MissingKeywords = "[\"Python\", \"Docker\"]",
                    Suggestions = "[\"Add more technical skills\"]",
                    IsPremium = false
                };

                _dbContext.Analyses.Add(analysis);
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Analysis saved to database!",
                    analysisId = analysis.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Database error", details = ex.Message });
            }
        }

        // 📊 GET ENDPOINT: Analiz sonucunu ID ile getir
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnalysis(int id)
        {
            try
            {
                var analysis = await _dbContext.Analyses.FindAsync(id);

                if (analysis == null)
                    return NotFound(new { error = "Analysis not found" });

                return Ok(new
                {
                    id = analysis.Id,
                    score = analysis.Score,
                    summary = analysis.Summary,
                    missingKeywords = JsonSerializer.Deserialize<List<string>>(analysis.MissingKeywords ?? "[]"),
                    suggestions = JsonSerializer.Deserialize<List<string>>(analysis.Suggestions ?? "[]"),
                    createdAt = analysis.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analysis");
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }
    }
}