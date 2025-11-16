namespace ResumeAtsChecker.Services.Interfaces
{
    public interface IPdfService
    {
        Task<string> ExtractTextFromPdfAsync(IFormFile pdfFile);
    }
}
