using UglyToad.PdfPig;
using System.Text.RegularExpressions;

namespace RAG_Search.Services
{
    public static class PdfService
    {
        public static string ExtractText(string filePath)
        {
            using var pdf = PdfDocument.Open(filePath);
            var pages = pdf.GetPages().Select(p => p.Text);
            var text = string.Join("\n", pages);
            // Basic cleanup
            text = Regex.Replace(text, @"\s+", " ").Trim();
            return text;
        }

        public static List<string> ChunkText(string text, int chunkSizeWords = 300, int overlap = 50)
        {
            var words = text.Split(' ');
            var chunks = new List<string>();
            int i = 0;
            while (i < words.Length)
            {
                var take = Math.Min(chunkSizeWords, words.Length - i);
                var chunk = string.Join(" ", words.Skip(i).Take(take));
                chunks.Add(chunk);
                i += (chunkSizeWords - overlap);
            }
            return chunks;
        }
    }

}
