namespace SmartHelpdesk.Models
{
    public class Article
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public float[]? VectorEmbedding {  get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
