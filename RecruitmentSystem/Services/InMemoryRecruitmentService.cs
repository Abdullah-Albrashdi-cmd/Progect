using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services;

/// <summary>
/// Keeps the project data as collections of the three C# classes (User, JobPosting, JobApplication)
/// exactly as Requirement 7 asks. Registered as a singleton, so data lives while the app is running.
/// </summary>
public class InMemoryRecruitmentService : IRecruitmentService
{
    private readonly object _lock = new();
    private readonly List<User> _users = new();
    private readonly List<JobPosting> _jobs = new();
    private readonly List<JobApplication> _applications = new();
    private int _userSeq, _jobSeq, _appSeq;

    public InMemoryRecruitmentService() => Seed();

    // ===================== Users =====================
    public User Register(User user)
    {
        lock (_lock)
        {
            if (_users.Any(u => u.Email == user.Email))
                throw new BusinessRuleException("An account with this email address already exists.");

            user.Id = ++_userSeq;
            _users.Add(user);
            return user;
        }
    }

    public User? Authenticate(string email, string password)
    {
        lock (_lock)
        {
            var user = _users.FirstOrDefault(u => u.Email == email.Trim().ToLowerInvariant());
            return user != null && user.VerifyPassword(password) ? user : null;
        }
    }

    public User? GetUser(int id)
    {
        lock (_lock) return _users.FirstOrDefault(u => u.Id == id);
    }

