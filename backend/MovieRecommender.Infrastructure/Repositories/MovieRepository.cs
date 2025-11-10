using Microsoft.EntityFrameworkCore;
using MovieRecommender.Domain.Entities;
using MovieRecommender.Domain.Interfaces;
using MovieRecommender.Infrastructure.Data;

namespace MovieRecommender.Infrastructure.Repositories
{


    public class MovieRepository : IMovieRepository
    {

        private readonly AppDbContext _context;

        public MovieRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddMovieAsync(Movie movie)
        {
            try
            {
                await _context.Movies.AddAsync(movie);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding movie: {ex.Message}");
            }

        }

        public async Task DeleteMovieAsync(int id)
        {
            try
            {
                var movie = await _context.Movies.FindAsync(id);
                if (movie != null)
                {
                    _context.Movies.Remove(movie);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting movie: {ex.Message}");
            }

        }

        public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
        {
            try
            {
                List<Movie> movies = _context.Movies.ToList();
                return await Task.FromResult<IEnumerable<Movie>>(movies);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all movies: {ex.Message}");
            }
        }

        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            try
            {
                var movie = await _context.Movies.FindAsync(id);
                return movie;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving movie by ID: {ex.Message}");
            }
        }

        public async Task UpdateMovieAsync(Movie movie)
        {
            try
            {
                _context.Movies.Update(movie);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating movie: {ex.Message}");
            }
        }

        public async Task DeleteAllMoviesAsync()
        {
            try
            {
                var all = await _context.Movies.ToListAsync();
                if (all.Count > 0)
                {
                    _context.Movies.RemoveRange(all);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting all movies: {ex.Message}");
            }
        }

        public async Task<Movie?> GetMovieByTmdbIdAsync(int tmdbId)
        {
            try
            {
                return await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving movie by TMDB id: {ex.Message}");
            }
        }
        async Task<IEnumerable<Movie>> IMovieRepository.GetSimilarMoviesAsync(List<float> promptEmbedding, int limit)
        {
            var movies = await _context.Movies.ToListAsync();

            return movies
                .Select(m => new
                {
                    Movie = m,
                    Similarity = CosineSimilarity(promptEmbedding, m.Embedding)
                })
                .OrderByDescending(x => x.Similarity)
                .Take(limit)
                .Select(x => x.Movie)
                .ToList();
        }

        private static double CosineSimilarity(List<float> a, List<float> b)
        {
            if (a.Count != b.Count) return 0;
            double dot = 0, magA = 0, magB = 0;

            for (int i = 0; i < a.Count; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }

            return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
        }

    
    }


}