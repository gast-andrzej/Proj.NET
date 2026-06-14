using WebApplication2.Models;

namespace WebApplication2.Services;

public interface IFilmRepository
{
    IEnumerable<Film> GetAll();
    (IEnumerable<Film> Items, int Total) GetAllPaged(int page, int pageSize);
    Film? GetById(int id);
    Film Add(Film film);
    bool Update(Film film);
    bool Delete(int id); // soft delete
    bool Rent(int id, string username);
    bool Return(int id, string username);
}
