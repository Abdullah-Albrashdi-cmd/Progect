using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RecruitmentSystem.Models;
using RecruitmentSystem.Services;

namespace RecruitmentSystem.Pages.Admin;

public class ReviewApplicationsModel : PageModel
{
    private readonly IRecruitmentService _service;
    public ReviewApplicationsModel(IRecruitmentService service) => _service = service;

    [BindProperty(SupportsGet = true)] public ApplicationStatus? StatusFilter { get; set; }
    [BindProperty(SupportsGet = true)] public int? JobId { get; set; }

    public IReadOnlyList<ApplicationDetails> Items { get; private set; } = [];
    public List<SelectListItem> JobOptions { get; private set; } = new();

    public void OnGet() => LoadData();

    public IActionResult OnPostUpdate(int id, ApplicationStatus status, string? note)
    {
        try
        {
            _service.UpdateApplicationStatus(id, status, note);
            TempData["Success"] = $"Application #{id} marked as {status.GetDisplayName()}.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }
        // Keep the current filters after saving
        return RedirectToPage(new { StatusFilter, JobId });
    }

    private void LoadData()
    {
        Items = _service.GetApplications(StatusFilter, JobId);
        JobOptions = _service.SearchJobs(null, null, null, activeOnly: false)
            .Select(j => new SelectListItem(j.Title + " - " + j.Company, j.Id.ToString()))
            .ToList();
    }
}
