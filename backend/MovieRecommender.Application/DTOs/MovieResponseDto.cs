using System.Text.Json.Serialization;

namespace MovieRecommender.Application.DTOs
{
    public class MovieResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public float? Rating { get; set; }

        public string? PosterPath { get; set; }
        public string? BackdropPath { get; set; }
    }
}