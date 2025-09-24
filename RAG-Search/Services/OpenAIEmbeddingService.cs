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


            // VERSION - Official SDK
            //You’d need to:
            //-Remove the current SDK
            //- Install the official one:
            //dotnet add package OpenAI --version 1.10.0
            //- Use this structure:
            //var client = new OpenAIClient("your-api-key");
            //var result = await client.GetEmbeddingsAsync(input, model: Model.TextEmbeddingAda002);
            //var vector = result.Data[0].Embedding.ToArray();

        }
    }
}
