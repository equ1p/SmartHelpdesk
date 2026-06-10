namespace SmartHelpdesk.Dto
{
    public class CreateArticleRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string>? Tags { get; set; } = new();
        public float[]? VectorEmbedding { get; set; }
    }

    public class UpdateArticleRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public List<string>? Tags { get; set; }
        public float[]? VectorEmbedding { get; set; }
    }

    public class VectorSearchRequest
    {
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public int MaxResults { get; set; } = 5;
    }
}
