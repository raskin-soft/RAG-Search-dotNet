namespace RAG_Search.Models
{
    public class Document
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FileName { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public long Size { get; set; }
    }
}
