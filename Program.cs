using WebApplication2.Models;
using WebApplication2.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IFilmRepository, InMemoryFilmRepository>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddControllers();
var app = builder.Build();

app.UseMiddleware<WebApplication2.Middleware.ErrorHandlingMiddleware>();
app.MapGet("/", () => Results.Content(System.IO.File.Exists("wwwroot/index.html") ? System.IO.File.ReadAllText("wwwroot/index.html") : "Film Rental API" , "text/html"));

app.MapControllers();
app.MapPost("/api/films/{id}/rent", (int id, HttpRequest req, IFilmRepository repo, UserService users) =>
{
    if(!req.Headers.TryGetValue("Authorization", out var auth)) return Results.Unauthorized();
    var parts = auth.ToString().Split(' ');
    if(parts.Length != 2) return Results.Unauthorized();
    var token = parts[1];
    var username = users.ValidateToken(token);
    if(username == null) return Results.Unauthorized();
    var current = users.CountRentedBy(username, repo);
    if(current >= 5) return Results.BadRequest(new { error = "Maksymalnie 5 wypożyczeń na użytkownika" });

    return repo.Rent(id, username) ? Results.Ok() : Results.BadRequest();
});

app.MapPost("/api/auth/login", (UserLogin login, UserService users) =>
{
    var token = users.Authenticate(login.Username, login.Password);
    if(token == null) return Results.Unauthorized();
    var role = users.GetRoleForUsername(login.Username) ?? "User";
    return Results.Ok(new AuthResponse{ Token = token, Username = login.Username, Role = role });
});

app.MapGet("/api/admin/users", (UserService users, IFilmRepository repo, HttpRequest req) =>
{
    if(!req.Headers.TryGetValue("Authorization", out var auth)) return Results.Unauthorized();
    var parts = auth.ToString().Split(' ');
    if(parts.Length != 2) return Results.Unauthorized();
    var token = parts[1];
    var username = users.ValidateToken(token);
    if(username == null) return Results.Unauthorized();
    var role = users.GetRoleForUsername(username);
    if(role != "Admin") return Results.Forbid();

    var userInfos = users.GetAllUsers();
    var data = userInfos.Select(u => new {
        u.Username,
        u.Role,
        Rented = repo.GetAll().Where(f => f.RentedBy == u.Username).Select(f => new { f.Id, f.Title })
    });
    return Results.Ok(data);
});

app.MapPost("/api/auth/logout", (HttpRequest req, UserService users) =>
{
    if(!req.Headers.TryGetValue("Authorization", out var auth)) return Results.BadRequest();
    var parts = auth.ToString().Split(' ');
    if(parts.Length != 2) return Results.BadRequest();
    var token = parts[1];
    users.Logout(token);
    return Results.Ok();
});

app.MapPost("/api/films/{id}/return", (int id, HttpRequest req, IFilmRepository repo, UserService users) =>
{
    if(!req.Headers.TryGetValue("Authorization", out var auth)) return Results.Unauthorized();
    var parts = auth.ToString().Split(' ');
    if(parts.Length != 2) return Results.Unauthorized();
    var token = parts[1];
    var username = users.ValidateToken(token);
    if(username == null) return Results.Unauthorized();
    var role = users.GetRoleForUsername(username);
    if(role == "Admin") return repo.Return(id, username) ? Results.Ok() : Results.BadRequest();
    return repo.Return(id, username) ? Results.Ok() : Results.BadRequest();
});

app.Run();
