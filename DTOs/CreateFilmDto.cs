using System.ComponentModel.DataAnnotations;

namespace WebApplication2.DTOs;

public class CreateFilmDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Director { get; set; }

    [Range(1800, 2100)]
    public int Year { get; set; }
}
