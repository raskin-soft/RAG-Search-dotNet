using OpenAI.Embeddings;

namespace RAG_Search.Services
{
    public class OpenAIEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly EmbeddingClient _client;

        public OpenAIEmbeddingService(IConfiguration config, HttpClient httpClient)
        {
            var apiKey = config["OpenAI:ApiKey"];
            
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException("OpenAI API key is missing in configuration.");

            _client = new EmbeddingClient("text-embedding-3-small", apiKey);
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<float>> GetEmbeddingAsync(string text)
        {
            // VERSION - Community SDK 
            OpenAIEmbedding embedding = await _client.GenerateEmbeddingAsync(text);
            float[] vector = embedding.ToFloats().ToArray();
            return vector.ToList();

        }
    }
}
