using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RecruitmentSystem.Models;
using RecruitmentSystem.Services;

namespace RecruitmentSystem.Pages;

public class JobDetailsModel : PageModel
{
    private readonly IRecruitmentService _service;
    public JobDetailsModel(IRecruitmentService service) => _service = service;

    public JobPosting Job { get; private set; } = null!;
    public bool AlreadyApplied { get; private set; }

    [BindProperty] public ApplyInput Input { get; set; } = new();

    public IActionResult OnGet(int id)
    {
        if (!Load(id)) return MissingJob();
        return Page();
    }

    public IActionResult OnPost(int id)
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToPage("/Login", new { ReturnUrl = Url.Page("/JobDetails", new { id }) });

        if (!User.IsInRole(nameof(UserRole.Candidate)))
        {
            TempData["Error"] = "Only candidate accounts can apply for jobs.";
            return RedirectToPage(new { id });
        }

        if (!Load(id)) return MissingJob();

        if (!Input.Confirm)
            ModelState.AddModelError("Input.Confirm", "Please confirm that your information is accurate.");

        if (!ModelState.IsValid) return Page();

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _service.Apply(id, userId, Input.CoverLetter, Input.ResumeUrl, Input.YearsExperience, Input.Source);
            TempData["Success"] = $"Your application for \"{Job.Title}\" was submitted successfully.";
            return RedirectToPage("/MyApplications");
        }
        catch (BusinessRuleException ex)
        {
            // Expected problems (duplicate application, job closed...) -> friendly message on the form
            ModelState.AddModelError(string.Empty, ex.Message);
            Load(id);
            return Page();
        }
    }

    private bool Load(int id)
    {
        var job = _service.GetJob(id);
        if (job == null) return false;
        Job = job;

        if (User.IsInRole(nameof(UserRole.Candidate)) &&
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            AlreadyApplied = _service.HasApplied(userId, id);

        return true;
    }

    private IActionResult MissingJob()
    {
        TempData["Error"] = "Sorry, that job posting could not be found.";
        return RedirectToPage("/Index");
    }

    public class ApplyInput
    {
        [Required(ErrorMessage = "Please write a short cover letter.")]
        [StringLength(1500, MinimumLength = 30, ErrorMessage = "Cover letter must be between 30 and 1500 characters.")]
        [Display(Name = "Cover letter")]
        public string CoverLetter { get; set; } = string.Empty;

        [Url(ErrorMessage = "Please enter a valid URL (e.g. https://...).")]
        [Display(Name = "Resume / portfolio link")]
        public string? ResumeUrl { get; set; }

        [Range(0, 40, ErrorMessage = "Years of experience must be between 0 and 40.")]
        [Display(Name = "Years of experience")]
        public int YearsExperience { get; set; }

        [Display(Name = "How did you hear about us?")]
        public string? Source { get; set; }

        [Display(Name = "I confirm that the information provided is accurate")]
        public bool Confirm { get; set; }
    }
}
