using WebApplication2.Domain.Exceptions;

namespace WebApplication2.Domain.Entities;

public class FilmEntity
{
    public int Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Director { get; private set; }
    public int Year { get; private set; }
    public bool IsRented { get; private set; }
    public string? RentedBy { get; private set; }
    public bool IsDeleted { get; private set; }

    public FilmEntity(int id, string title, string? director, int year, bool isRented, string? rentedBy, bool isDeleted)
    {
        Id = id; Title = title; Director = director; Year = year; IsRented = isRented; RentedBy = rentedBy; IsDeleted = isDeleted;
    }

    public void Rent(string username)
    {
        if (IsDeleted) throw new DomainException("Nie można wypożyczyć usuniętego filmu");
        if (IsRented) throw new DomainException("Film jest już wypożyczony");
        if (string.IsNullOrWhiteSpace(username)) throw new DomainException("Nieprawidłowy użytkownik");
        IsRented = true;
        RentedBy = username;
    }

    public void Return(string username, bool isAdmin)
    {
        if (!IsRented) throw new DomainException("Film nie jest wypożyczony");
        if (!isAdmin && RentedBy != username) throw new DomainException("Tylko wypożyczający lub admin może zwrócić film");
        IsRented = false;
        RentedBy = null;
    }

    public void SoftDelete() => IsDeleted = true;
}
