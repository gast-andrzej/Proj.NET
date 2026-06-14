namespace WebApplication2.Models;

public class Film
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Director { get; set; }
    public int Year { get; set; }
    public bool IsRented { get; set; }
    public string? RentedBy { get; set; }
    public bool IsDeleted { get; set; }
}
