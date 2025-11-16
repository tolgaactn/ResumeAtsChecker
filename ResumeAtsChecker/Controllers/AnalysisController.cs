using Microsoft.AspNetCore.Mvc;
using ResumeAtsChecker.Data;
using ResumeAtsChecker.Services;

namespace ResumeAtsChecker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IPdfService _pdfService;
        private readonly AppDbContext _dbContext;

        public AnalysisController(IPdfService pdfService, AppDbContext dbContext)
        {
            _pdfService = pdfService;
            _dbContext = dbContext;
        }

        // POST: api/analysis/extract-text
        [HttpPost("extract-text")]
        [Consumes("multipart/form-data")]  // 👈 Bunu ekledik
        public async Task<IActionResult> ExtractText(IFormFile resume)  // 👈 [FromForm] kaldırdık
        {
            try
            {
                // Validation
                if (resume == null || resume.Length == 0)
                    return BadRequest(new { error = "PDF file is required" });

                // PDF'den text çıkar
                var extractedText = await _pdfService.ExtractTextFromPdfAsync(resume);

                // Response dön
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

        [HttpPost("test-db")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var analysis = new Analysis
                {
                    UserId = "test-user-123",
                    ExtractedText = "This is a test extracted text.",
                    JobDescription = "This is a test job description.",
                    Score = 85,
                    Summary = "This is a test summary.",
                    MissingKeywords = "[\"keyword1\", \"keyword2\"]",
                    Suggestions = "[\"suggestion1\", \"suggestion2\"]",
                    IsPremium = false

                };
                _dbContext.Analyses.Add(analysis);
                await _dbContext.SaveChangesAsync();

                return Ok(new { success = true, analysisId = analysis.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while testing the database", details = ex.Message });
            }
        }
    }
}