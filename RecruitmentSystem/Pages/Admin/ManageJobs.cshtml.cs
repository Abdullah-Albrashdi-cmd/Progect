using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RecruitmentSystem.Models;
using RecruitmentSystem.Services;

namespace RecruitmentSystem.Pages.Admin;

public class ManageJobsModel : PageModel
{
    private readonly IRecruitmentService _service;
    public ManageJobsModel(IRecruitmentService service) => _service = service;

    public IReadOnlyList<JobPosting> Jobs { get; private set; } = [];
    public Dictionary<int, int> ApplicationCounts { get; private set; } = new();
    public int ActiveCount { get; private set; }
    public int TotalApplications { get; private set; }
    public int PendingCount { get; private set; }

    // The JobPosting class itself is the form model, so its validation attributes apply (Requirement 14).
    [BindProperty] public JobPosting Job { get; set; } = new();

    public bool IsEditing => Job.Id != 0;

    public IActionResult OnGet(int? editId)
    {
        LoadData();

        if (editId.HasValue)
        {
            var existing = _service.GetJob(editId.Value);
            if (existing == null)
            {
                TempData["Error"] = "The job you tried to edit no longer exists.";
                return RedirectToPage();
            }
            Job = existing;
        }
        return Page();
    }

    public IActionResult OnPostSave()
    {
        if (!ModelState.IsValid)
        {
            LoadData();
            return Page();
        }

        try
        {
            if (Job.Id == 0)
            {
                _service.AddJob(Job);
                TempData["Success"] = $"Job \"{Job.Title}\" was created.";
            }
            else
            {
                _service.UpdateJob(Job);
                TempData["Success"] = $"Job \"{Job.Title}\" was updated.";
            }
            return RedirectToPage();
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            LoadData();
            return Page();
        }
    }

    public IActionResult OnPostToggle(int id)
    {
        try
        {
            _service.ToggleJob(id);
            TempData["Success"] = "Job status updated.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToPage();
    }

    public IActionResult OnPostDelete(int id)
    {
        try
        {
            _service.DeleteJob(id);
            TempData["Success"] = "Job and its applications were deleted.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToPage();
    }

    private void LoadData()
    {
        Jobs = _service.SearchJobs(null, null, null, activeOnly: false);
        var applications = _service.GetApplications();
        ApplicationCounts = applications.GroupBy(a => a.Job.Id).ToDictionary(g => g.Key, g => g.Count());
        ActiveCount = Jobs.Count(j => j.IsActive());
        TotalApplications = applications.Count;
        PendingCount = applications.Count(a => a.Application.Status == ApplicationStatus.Pending);
    }
}
