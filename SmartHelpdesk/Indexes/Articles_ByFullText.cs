using Raven.Client.Documents.Indexes;
using SmartHelpdesk.Models;

namespace SmartHelpdesk.Indexes
{
    public class Articles_ByFullText : AbstractIndexCreationTask<Article, Articles_ByFullText.Result>
    {
        public class Result
        {
            public string SearchText { get; set; } = string.Empty;
            public string[] Tags { get; set; } = Array.Empty<string>();
        }

        public Articles_ByFullText()
        {
            Map = articles => from a in articles
                              select new Result
                              {
                                  SearchText = a.Title + " " + a.Content,
                                  Tags = a.Tags.ToArray()
                              };
            Index(x => x.SearchText, FieldIndexing.Search);
            Analyze(x => x.SearchText, "StandardAnalyzer");
        }
    }
}
