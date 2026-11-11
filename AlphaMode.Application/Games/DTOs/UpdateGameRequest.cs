using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Application.Games.DTOs;

public class UpdateGameRequest
{
    public Guid Id { get; set; }


    [Required(ErrorMessage = "Game name is required.")]
    [StringLength(
        150,
        ErrorMessage = "Game name cannot exceed 150 characters.")]
    public string Name { get; set; } = string.Empty;


    [Required(ErrorMessage = "Version is required.")]
    [StringLength(
        50,
        ErrorMessage = "Version cannot exceed 50 characters.")]
    public string Version { get; set; } = string.Empty;


    [Required(ErrorMessage = "Please select a platform.")]
    public string Platform { get; set; } = string.Empty;


    [Url(ErrorMessage = "Please enter a valid image URL.")]
    [StringLength(
        500,
        ErrorMessage = "Image URL cannot exceed 500 characters.")]
    public string? ImageUrl { get; set; }


    public bool IsActive { get; set; }
}