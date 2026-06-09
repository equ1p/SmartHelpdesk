using Raven.Client.Documents.Indexes;
using SmartHelpdesk.Models;

namespace SmartHelpdesk.Indexes
{
    public class Articles_ByVector : AbstractIndexCreationTask<Article, Articles_ByVector.IndexEntry>
    {
        public class IndexEntry
        {
            public object Vector { get; set; } = null!;
        }

        public Articles_ByVector()
        {
            Map = articles => from a in articles
                              where a.VectorEmbedding != null
                              select new IndexEntry
                              {
                                  Vector = CreateVector(a.VectorEmbedding)
                              };

            SearchEngineType = SearchEngineType.Corax;
        }
    }
}
