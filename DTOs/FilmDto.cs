namespace WebApplication2.DTOs;

public class FilmDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Director { get; set; }
    public int Year { get; set; }
    public bool IsRented { get; set; }
}
