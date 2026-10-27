using Microsoft.AspNetCore.Authentication.Cookies;
using RecruitmentSystem.Models;
using RecruitmentSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------
builder.Services.AddRazorPages(options =>
{
    // Admin folder is restricted to HR administrators; candidate page to candidates.
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    options.Conventions.AuthorizePage("/MyApplications", "CandidateOnly");
});

// Requirement 7: the three C# classes are kept as in-memory collections inside this singleton service.
builder.Services.AddSingleton<IRecruitmentService, InMemoryRecruitmentService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(nameof(UserRole.Admin)));
    options.AddPolicy("CandidateOnly", policy => policy.RequireRole(nameof(UserRole.Candidate)));
});

var app = builder.Build();

// ---------- Pipeline ----------
// Requirement 12: global error handling -> friendly /Error page for exceptions and 404/403 etc.
app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
