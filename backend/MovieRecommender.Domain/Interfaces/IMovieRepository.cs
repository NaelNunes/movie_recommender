using MovieRecommender.Domain.Entities;

namespace MovieRecommender.Domain.Interfaces
{
    public interface IMovieRepository
    {
        Task<IEnumerable<Entities.Movie>> GetAllMoviesAsync();
        Task<Entities.Movie?> GetMovieByIdAsync(int id);
        Task AddMovieAsync(Entities.Movie movie);
        Task UpdateMovieAsync(Entities.Movie movie);
        Task DeleteMovieAsync(int id);
        Task DeleteAllMoviesAsync();
    Task<Entities.Movie?> GetMovieByTmdbIdAsync(int tmdbId);
        Task<IEnumerable<Movie>> GetSimilarMoviesAsync(List<float> promptEmbedding, int limit = 5);
    }
}