using System.ComponentModel.DataAnnotations;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Validation;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class ValidDecimalAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || value?.ToString()?.Length == 0)
        {
            return ValidationResult.Success;
        }

        return decimal.TryParse(value?.ToString(), out _) ? ValidationResult.Success : new ValidationResult(ErrorMessage);
    }
}
