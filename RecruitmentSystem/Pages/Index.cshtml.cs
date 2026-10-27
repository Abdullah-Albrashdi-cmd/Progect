using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RecruitmentSystem.Models;
using RecruitmentSystem.Services;

namespace RecruitmentSystem.Pages;

public class IndexModel : PageModel
{
    private readonly IRecruitmentService _service;
    public IndexModel(IRecruitmentService service) => _service = service;

    [BindProperty(SupportsGet = true)] public string? Keyword { get; set; }
    [BindProperty(SupportsGet = true)] public string? Location { get; set; }
    [BindProperty(SupportsGet = true)] public JobType? Type { get; set; }

    public IReadOnlyList<JobPosting> Jobs { get; private set; } = [];

    public void OnGet() => Jobs = _service.SearchJobs(Keyword, Location, Type, activeOnly: true);

    // Called by jQuery AJAX: /Index?handler=Search&Keyword=...  -> returns only the job cards (no full reload)
    public IActionResult OnGetSearch()
    {
        Jobs = _service.SearchJobs(Keyword, Location, Type, activeOnly: true);
        return Partial("_JobList", Jobs);
    }
}
