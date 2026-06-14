using System.Collections.Concurrent;
using WebApplication2.Models;

namespace WebApplication2.Services;

public class UserService
{
    private class User { public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; public string Role { get; set; } = "User"; }

    private readonly List<User> _users = new()
    {
        new User { Username = "JanKolwalski", Password = "zaq1@WSX", Role = "User" },
        new User { Username = "BopBopowsky", Password = "zaq1@WSX", Role = "User" },
        new User { Username = "admin", Password = "admin", Role = "Admin" }
    };

    // token -> username
    private readonly ConcurrentDictionary<string, string> _tokens = new();

    public string? Authenticate(string username, string password)
    {
        var u = _users.FirstOrDefault(x => x.Username == username && x.Password == password);
        if (u == null) return null;
        var token = Guid.NewGuid().ToString("N");
        _tokens[token] = u.Username;
        return token;
    }

    public string? ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        return _tokens.TryGetValue(token, out var username) ? username : null;
    }

    public int CountRentedBy(string username, IFilmRepository repo)
    {
        return repo.GetAll().Count(f => f.RentedBy == username);
    }

    public string? GetRoleForUsername(string username)
    {
        var u = _users.FirstOrDefault(x => x.Username == username);
        return u?.Role;
    }

    public IEnumerable<UserInfo> GetAllUsers()
    {
        return _users.Select(u => new UserInfo { Username = u.Username, Role = u.Role }).ToList();
    }

    public void Logout(string token)
    {
        if (string.IsNullOrEmpty(token)) return;
        _tokens.TryRemove(token, out _);
    }
}
