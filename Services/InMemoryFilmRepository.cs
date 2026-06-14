using System.Text.Json;
using WebApplication2.Models;

namespace WebApplication2.Services;

public class InMemoryFilmRepository : IFilmRepository
{
    private readonly List<Film> _films = new();
    private readonly object _lock = new();
    private int _nextId = 1;
    private readonly string _filePath;

    public InMemoryFilmRepository()
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, "films.json");
        if (File.Exists(_filePath))
        {
            try
            {
                var json = File.ReadAllText(_filePath);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var list = JsonSerializer.Deserialize<List<Film>>(json, opts);
                if (list != null && list.Any())
                {
                    _films.AddRange(list);
                    _nextId = _films.Max(f => f.Id) + 1;
                    return;
                }
            }
            catch
            {
                // ignore and fall back to defaults
            }
        }

        // default seed
        _films.AddRange(new[] {
            new Film { Id = 1, Title = "The Matrix", Director = "Wachowskis", Year = 1999, IsRented = false },
            new Film { Id = 2, Title = "Inception", Director = "Christopher Nolan", Year = 2010, IsRented = false },
            new Film { Id = 3, Title = "The Godfather", Director = "Francis Ford Coppola", Year = 1972, IsRented = false },
            new Film { Id = 4, Title = "Pulp Fiction", Director = "Quentin Tarantino", Year = 1994, IsRented = false },
            new Film { Id = 5, Title = "The Shawshank Redemption", Director = "Frank Darabont", Year = 1994, IsRented = false },
            new Film { Id = 6, Title = "Forrest Gump", Director = "Robert Zemeckis", Year = 1994, IsRented = false },
            new Film { Id = 7, Title = "The Dark Knight", Director = "Christopher Nolan", Year = 2008, IsRented = false },
            new Film { Id = 8, Title = "Fight Club", Director = "David Fincher", Year = 1999, IsRented = false },
            new Film { Id = 9, Title = "The Lord of the Rings: The Fellowship of the Ring", Director = "Peter Jackson", Year = 2001, IsRented = false },
            new Film { Id = 10, Title = "The Lord of the Rings: The Two Towers", Director = "Peter Jackson", Year = 2002, IsRented = false },
            new Film { Id = 11, Title = "The Lord of the Rings: The Return of the King", Director = "Peter Jackson", Year = 2003, IsRented = false },
            new Film { Id = 12, Title = "The Social Network", Director = "David Fincher", Year = 2010, IsRented = false },
            new Film { Id = 13, Title = "Parasite", Director = "Bong Joon-ho", Year = 2019, IsRented = false },
            new Film { Id = 14, Title = "Interstellar", Director = "Christopher Nolan", Year = 2014, IsRented = false },
            new Film { Id = 15, Title = "Casablanca", Director = "Michael Curtiz", Year = 1942, IsRented = false },
            new Film { Id = 16, Title = "Back to the Future", Director = "Robert Zemeckis", Year = 1985, IsRented = false },
            new Film { Id = 17, Title = "Gladiator", Director = "Ridley Scott", Year = 2000, IsRented = false },
            new Film { Id = 18, Title = "The Silence of the Lambs", Director = "Jonathan Demme", Year = 1991, IsRented = false },
            new Film { Id = 19, Title = "Toy Story", Director = "John Lasseter", Year = 1995, IsRented = false },
            new Film { Id = 20, Title = "Spirited Away", Director = "Hayao Miyazaki", Year = 2001, IsRented = false }
        });
        _nextId = _films.Max(f => f.Id) + 1;
        Save();
    }

    public IEnumerable<Film> GetAll()
    {
        lock(_lock) { return _films.Where(f => !f.IsDeleted).Select(f => Clone(f)).ToList(); }
    }

    public Film? GetById(int id) => _films.FirstOrDefault(f => f.Id == id) is Film f ? Clone(f) : null;

    public Film Add(Film film)
    {
        lock(_lock)
        {
            film.Id = _nextId++;
            _films.Add(Clone(film));
            Save();
            return Clone(film);
        }
    }

    public bool Update(Film film)
    {
        lock(_lock)
        {
            var existing = _films.FirstOrDefault(x => x.Id == film.Id);
            if (existing == null) return false;
            existing.Title = film.Title;
            existing.Director = film.Director;
            existing.Year = film.Year;
            existing.IsRented = film.IsRented;
            existing.RentedBy = film.RentedBy;
            Save();
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock(_lock)
        {
            var film = _films.FirstOrDefault(f => f.Id == id);
            if (film == null) return false;
            film.IsDeleted = true;
            Save();
            return true;
        }
    }

    public bool Rent(int id, string username)
    {
        lock(_lock)
        {
            var film = _films.FirstOrDefault(f => f.Id == id);
            if (film == null || film.IsRented) return false;
            film.IsRented = true;
            film.RentedBy = username;
            Save();
            return true;
        }
    }

    public bool Return(int id, string username)
    {
        lock(_lock)
        {
            var film = _films.FirstOrDefault(f => f.Id == id);
            if (film == null || !film.IsRented) return false;
            if (film.RentedBy != null && film.RentedBy != username) return false;
            film.IsRented = false;
            film.RentedBy = null;
            Save();
            return true;
        }
    }

    public (IEnumerable<Film> Items, int Total) GetAllPaged(int page, int pageSize)
    {
        lock(_lock)
        {
            var q = _films.Where(f => !f.IsDeleted).ToList();
            var total = q.Count;
            var items = q.Skip((page - 1) * pageSize).Take(pageSize).Select(f => Clone(f)).ToList();
            return (items, total);
        }
    }

    private void Save()
    {
        try
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_films, opts);
            File.WriteAllText(_filePath, json);
        }
        catch
        {
            // ignore write errors
        }
    }

    private static Film Clone(Film f) => new Film { Id = f.Id, Title = f.Title, Director = f.Director, Year = f.Year, IsRented = f.IsRented, RentedBy = f.RentedBy, IsDeleted = f.IsDeleted };
}
