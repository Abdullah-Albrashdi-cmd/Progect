using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace RecruitmentSystem.Models;

public static class EnumExtensions
{
    /// <summary>Returns the [Display(Name)] text of an enum value (falls back to the enum name).</summary>
    public static string GetDisplayName(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        return member?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString();
    }

    /// <summary>Bootstrap badge colour for each application status.</summary>
    public static string ToBadgeClass(this ApplicationStatus status) => status switch
    {
        ApplicationStatus.Pending => "text-bg-secondary",
        ApplicationStatus.UnderReview => "text-bg-info",
        ApplicationStatus.Interviewed => "text-bg-warning",
        ApplicationStatus.Accepted => "text-bg-success",
        ApplicationStatus.Rejected => "text-bg-danger",
        _ => "text-bg-light"
    };
}
