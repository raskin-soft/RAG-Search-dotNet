using Microsoft.AspNetCore.Mvc;
using RAG_Search.Services;

namespace RAG_Search.Controllers
{
    [Route("rag")]
    public class RagController : Controller
    {
        private readonly OpenAIEmbeddingService _embedding;
        private readonly QdrantService _qdrant;
        private readonly RagQueryService _ragQuery;
        private readonly IConfiguration _config;
        private readonly SqlChunkLoaderService _sqlChunkLoaderService;


        public RagController(
            OpenAIEmbeddingService embedding,
            QdrantService qdrant,
            RagQueryService ragQuery,
            IConfiguration config,
            SqlChunkLoaderService sqlChunkLoaderService)

        {
            _embedding = embedding;
            _qdrant = qdrant;
            _ragQuery = ragQuery;
            _config = config;
            _sqlChunkLoaderService = sqlChunkLoaderService;

        }

        [HttpGet("index")]
        public async Task<IActionResult> IndexAsync()
        {
            var allDocs = await _qdrant.GetAllUploadedDocumentsAsync();

            ViewBag.AllDocuments = allDocs;

            return View();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "Please select a PDF file.";
                return View("Index");
            }

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            Directory.CreateDirectory(uploads);

            var path = Path.Combine(uploads, Guid.NewGuid().ToString() + "_" + file.FileName);
            using (var fs = new FileStream(path, FileMode.Create))
                await file.CopyToAsync(fs);

            // Step 1: Extract text and chunk
            var text = PdfService.ExtractText(path);
            int chunkSize = int.Parse(_config["Rag:ChunkSizeWords"] ?? "300");
            int overlap = int.Parse(_config["Rag:ChunkOverlapWords"] ?? "50");
            var chunks = PdfService.ChunkText(text, chunkSize, overlap);

            var docId = Guid.NewGuid();

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];

                // Step 2: Get embedding asynchronously
                var embList = await _embedding.GetEmbeddingAsync(chunk);
                var embArray = embList.ToArray(); // convert List<float> -> float[]

                // Step 3: Prepare payload
                var pointId = $"{docId}-{i}";

                var payload = new Dictionary<string, object>
                {
                    ["documentId"] = docId.ToString(),
                    ["chunkIndex"] = i,
                    ["preview"] = chunk.Length > 200 ? chunk.Substring(0, 200) : chunk,
                    ["fileName"] = file.FileName
                };

                // Step 4: Upsert chunk to Qdrant
                await _qdrant.UpsertChunkAsync(pointId, embArray, payload);
            }

            ViewBag.Message = $"Uploaded {file.FileName} and indexed {chunks.Count} chunks.";
            //return RedirectToAction("Index");
            return View("Index");
        }


        [HttpPost("LoadDBData")]
        public async Task<IActionResult> LoadDBData()
        {

            // Step 1: Extract text and chunk
            var text = _sqlChunkLoaderService.LoadChunksFromSql();
            int chunkSize = int.Parse(_config["Rag:ChunkSizeWords"] ?? "300");
            int overlap = int.Parse(_config["Rag:ChunkOverlapWords"] ?? "50");
            var chunks = PdfService.ChunkText(text, chunkSize, overlap);

            var docId = Guid.NewGuid();

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];

                // Step 2: Get embedding asynchronously
                var embList = await _embedding.GetEmbeddingAsync(chunk);
                var embArray = embList.ToArray(); // convert List<float> -> float[]

                // Step 3: Prepare payload
                var pointId = $"{docId}-{i}";

                var payload = new Dictionary<string, object>
                {
                    ["documentId"] = docId.ToString(),
                    ["chunkIndex"] = i,
                    ["preview"] = chunk.Length > 200 ? chunk.Substring(0, 200) : chunk,

                    ["fileName"] = "SQL Database"
                };

                // Step 4: Upsert chunk to Qdrant
                await _qdrant.UpsertChunkAsync(pointId, embArray, payload);
            }


            ViewBag.Message = $"Uploaded SQL Database and indexed {chunks.Count} chunks.";
            //return RedirectToAction("Index");
            return View("Index");
        }


        [HttpPost("query")]
        public async Task<IActionResult> Query(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                ViewBag.Message = "Please enter a question.";
                return View("Index");
            }

            var allDocs = await _qdrant.GetAllUploadedDocumentsAsync();
            ViewBag.AllDocuments = allDocs;

            var (answer, files) = await _ragQuery.AnswerAsync(question);
            ViewBag.SourceDocuments = files[0].ToString(); ;
            ViewBag.Question = question;
            ViewBag.Answer = answer;

            //return RedirectToAction("Index");
            return View("Index");
        }
    }

}