    // ===================== Jobs =====================
    public IReadOnlyList<JobPosting> SearchJobs(string? keyword, string? location, JobType? type, bool activeOnly)
    {
        lock (_lock)
        {
            IEnumerable<JobPosting> query = _jobs;

            if (activeOnly) query = query.Where(j => j.IsActive());

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();
                query = query.Where(j =>
                    j.Title.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    j.Company.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    j.Description.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    j.Requirements.Contains(k, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(j => j.Location.Contains(location.Trim(), StringComparison.OrdinalIgnoreCase));

            if (type.HasValue) query = query.Where(j => j.Type == type.Value);

            return query.OrderByDescending(j => j.PostedDate).ToList();
        }
    }

    public JobPosting? GetJob(int id)
    {
        lock (_lock) return _jobs.FirstOrDefault(j => j.Id == id);
    }

    public JobPosting AddJob(JobPosting job)
    {
        lock (_lock)
        {
            var created = new JobPosting(job.Title.Trim(), job.Company.Trim(), job.Location.Trim(), job.Type,
                job.Description.Trim(), job.Requirements.Trim(), job.SalaryMin, job.SalaryMax, job.Deadline)
            {
                Id = ++_jobSeq,
                PostedDate = DateTime.Now,
                IsOpen = true
            };
            _jobs.Add(created);
            return created;
        }
    }

    public JobPosting UpdateJob(JobPosting job)
    {
        lock (_lock)
        {
            var existing = _jobs.FirstOrDefault(j => j.Id == job.Id)
                ?? throw new BusinessRuleException("This job posting no longer exists.");
            existing.UpdateDetails(job);
            return existing;
        }
    }

    public void ToggleJob(int id)
    {
        lock (_lock)
        {
            var job = _jobs.FirstOrDefault(j => j.Id == id)
                ?? throw new BusinessRuleException("This job posting no longer exists.");
            if (job.IsOpen) job.Close(); else job.Reopen();
        }
    }

    public void DeleteJob(int id)
    {
        lock (_lock)
        {
            var job = _jobs.FirstOrDefault(j => j.Id == id)
                ?? throw new BusinessRuleException("This job posting no longer exists.");
            _applications.RemoveAll(a => a.JobId == id);
            _jobs.Remove(job);
        }
    }

    // ===================== Applications =====================
    public JobApplication Apply(int jobId, int userId, string coverLetter, string? resumeUrl, int yearsExperience, string? source)
    {
        lock (_lock)
        {
            var job = _jobs.FirstOrDefault(j => j.Id == jobId)
                ?? throw new BusinessRuleException("This job posting no longer exists.");

            if (!job.IsActive())
                throw new BusinessRuleException("This position is no longer accepting applications.");

            if (!_users.Any(u => u.Id == userId))
                throw new BusinessRuleException("Your account could not be found. Please log in again.");

            if (_applications.Any(a => a.JobId == jobId && a.UserId == userId))
                throw new BusinessRuleException("You have already applied for this position.");

            var application = new JobApplication(jobId, userId, coverLetter, resumeUrl, yearsExperience, source)
            {
                Id = ++_appSeq
            };
            _applications.Add(application);
            return application;
        }
    }

    public bool HasApplied(int userId, int jobId)
    {
        lock (_lock) return _applications.Any(a => a.UserId == userId && a.JobId == jobId);
    }

    public void Withdraw(int applicationId, int userId)
    {
        lock (_lock)
        {
            var app = _applications.FirstOrDefault(a => a.Id == applicationId && a.UserId == userId)
                ?? throw new BusinessRuleException("Application not found.");

            if (!app.CanWithdraw())
                throw new BusinessRuleException("Only pending applications can be withdrawn.");

            _applications.Remove(app);
        }
    }

    public void UpdateApplicationStatus(int applicationId, ApplicationStatus status, string? note)
    {
        if (!Enum.IsDefined(status))
            throw new BusinessRuleException("Invalid application status.");
        if (note is { Length: > 300 })
            throw new BusinessRuleException("The note cannot exceed 300 characters.");

        lock (_lock)
        {
            var app = _applications.FirstOrDefault(a => a.Id == applicationId)
                ?? throw new BusinessRuleException("Application not found.");
            app.UpdateStatus(status, note);
        }
    }

    public IReadOnlyList<ApplicationDetails> GetApplicationsForUser(int userId)
    {
        lock (_lock) return Build(_applications.Where(a => a.UserId == userId));
    }

    public IReadOnlyList<ApplicationDetails> GetApplications(ApplicationStatus? status = null, int? jobId = null)
    {
        lock (_lock)
        {
            IEnumerable<JobApplication> query = _applications;
            if (status.HasValue) query = query.Where(a => a.Status == status.Value);
            if (jobId.HasValue) query = query.Where(a => a.JobId == jobId.Value);
            return Build(query);
        }
    }

    private IReadOnlyList<ApplicationDetails> Build(IEnumerable<JobApplication> applications) =>
        applications
            .Select(a => new
            {
                App = a,
                Job = _jobs.FirstOrDefault(j => j.Id == a.JobId),
                Applicant = _users.FirstOrDefault(u => u.Id == a.UserId)
            })
            .Where(x => x.Job != null && x.Applicant != null)
            .Select(x => new ApplicationDetails(x.App, x.Job!, x.Applicant!))
            .OrderByDescending(d => d.Application.AppliedDate)
            .ToList();

    // ===================== Demo data =====================
    private void Seed()
    {
        Register(new User("HR Administrator", "admin@hr.com", "96890000001", "Admin@123", UserRole.Admin));
        var candidate = Register(new User("Demo Candidate", "candidate@demo.com", "96890000002", "Candidate@123"));

        var today = DateTime.Today;
        AddJob(new JobPosting("Junior .NET Developer", "Omani Tech Solutions", "Muscat", JobType.FullTime,
            "Build and maintain ASP.NET Core web applications with a friendly engineering team. You will work with Razor Pages, C# and SQL databases.",
            "B.Sc. in Computer Science or similar. Knowledge of C#, SQL and HTML/CSS.", 450, 650, today.AddDays(30)));
        AddJob(new JobPosting("Network Support Intern", "Gulf Connect", "Sohar", JobType.Internship,
            "Assist the network operations team with monitoring, troubleshooting and documentation of day-to-day incidents.",
            "Final-year IT student. Basic networking knowledge (TCP/IP, routing).", 150, 250, today.AddDays(21)));
        AddJob(new JobPosting("Data Analyst", "Al Noor Insights", "Muscat", JobType.FullTime,
            "Collect, clean and analyse business data and present findings to management using dashboards and reports.",
            "Degree in statistics, IT or business. Excel and SQL required, Python is a plus.", 500, 800, today.AddDays(45)));
        AddJob(new JobPosting("UI/UX Designer", "Pixel Oman", "Remote", JobType.Remote,
            "Design clean, responsive interfaces for web and mobile products and run small usability tests with users.",
            "Portfolio of UI work. Experience with Figma and basic HTML/CSS.", 400, 700, today.AddDays(25)));
        AddJob(new JobPosting("HR Assistant", "Salalah Hospitality Group", "Salalah", JobType.PartTime,
            "Support recruitment coordination, schedule interviews and keep employee records accurate and up to date.",
            "Good communication in Arabic and English. Familiar with Microsoft Office.", 200, 300, today.AddDays(14)));
        AddJob(new JobPosting("Cybersecurity Analyst", "SecureNet Oman", "Muscat", JobType.Contract,
            "Monitor security alerts, run vulnerability scans and help write incident reports for enterprise clients.",
            "Security fundamentals, Linux basics. Certifications such as Security+ are an advantage.", 700, 1100, today.AddDays(35)));

        Apply(1, candidate.Id, "I am a final-year Computer Science student with hands-on experience in C# and ASP.NET Core projects.",
            "https://github.com/demo-candidate", 1, "University career fair");
        Apply(3, candidate.Id, "I enjoy working with data and have completed several SQL and Excel analysis projects during my studies.",
            null, 0, "Online job board");
        UpdateApplicationStatus(2, ApplicationStatus.UnderReview, "Your profile is being reviewed by the hiring team.");
    }
}
