using Microsoft.AspNetCore.Mvc;
using WebApplication2.DTOs;
using WebApplication2.Models;
using WebApplication2.Services;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilmsController : ControllerBase
{
    private readonly IFilmRepository _repo;

    public FilmsController(IFilmRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public ActionResult<IEnumerable<FilmDto>> GetAll([FromQuery]int page = 1, [FromQuery]int pageSize = 10)
    {
        var (items, total) = _repo.GetAllPaged(page, pageSize);
        var films = items.Select(f => new FilmDto { Id = f.Id, Title = f.Title, Director = f.Director, Year = f.Year, IsRented = f.IsRented });
        Response.Headers.Add("X-Total-Count", total.ToString());
        return Ok(films);
    }

    [HttpGet("{id}")]
    public ActionResult<FilmDto> GetById(int id)
    {
        var f = _repo.GetById(id);
        if (f == null) return NotFound();
        var dto = new FilmDto { Id = f.Id, Title = f.Title, Director = f.Director, Year = f.Year, IsRented = f.IsRented };
        return Ok(dto);
    }

    [HttpPost]
    public ActionResult<FilmDto> Create(CreateFilmDto dto)
    {
        var film = new Film { Title = dto.Title, Director = dto.Director, Year = dto.Year, IsRented = false };
        var added = _repo.Add(film);
        var res = new FilmDto { Id = added.Id, Title = added.Title, Director = added.Director, Year = added.Year, IsRented = added.IsRented };
        return CreatedAtAction(nameof(GetById), new { id = res.Id }, res);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, FilmDto dto)
    {
        if (id != dto.Id) return BadRequest();
        var film = new Film { Id = dto.Id, Title = dto.Title, Director = dto.Director, Year = dto.Year, IsRented = dto.IsRented };
        return _repo.Update(film) ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        return _repo.Delete(id) ? NoContent() : NotFound();
    }
}
