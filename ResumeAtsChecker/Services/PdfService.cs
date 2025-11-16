
using System.Text;
using UglyToad.PdfPig;

namespace ResumeAtsChecker.Services
{
    public class PdfService : IPdfService
    {
        public Task<string> ExtractTextFromPdfAsync(IFormFile pdfFile)
        {
            if(pdfFile ==null || pdfFile.Length == 0)
            {
                throw new ArgumentException("PDF file is null or empty.");
            }

            if(pdfFile.Length >5 * 1024 * 1024)
            {
                throw new ArgumentException("PDF file size exceeds the 5MB limit.");
            }

            var extractedText = new StringBuilder();

            using (var stream = pdfFile.OpenReadStream())
            {
                using (var document = PdfDocument.Open(stream))
                {
                    foreach (var page in document.GetPages())
                    {
                        var text = page.Text;
                        extractedText.AppendLine(text);
                    }
                }
            }
            return Task.FromResult(extractedText.ToString());
        }
    }
}
