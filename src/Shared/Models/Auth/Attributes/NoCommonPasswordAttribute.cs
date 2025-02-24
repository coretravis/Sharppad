namespace SharpPad.Shared.Models.Auth.Attributes;


using System.ComponentModel.DataAnnotations;
public class NoCommonPasswordAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string password)
        {
            // Add your list of common passwords
            var commonPasswords = new[] { "Password123!", "Admin123!", "Welcome123!" };
            if (commonPasswords.Contains(password))
            {
                return new ValidationResult("This is a commonly used password. Please choose something more unique.");
            }
        }
        return ValidationResult.Success;
    }
}
