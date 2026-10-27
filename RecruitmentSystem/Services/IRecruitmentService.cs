using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services;

/// <summary>An application together with its job and applicant (used by the list pages).</summary>
public record ApplicationDetails(JobApplication Application, JobPosting Job, User Applicant);

public interface IRecruitmentService
{
    // Users
    User Register(User user);
    User? Authenticate(string email, string password);
    User? GetUser(int id);

    // Jobs
    IReadOnlyList<JobPosting> SearchJobs(string? keyword, string? location, JobType? type, bool activeOnly);
    JobPosting? GetJob(int id);
    JobPosting AddJob(JobPosting job);
    JobPosting UpdateJob(JobPosting job);
    void ToggleJob(int id);
    void DeleteJob(int id);

    // Applications
    JobApplication Apply(int jobId, int userId, string coverLetter, string? resumeUrl, int yearsExperience, string? source);
    bool HasApplied(int userId, int jobId);
    void Withdraw(int applicationId, int userId);
    void UpdateApplicationStatus(int applicationId, ApplicationStatus status, string? note);
    IReadOnlyList<ApplicationDetails> GetApplicationsForUser(int userId);
    IReadOnlyList<ApplicationDetails> GetApplications(ApplicationStatus? status = null, int? jobId = null);
}
