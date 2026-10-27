using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.Models;

/// <summary>Requirement 6 - enumerations used consistently across the whole application.</summary>
public enum UserRole
{
    Candidate = 0,
    Admin = 1
}

public enum JobType
{
    [Display(Name = "Full Time")] FullTime,
    [Display(Name = "Part Time")] PartTime,
    Contract,
    Internship,
    Remote
}

public enum ApplicationStatus
{
    Pending,
    [Display(Name = "Under Review")] UnderReview,
    Interviewed,
    Accepted,
    Rejected
}
