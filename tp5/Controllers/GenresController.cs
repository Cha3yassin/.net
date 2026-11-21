using Microsoft.AspNetCore.Mvc;
using MoviesApi.Dtos;
using MoviesApi.Models;
using MoviesApi.Services;

namespace MoviesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenresController : ControllerBase
    {
        private readonly IGenresService _genresService;

        public GenresController(IGenresService genresService)
        {
            _genresService = genresService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var genres = await _genresService.GetAll();
            return Ok(genres);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(byte id)
        {
            var genre = await _genresService.GetById(id);

            if (genre == null)
                return NotFound();

            return Ok(genre);
        }

        [HttpPost]
        public async Task<IActionResult> Create(GenreDto dto)
        {
            var genre = new Genre
            {
                Name = dto.Name
            };

            await _genresService.Add(genre);

            return CreatedAtAction(
                nameof(GetById),
                new { id = genre.Id },
                genre);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(byte id, GenreDto dto)
        {
            var genre = await _genresService.GetById(id);

            if (genre == null)
                return NotFound();

            genre.Name = dto.Name;
            _genresService.Update(genre);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(byte id)
        {
            var genre = await _genresService.GetById(id);

            if (genre == null)
                return NotFound();

            _genresService.Delete(genre);

            return NoContent();
        }
    }
}
