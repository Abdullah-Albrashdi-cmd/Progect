namespace RecruitmentSystem.Models;

/// <summary>Entity 3 (Applications table) - a candidate's application for a job posting.</summary>
public class JobApplication
{
    public int Id { get; set; }
    public int JobId { get; }
    public int UserId { get; }
    public string CoverLetter { get; }
    public string? ResumeUrl { get; }
    public int YearsExperience { get; }
    public string? Source { get; }
    public DateTime AppliedDate { get; } = DateTime.Now;
    public ApplicationStatus Status { get; private set; } = ApplicationStatus.Pending;
    public string? ReviewerNote { get; private set; }
    public DateTime? LastUpdated { get; private set; }

    public JobApplication(int jobId, int userId, string coverLetter, string? resumeUrl, int yearsExperience, string? source)
    {
        JobId = jobId;
        UserId = userId;
        CoverLetter = coverLetter.Trim();
        ResumeUrl = string.IsNullOrWhiteSpace(resumeUrl) ? null : resumeUrl.Trim();
        YearsExperience = yearsExperience;
        Source = string.IsNullOrWhiteSpace(source) ? null : source;
    }

    /// <summary>Accepted / Rejected are final decisions.</summary>
    public bool IsFinal() => Status is ApplicationStatus.Accepted or ApplicationStatus.Rejected;

    /// <summary>Candidates may only withdraw while the application is still untouched.</summary>
    public bool CanWithdraw() => Status == ApplicationStatus.Pending;

    public void UpdateStatus(ApplicationStatus newStatus, string? note)
    {
        if (IsFinal() && newStatus != Status)
            throw new BusinessRuleException(
                $"This application is already {Status.GetDisplayName().ToLower()} and can no longer be changed.");

        Status = newStatus;
        ReviewerNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        LastUpdated = DateTime.Now;
    }
}
