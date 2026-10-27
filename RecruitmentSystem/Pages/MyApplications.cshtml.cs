using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RecruitmentSystem.Models;
using RecruitmentSystem.Services;

namespace RecruitmentSystem.Pages;

public class MyApplicationsModel : PageModel
{
    private readonly IRecruitmentService _service;
    public MyApplicationsModel(IRecruitmentService service) => _service = service;

    public IReadOnlyList<ApplicationDetails> Items { get; private set; } = [];

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public void OnGet() => Items = _service.GetApplicationsForUser(UserId);

    public IActionResult OnPostWithdraw(int id)
    {
        try
        {
            _service.Withdraw(id, UserId);
            TempData["Success"] = "Your application was withdrawn.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToPage();
    }
}
