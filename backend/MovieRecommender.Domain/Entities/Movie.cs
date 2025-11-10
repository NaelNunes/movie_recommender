namespace MovieRecommender.Domain.Entities
{
    public class Movie
    {
        public int Id { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string overview { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public string Director { get; set; } = string.Empty;
        public double Rating { get; set; }
        public string poster_path { get; set; } = string.Empty;
        public string backdropt_path { get; set; } = string.Empty;
        public List<float> Embedding { get; set; } = new();
    }
}