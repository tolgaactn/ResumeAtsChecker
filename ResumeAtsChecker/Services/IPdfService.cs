namespace ResumeAtsChecker.Services
{
    public interface IPdfService
    {
        Task<string> ExtractTextFromPdfAsync(IFormFile pdfFile);
    }
}
