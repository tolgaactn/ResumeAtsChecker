namespace ResumeAtsChecker.Dtos
{
    public class AnalysisResponseDto
    {
        public int Score { get; set; }  //ATS skoru (0-100)
        public string Summary { get; set; } //Genel değerlendirme   
        public List<string> MissingKeywords { get; set; }   //Eksik kelimeler   
        public List<string> Suggestions { get; set; }  //İyileştirme önerileri
        public string ExtractedText { get; set; }   //PDF'den çıkan text (debug için)
    }
}
