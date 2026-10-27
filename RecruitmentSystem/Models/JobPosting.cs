using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.Models;

/// <summary>Entity 2 (JobPostings table). Also used directly as the bound model of the admin job form,
/// so the validation attributes below drive Requirement 14.</summary>
public class JobPosting : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Job title is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
    [Display(Name = "Job title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Company name is required.")]
    [StringLength(80, ErrorMessage = "Company name cannot exceed 80 characters.")]
    public string Company { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(60, ErrorMessage = "Location cannot exceed 60 characters.")]
    public string Location { get; set; } = string.Empty;

    [Display(Name = "Job type")]
    public JobType Type { get; set; } = JobType.FullTime;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, MinimumLength = 20, ErrorMessage = "Description must be between 20 and 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Requirements are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Requirements must be between 10 and 1000 characters.")]
    public string Requirements { get; set; } = string.Empty;

    [Range(100, 20000, ErrorMessage = "Minimum salary must be between 100 and 20,000 OMR.")]
    [Display(Name = "Min salary (OMR)")]
    public int SalaryMin { get; set; } = 400;

    [Range(100, 20000, ErrorMessage = "Maximum salary must be between 100 and 20,000 OMR.")]
    [Display(Name = "Max salary (OMR)")]
    public int SalaryMax { get; set; } = 700;

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Application deadline")]
    public DateTime Deadline { get; set; } = DateTime.Today.AddDays(30);

    public DateTime PostedDate { get; set; } = DateTime.Now;
    public bool IsOpen { get; set; } = true;

    public JobPosting() { }

    public JobPosting(string title, string company, string location, JobType type, string description,
                      string requirements, int salaryMin, int salaryMax, DateTime deadline)
    {
        Title = title; Company = company; Location = location; Type = type;
        Description = description; Requirements = requirements;
        SalaryMin = salaryMin; SalaryMax = salaryMax; Deadline = deadline;
    }

    // ---------- business logic ----------
    public bool IsActive() => IsOpen && Deadline.Date >= DateTime.Today;

    public string GetState() => !IsOpen ? "Closed" : Deadline.Date < DateTime.Today ? "Expired" : "Open";

    public string GetSalaryRange() => $"{SalaryMin:N0} - {SalaryMax:N0} OMR";

    public void Close() => IsOpen = false;
    public void Reopen() => IsOpen = true;

    public void UpdateDetails(JobPosting source)
    {
        Title = source.Title.Trim();
        Company = source.Company.Trim();
        Location = source.Location.Trim();
        Type = source.Type;
        Description = source.Description.Trim();
        Requirements = source.Requirements.Trim();
        SalaryMin = source.SalaryMin;
        SalaryMax = source.SalaryMax;
        Deadline = source.Deadline;
    }

    // Cross-field validation (runs after the attribute validation succeeds)
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SalaryMax < SalaryMin)
            yield return new ValidationResult("Maximum salary must be greater than or equal to minimum salary.",
                new[] { nameof(SalaryMax) });

        if (Id == 0 && Deadline.Date < DateTime.Today)
            yield return new ValidationResult("The deadline cannot be in the past.", new[] { nameof(Deadline) });
    }
}
