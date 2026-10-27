namespace RecruitmentSystem.Models;

/// <summary>Thrown when a business rule is broken (duplicate application, missing record, ...).
/// Pages catch it and show a friendly message instead of crashing.</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
