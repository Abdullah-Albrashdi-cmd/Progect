using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RecruitmentSystem.Models;
using RecruitmentSystem.Services;

namespace RecruitmentSystem.Pages;

public class RegisterModel : PageModel
{
    private readonly IRecruitmentService _service;
    public RegisterModel(IRecruitmentService service) => _service = service;

    [BindProperty] public RegisterInput Input { get; set; } = new();

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToPage("/Index");
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            _service.Register(new User(Input.FullName, Input.Email, Input.Phone, Input.Password));
            TempData["Success"] = "Account created successfully. Please log in.";
            return RedirectToPage("/Login");
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError("Input.Email", ex.Message);
            return Page();
        }
    }

    public class RegisterInput
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 80 characters.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\+?[0-9]{8,15}$", ErrorMessage = "Phone must contain 8-15 digits (optional leading +).")]
        [Display(Name = "Phone number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 50 characters.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one letter and one number.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
