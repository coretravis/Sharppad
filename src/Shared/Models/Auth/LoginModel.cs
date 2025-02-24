using System.ComponentModel.DataAnnotations;

namespace SharpPad.Shared.Models.Auth;
/// <summary>
/// Represents a login model for authentication.
/// </summary>
public class LoginModel : IValidatableObject
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores and hyphens")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Validates the login model.
    /// </summary>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>A collection of validation results.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Add any extra custom validation logic here
        if (Username.Equals(Password, StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult("Password cannot be the same as username",
                new[] { nameof(Password) });
        }
    }
}
