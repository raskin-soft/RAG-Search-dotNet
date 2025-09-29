using Microsoft.Data.SqlClient;
using RAG_Search.Models;
using System.Text;

namespace RAG_Search.Services
{
    public class SqlChunkLoaderService
    {
        private readonly OpenAIEmbeddingService _embeddingService;
        private readonly string _connectionString;

        public SqlChunkLoaderService(OpenAIEmbeddingService embeddingService, IConfiguration config)
        {
            _embeddingService = embeddingService;
            _connectionString = config["ConnectionStrings:DefaultConnection"];
        }

        public string LoadChunksFromSql()
        {
            var mergedText = new StringBuilder();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("SELECT columnname FROM tablename", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var text = reader.GetString(0);
                mergedText.Append(text).Append(' ');
            }

            return mergedText.ToString().Trim();
        }

    }
}
