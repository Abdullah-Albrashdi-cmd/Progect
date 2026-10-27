using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RecruitmentSystem.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    private readonly ILogger<ErrorModel> _logger;
    private readonly IWebHostEnvironment _env;

    public ErrorModel(ILogger<ErrorModel> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    [BindProperty(SupportsGet = true)] public int? Code { get; set; }

    public string Heading { get; private set; } = "Something went wrong";
    public string Message { get; private set; } = "An unexpected error occurred. Please try again.";
    public string? RequestId { get; private set; }
    public string? Details { get; private set; }

    public void OnGet()
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionFeature?.Error != null)
        {
            Code = 500;
            _logger.LogError(exceptionFeature.Error, "Unhandled exception on {Path}", exceptionFeature.Path);
            if (_env.IsDevelopment()) Details = exceptionFeature.Error.Message;
        }

        (Heading, Message) = Code switch
        {
            400 => ("Bad request", "The request could not be understood. Please check the data you entered."),
            401 => ("Please log in", "You need to log in to view this page."),
            403 => ("Access denied", "You do not have permission to view this page."),
            404 => ("Page not found", "The page or record you are looking for does not exist or was removed."),
            500 => ("Server error", "Something went wrong on our side. The problem has been logged - please try again later."),
            _ => ("Something went wrong", "An unexpected error occurred. Please try again.")
        };
    }

    // The exception handler re-executes the original request, so POST requests land here too.
    public void OnPost() => OnGet();
}
