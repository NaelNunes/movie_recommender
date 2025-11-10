using Microsoft.AspNetCore.Mvc;
using MovieRecommender.Application.DTOs;
using MovieRecommender.Application.Services;
using MovieRecommender.Domain.Entities;
using MovieRecommender.Domain.Interfaces;

namespace MovieRecommender.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly MovieRecommender.Application.Services.MovieService _movieService;

        public MoviesController(MovieRecommender.Application.Services.MovieService movieService)
        {
            _movieService = movieService;
        }

        public class PromptRequest
        {
            public string Prompt { get; set; } = string.Empty;
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] PromptRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest("Prompt is required.");

            const int enforcedLimit = 5;
            var similarMovies = await _movieService.SearchSimilarAsync(request.Prompt, enforcedLimit);

            var result = similarMovies.Select(m => new MovieRecommender.Application.DTOs.MovieResponseDto
			{
				Id = m.Id,
				Title = m.Title,
				Description = m.overview,
				Genre = m.Genre,
				Rating = (float?)m.Rating,
				Director = m.Director,
				PosterPath = ResolveImageUrl(m.poster_path, isBackdrop: false),
				BackdropPath = ResolveImageUrl(m.backdropt_path, isBackdrop: true)
			});

            return Ok(result);
        }

        public class SeedRequest
        {
            public List<string> Titles { get; set; } = new();
        }

        [HttpPost("create")]
        public async Task<IActionResult> Seed([FromBody] SeedRequest request)
        {
            if (request == null || request.Titles == null || request.Titles.Count == 0)
                return BadRequest("Provide a list of movie titles to seed.");

            var added = await _movieService.SeedMoviesAsync(request.Titles);

            var result = added.Select(m => new MovieRecommender.Application.DTOs.MovieResponseDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.overview,
                Genre = m.Genre,
                PosterPath = ResolveImageUrl(m.poster_path, isBackdrop: false),
                BackdropPath = ResolveImageUrl(m.backdropt_path, isBackdrop: true)
            });

            return Ok(result);
        }

        public class SeedPopularRequest
        {
            public int Count { get; set; } = 200;
        }

        [HttpPost("sendmovies/")]
        public async Task<IActionResult> SeedPopular([FromBody] SeedPopularRequest request)
        {
            if (request == null || request.Count <= 0)
                return BadRequest("Provide a positive count of movies to seed.");

            var limit = Math.Min(request.Count, 1000);

            var results = await _movieService.SeedPopularAsync(limit);

            var seededCount = results.Count(r => r.Success);

            return Ok(new { requested = request.Count, seeded = seededCount, items = results });
        }

        [HttpDelete("admin/clear")]
        public async Task<IActionResult> ClearAllMovies()
        {
            await _movieService.DeleteAllMoviesAsync();
            return Ok(new { cleared = true });
        }

        // Helper to ensure we return a usable image URL to the client.
        // If the stored path is already a full URL, return as-is. If it's a TMDB partial path
        // (starts with '/'), prepend the TMDB image base. Otherwise return null when empty.
        private static string? ResolveImageUrl(string? storedPath, bool isBackdrop)
        {
            if (string.IsNullOrWhiteSpace(storedPath)) return null;
            var trimmed = storedPath.Trim();
            if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return trimmed;

            if (trimmed.StartsWith("/"))
            {
                return isBackdrop ? $"https://image.tmdb.org/t/p/original{trimmed}" : $"https://image.tmdb.org/t/p/w500{trimmed}";
            }

            // If it's some relative or already-resolved path, just return it — frontend can decide how to fetch.
            return trimmed;
        }
    }
}
