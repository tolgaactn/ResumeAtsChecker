using System.ComponentModel.DataAnnotations;

namespace ResumeAtsChecker.Data
{
    public class Analysis
    {
        [Key]
        public int Id { get; set; } 
        [Required]
        public string UserId { get; set; } //Şimdilik boş ileride auth ekleyeceğiz
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ExtractedText { get; set; }  //PDF'den çıkarılan metin
        public string? JobDescription { get; set; }   //İş ilanı metni
        public int Score { get; set; }  //ATS skoru (0-100)
        public string? Summary { get; set; } // AI'dan gelen özet
        public string? MissingKeywords { get; set; }   //JSON array olarak saklanacak
        public string? Suggestions { get; set; }  //JSON array olarak saklanacak
        public bool IsPremium { get; set; } = false; //Free mi Pro mu?

    }
}
