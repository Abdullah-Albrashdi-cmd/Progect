# TalentBridge - Online Recruitment & Management System
COMP4701 Web Application Development - Fall 2026 - Project Phase 1 (Part B)

ASP.NET Core Razor Pages + jQuery (AJAX) + Bootstrap 5. Data is kept in memory as collections of three C# classes
(`User`, `JobPosting`, `JobApplication`), as Requirement 7 describes. Restarting the app resets the data to the demo seed.

## Run it
1. Open `RecruitmentSystem.slnx` (or `RecruitmentSystem/RecruitmentSystem.csproj`) in Visual Studio.
2. If you do not have the .NET 10 SDK, change `<TargetFramework>` in the `.csproj` to `net8.0`.
3. Press **F5**. (CLI: `cd RecruitmentSystem && dotnet run`)
4. The pages load Bootstrap, jQuery and fonts from CDNs, so you need internet access.

Demo accounts
| Role | Email | Password |
|---|---|---|
| HR Admin | admin@hr.com | Admin@123 |
| Candidate | candidate@demo.com | Candidate@123 |

## Folder map
```
RecruitmentSystem/
  Program.cs                      services, cookie auth, role policies, global error handling
  Models/        Enums.cs, EnumExtensions.cs, BusinessRuleException.cs,
                 User.cs, JobPosting.cs, JobApplication.cs
  Services/      IRecruitmentService.cs, InMemoryRecruitmentService.cs (collections + business rules + seed data)
  Pages/         Index, JobDetails, MyApplications, Login, Register, Logout, AccessDenied, Error
  Pages/Admin/   ManageJobs, ReviewApplications
  Pages/Shared/  _Layout, _Alerts, _JobList, _ValidationScriptsPartial
  wwwroot/       css/site.css, js/site.js
```

## Who does what
**Member 1 (Saif)** - candidate side
- `Models/User.cs`, `Models/JobApplication.cs`, enums (`ApplicationStatus`, `JobType`, `UserRole`)
- `Index` (search + jQuery AJAX), `JobDetails` (apply form), `MyApplications`, `Login`, `Register`
- Validation: email, string length, required, regex (phone/password), range, URL, compare
- Business logic: duplicate-application check, only-pending withdraw, final status lock

**Member 2 (Abdullah)** - admin side, layout, styling, errors
- `Models/JobPosting.cs`
- `Pages/Admin/ManageJobs` (create / edit / close / delete) and `Pages/Admin/ReviewApplications` (update status + note)
- `Pages/Shared/_Layout.cshtml`, `wwwroot/css/site.css`, jQuery table filter/sort in `site.js`
- `Pages/Error.cshtml(.cs)`, `AccessDenied`, error handling in `Program.cs`
- Validation: `[Required]`, `[StringLength]`, `[Range]` and cross-field `IValidatableObject` on `JobPosting`

Shared: `Program.cs`, `Services/`, `README`.

## Rubric checklist (Part B)
| # | Requirement | Where |
|---|---|---|
| 6 | Enumeration | `Models/Enums.cs` - used in models, service, dropdowns, badges |
| 7 | 3 C# classes | `User`, `JobPosting`, `JobApplication` (fields, properties, constructors, methods); stored as `List<T>` in the service |
| 8 | Front-end framework | jQuery AJAX live search (`Index`), table filter + sort, confirm dialogs, alert auto-dismiss (`site.js`) |
| 9 | 4+ Razor Pages | Index, JobDetails, MyApplications, Login, Register, Admin/ManageJobs, Admin/ReviewApplications - text boxes, dropdowns, checkboxes, textareas, buttons |
| 10 | Business logic | `InMemoryRecruitmentService` + `JobApplication.UpdateStatus` + page handlers (apply, withdraw, CRUD, status update) |
| 11 | Custom layout | `_Layout.cshtml` with 4 sections (`Styles`, `Hero`, `Footer`, `Scripts`) and role-based navbar |
| 12 | Error handling | `BusinessRuleException`, try/catch in handlers, `/Error` page for exceptions and 404/403, `TempData` alerts |
| 13 | Bootstrap styling | Bootstrap 5.3 customised in `site.css` (navbar, buttons, forms, tables, badges, cards) |
| 14 | Form validation | Required, StringLength, Range, EmailAddress, RegularExpression, Url, Compare, custom `IValidatableObject` - client + server side |

## Things to mention in the report
- Phase 1 stores data in memory (collections of the three classes), as the assignment asks. If Part A mentions
  SQL Server / EF Core / JWT, say that these are the planned persistence/security layer for the full system, and that
  Phase 1 uses in-memory collections and cookie authentication with role-based authorization.
- Passwords are salted and hashed (PBKDF2-SHA256), never stored in plain text.
- Suggested video demo order: register -> login as candidate -> AJAX search -> apply -> duplicate apply error ->
  My Applications -> login as admin -> create/edit job with validation errors -> review applications ->
  candidate sees the new status -> open `/does-not-exist` to show the error page.
