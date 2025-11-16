namespace ResumeAtsChecker.Dtos
{
    public class AnalysisRequestDto
    {
        public IFormFile Resume { get; set; }  //pdf dosyası
        public string JobDescription { get; set; }  //iş ilanı metni
    }
}
