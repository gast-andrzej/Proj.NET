using WebApplication2.Domain.Entities;
using WebApplication2.Domain.Exceptions;
using WebApplication2.Services;

namespace WebApplication2.Application.UseCases;

public class ReturnFilmHandler
{
    private readonly IFilmRepository _repo;
    private readonly UserService _users;

    public ReturnFilmHandler(IFilmRepository repo, UserService users)
    {
        _repo = repo; _users = users;
    }

    public void Handle(int filmId, string username)
    {
        var film = _repo.GetById(filmId);
        if (film == null) throw new NotFoundException($"Film {filmId} nie znaleziony");

        var isAdmin = _users.GetRoleForUsername(username) == "Admin";
        var entity = new FilmEntity(film.Id, film.Title, film.Director, film.Year, film.IsRented, film.RentedBy, film.IsDeleted);
        entity.Return(username, isAdmin);

        film.IsRented = entity.IsRented;
        film.RentedBy = entity.RentedBy;
        _repo.Update(film);
    }
}
