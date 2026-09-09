using System.ComponentModel.DataAnnotations;

namespace AlphaMode.ViewModels;

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "Enter your first name.")]
    [StringLength(50, MinimumLength = 2)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your last name.")]
    [StringLength(50, MinimumLength = 2)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Choose a gamer tag.")]
    [StringLength(30, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Use only letters, numbers, underscores, and hyphens.")]
    [Display(Name = "Gamer tag")]
    public string GamerTag { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Create a password.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Use at least 8 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms to create an account.")]
    public bool AcceptTerms { get; set; }

    [Display(Name = "Keep me signed in")]
    public bool RememberMe { get; set; }
}

