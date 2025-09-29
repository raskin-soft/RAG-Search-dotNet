using System.Text;

namespace RAG_Search.Services
{
    public class RagQueryService
    {
        //private readonly OllamaEmbeddingService _embedding;
        private readonly OpenAIEmbeddingService _embedding;
        private readonly QdrantService _qdrant;
        private readonly IConfiguration _cfg;
        private readonly HttpClient _http;

        public RagQueryService(OpenAIEmbeddingService embedding, QdrantService qdrant, IConfiguration cfg, IHttpClientFactory factory)
        {
            _embedding = embedding;
            _qdrant = qdrant;
            _cfg = cfg;
            _http = factory.CreateClient();
        }

        //public async Task<string> AnswerAsync(string query, int topK = 5)
            public async Task<(string Answer, List<string> FileNames)> AnswerAsync(string query, int topK = 5)

        {
            // Step 1: Get query embedding
            var qEmb = await _embedding.GetEmbeddingAsync(query);

            // Step 2: Search Qdrant
            var hits = await _qdrant.SearchAsync(qEmb.ToArray(), topK);

            // Step 3: Build context from chunks
            var contextBuilder = new StringBuilder();

            var fileNames = new HashSet<string>();

            foreach (var (chunk, score, payload) in hits)
            {
                var fileName = payload.TryGetValue("fileName", out var f) ? f.ToString() : "Unknown Document";
                fileNames.Add(fileName);

                // You can also use payload["documentId"], ["chunkIndex"], etc.
                if (payload.TryGetValue("preview", out var preview))
                {

                    contextBuilder.AppendLine($"[{fileName}] {chunk}");
                    contextBuilder.AppendLine("\n---\n");
                }
                else
                {
                    // fallback to chunk text if preview not in payload
                    contextBuilder.AppendLine(chunk);
                    contextBuilder.AppendLine("\n---\n");
                }
            }

            // Step 4: Build prompt for OpenAI
            var prompt = $"You are a helpful assistant. Use the provided context to answer the question.\n\nContext:\n{contextBuilder}\n\nQuestion: {query}\nAnswer concisely and cite relevant context chunk indices if possible.";

            // Step 5: Call OpenAI LLM to generate answer
            var answer = await CallOpenAIAsync(prompt);

            //return answer;

            return (answer, fileNames.ToList());
        }


        private async Task<string> CallOpenAIAsync(string prompt)
        {
            var apiKey = _cfg["OpenAI:ApiKey"];
            
            var model = _cfg["OpenAI:Model"] ?? "gpt-4o-mini";
            var req = new
            {
                model = model,
                messages = new[] {
                new { role = "system", content = "You are a helpful assistant." },
                new { role = "user", content = prompt }
            },
                max_tokens = 800
            };

            var reqJson = Newtonsoft.Json.JsonConvert.SerializeObject(req);
            using var message = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(reqJson, System.Text.Encoding.UTF8, "application/json")
            };
            message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var resp = await _http.SendAsync(message);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadAsStringAsync();
            dynamic j = Newtonsoft.Json.JsonConvert.DeserializeObject(body);
            string content = j.choices[0].message.content;
            return content;
        }
    }

}
