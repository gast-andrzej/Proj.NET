using WebApplication2.Domain.Entities;
using WebApplication2.Domain.Exceptions;
using WebApplication2.Services;

namespace WebApplication2.Application.UseCases;

public class RentFilmHandler
{
    private readonly IFilmRepository _repo;
    private readonly UserService _users;

    public RentFilmHandler(IFilmRepository repo, UserService users)
    {
        _repo = repo; _users = users;
    }

    public void Handle(int filmId, string username)
    {
        var film = _repo.GetById(filmId);
        if (film == null) throw new NotFoundException($"Film {filmId} nie znaleziony");

        // business rule: max 5 rentals
        var count = _users.CountRentedBy(username, _repo);
        if (count >= 5) throw new DomainException("Maksymalnie 5 wypożyczeń na użytkownika");

        var entity = new FilmEntity(film.Id, film.Title, film.Director, film.Year, film.IsRented, film.RentedBy, film.IsDeleted);
        entity.Rent(username);

        // persist changes back to model
        film.IsRented = entity.IsRented;
        film.RentedBy = entity.RentedBy;
        _repo.Update(film);
    }
}
