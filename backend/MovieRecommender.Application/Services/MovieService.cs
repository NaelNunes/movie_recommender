using Microsoft.Extensions.Logging;
using MovieRecommender.Domain.Entities;
using MovieRecommender.Domain.Interfaces;

namespace MovieRecommender.Application.Services
{
    public class MovieService
    {
    private readonly IMovieRepository _movieRepository;
    private readonly EmbeddingService _embeddingService;
    private readonly ExternalMovieService _externalMovieService;
    private readonly Microsoft.Extensions.Logging.ILogger<MovieService> _logger;

        public MovieService(IMovieRepository movieRepository, EmbeddingService embeddingService, ExternalMovieService externalMovieService, Microsoft.Extensions.Logging.ILogger<MovieService> logger)
        {
            _movieRepository = movieRepository;
            _embeddingService = embeddingService;
            _externalMovieService = externalMovieService;
            _logger = logger;
        }

        public async Task AddMovieAsync(Movie movie)
        {
            if (movie.Embedding == null || movie.Embedding.Count == 0)
            {
                movie.Embedding = await _embeddingService.GenerateEmbeddingAsync(movie.Title, movie.overview, movie.Genre);
            }

            await _movieRepository.AddMovieAsync(movie);
        }

        public async Task DeleteMovieAsync(int id)
        {
            await _movieRepository.DeleteMovieAsync(id);
        }

        public async Task DeleteAllMoviesAsync()
        {
            await _movieRepository.DeleteAllMoviesAsync();
        }

        public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
        {
            return await _movieRepository.GetAllMoviesAsync();
        }

        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            return await _movieRepository.GetMovieByIdAsync(id);
        }

        public async Task UpdateMovieAsync(Movie movie)
        {
            await _movieRepository.UpdateMovieAsync(movie);
        }

        public async Task<IEnumerable<Movie>> SearchSimilarAsync(string prompt, int limit = 5)
        {
            var promptEmbedding = await _embeddingService.GeneratePromptEmbeddingAsync(prompt);
            var similar = await _movieRepository.GetSimilarMoviesAsync(promptEmbedding, limit);
            return similar;
        }

        public async Task<IEnumerable<Movie>> SeedMoviesAsync(IEnumerable<string> titles)
        {
            var added = new List<Movie>();

            foreach (var title in titles)
            {
                if(string.IsNullOrWhiteSpace(title))
                    continue;
                else
                {
                    var (poster, backdrop) = await _externalMovieService.GetMovieImagesAsync(title);

                    var movie = new Movie
                    {
                        Title = title,
                        overview = string.Empty,
                        Genre = string.Empty,
                        poster_path = poster ?? string.Empty,
                        backdropt_path = backdrop ?? string.Empty,
                    };

                    await AddMovieAsync(movie);
                    added.Add(movie);
                }
            }

            return added;
        }

        public async Task<List<MovieRecommender.Application.DTOs.MovieSeedResultDto>> SeedPopularAsync(int count)
        {
            var results = new List<MovieRecommender.Application.DTOs.MovieSeedResultDto>();
            if (count <= 0) return results;

            var external = _externalMovieService;
            var movies = await external.GetPopularMoviesAsync(count);
            _logger.LogInformation("External returned {Count} movies for requested {Requested}", movies.Count, count);

            if (movies == null || movies.Count == 0)
            {
                _logger.LogWarning("No movies returned from TMDB for SeedPopularAsync. Check TMDB_API_KEY and network connectivity.");
                results.Add(new MovieRecommender.Application.DTOs.MovieSeedResultDto
                {
                    TmdbId = 0,
                    Title = string.Empty,
                    Success = false,
                    Message = "No movies returned from TMDB. Check TMDB_API_KEY and network connectivity."
                });

                return results;
            }

            foreach (var m in movies)
            {
                var seedResult = new MovieRecommender.Application.DTOs.MovieSeedResultDto
                {
                    TmdbId = m.TmdbId,
                    Title = m.Title
                };

                try
                {
                    var existing = await _movieRepository.GetMovieByTmdbIdAsync(m.TmdbId);
                    if (existing != null)
                    {
                        seedResult.MovieId = existing.Id;
                        seedResult.Success = false;
                        seedResult.Message = "Skipped: movie with same TMDB id already exists.";
                        seedResult.Embedding = existing.Embedding ?? new List<float>();
                        results.Add(seedResult);
                        continue;
                    }
                    var movie = new Movie
                    {
                        TmdbId = m.TmdbId,
                        Title = m.Title,
                        overview = m.Overview ?? string.Empty,
                        Genre = m.Genre ?? string.Empty,
                        Director = m.Director ?? string.Empty,
                        Rating = m.Rating ?? 0.0,
                        ReleaseYear = m.ReleaseYear,
                        poster_path = m.PosterPath ?? string.Empty,
                        backdropt_path = m.BackdropPath ?? string.Empty
                    };

                    movie.Embedding = await _embeddingService.GenerateEmbeddingAsync(movie.Title, movie.overview, movie.Genre);

                    await _movieRepository.AddMovieAsync(movie);

                    seedResult.MovieId = movie.Id;
                    seedResult.Success = true;
                    seedResult.Embedding = movie.Embedding ?? new List<float>();
                }
                catch (Exception ex)
                {
                    seedResult.Success = false;
                    seedResult.Message = ex.Message;
                }

                results.Add(seedResult);
            }

            return results;
        }

    }
}