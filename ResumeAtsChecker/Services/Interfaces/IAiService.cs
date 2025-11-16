using ResumeAtsChecker.Dtos;

namespace ResumeAtsChecker.Services.Interfaces
{
    public interface IAiService
    {
        Task<AnalysisResponseDto> AnalyzeResumeAsync(string resumeText, string jobDescription);
    }
}
