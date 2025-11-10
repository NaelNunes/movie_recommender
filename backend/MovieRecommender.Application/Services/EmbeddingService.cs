using OpenAI.Embeddings;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieRecommender.Application.Services
{
    public class EmbeddingService
    {
        private readonly EmbeddingClient _client;

        public EmbeddingService(string apiKey)
        {
            _client = new EmbeddingClient("text-embedding-3-small", apiKey);
        }

        public async Task<List<float>> GenerateEmbeddingAsync(string title, string description, string genre)
        {
            string textToEmbed = $"{title}. {description}. Genre: {genre}.";

            var result = await _client.GenerateEmbeddingAsync(textToEmbed);

            ReadOnlyMemory<float> vector = result.Value.ToFloats();

            return vector.ToArray().ToList();

        }

        public async Task<List<float>> GeneratePromptEmbeddingAsync(string prompt)
        {
            var result = await _client.GenerateEmbeddingAsync(prompt);

            ReadOnlyMemory<float> vector = result.Value.ToFloats();

            return vector.ToArray().ToList();
        }
    }
}