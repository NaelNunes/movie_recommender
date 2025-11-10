namespace MovieRecommender.Application.DTOs
{
    public class MovieSeedResultDto
    {
        public int? MovieId { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<float> Embedding { get; set; } = new List<float>();
    }
}
