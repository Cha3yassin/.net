using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoviesApi.Dtos;
using MoviesApi.Models;
using MoviesApi.Services;

namespace MoviesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMoviesService _moviesService;
        private readonly IGenresService _genresService;
        private readonly IMapper _mapper;

        public MoviesController(
            IMoviesService moviesService,
            IGenresService genresService,
            IMapper mapper)
        {
            _moviesService = moviesService;
            _genresService = genresService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] byte genreId = 0)
        {
            var movies = await _moviesService.GetAll(genreId);

            var result = _mapper.Map<IEnumerable<MovieDetailsDto>>(movies);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var movie = await _moviesService.GetById(id);

            if (movie == null)
                return NotFound();

            var result = _mapper.Map<MovieDetailsDto>(movie);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MovieDto dto)
        {
            if (!IsValidPoster(dto.Poster))
                return BadRequest("Le poster doit être un fichier .png ou .jpg de 1 Mo maximum.");

            if (!await _genresService.IsvalidGenre(dto.GenreId))
                return BadRequest("Le genre indiqué n'existe pas.");

            byte[] posterBytes;

            using (var stream = new MemoryStream())
            {
                await dto.Poster.CopyToAsync(stream);
                posterBytes = stream.ToArray();
            }

            var movie = new Movie
            {
                Title = dto.Title,
                Year = dto.Year,
                Rate = dto.Rate,
                Storeline = dto.Storeline,
                GenreId = dto.GenreId,
                Poster = posterBytes
            };

            await _moviesService.Add(movie);

            var result = _mapper.Map<MovieDetailsDto>(
                await _moviesService.GetById(movie.Id));

            return CreatedAtAction(
                nameof(GetById),
                new { id = movie.Id },
                result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] MovieDto dto)
        {
            var movie = await _moviesService.GetById(id);

            if (movie == null)
                return NotFound();

            if (!await _genresService.IsvalidGenre(dto.GenreId))
                return BadRequest("Le genre indiqué n'existe pas.");

            movie.Title = dto.Title;
            movie.Year = dto.Year;
            movie.Rate = dto.Rate;
            movie.Storeline = dto.Storeline;
            movie.GenreId = dto.GenreId;

            if (dto.Poster != null && dto.Poster.Length > 0)
            {
                if (!IsValidPoster(dto.Poster))
                    return BadRequest("Le poster doit être un fichier .png ou .jpg de 1 Mo maximum.");

                using var stream = new MemoryStream();
                await dto.Poster.CopyToAsync(stream);
                movie.Poster = stream.ToArray();
            }

            _moviesService.Update(movie);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _moviesService.GetById(id);

            if (movie == null)
                return NotFound();

            _moviesService.Delete(movie);

            return NoContent();
        }

        private bool IsValidPoster(IFormFile poster)
        {
            if (poster == null || poster.Length == 0)
                return false;

            if (poster.Length > 1024 * 1024)
                return false;

            var extension = Path.GetExtension(poster.FileName);

            return extension.Equals(
                       ".png",
                       StringComparison.OrdinalIgnoreCase)
                || extension.Equals(
                       ".jpg",
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}
