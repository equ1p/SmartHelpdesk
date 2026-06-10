using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Client.Documents.Linq;
using SmartHelpdesk.Dto;
using SmartHelpdesk.Models;
using SmartHelpdesk.Indexes;

namespace SmartHelpdesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly IDocumentStore _store;

        public ArticlesController(IDocumentStore store)
        {
            _store = store;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            using var session = _store.OpenAsyncSession();

            QueryStatistics stats;
            var results = await session.Query<Article>()
                .Statistics(out stats)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PageResponse<Article>
            {
                Items = results,
                TotalCount = (int)stats.TotalResults,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{*id}")]
        public async Task<IActionResult> GetById(string id)
        {
            using var session = _store.OpenAsyncSession();
            var article = await session.LoadAsync<Article>(id);

            if (article == null)
                return NotFound();

            return Ok(article);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateArticleRequest request)
        {
            using var session = _store.OpenAsyncSession();

            var article = new Article
            {
                Title = request.Title,
                Content = request.Content,
                Tags = request.Tags,
                VectorEmbedding = request.VectorEmbedding,
                CreatedAt = DateTime.UtcNow
            };

            await session.StoreAsync(article);
            await session.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = article.Id }, article);
        }

        [HttpPut("{*id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateArticleRequest request)
        {
            using var session = _store.OpenAsyncSession();
            var article = await session.LoadAsync<Article>(id);

            if (article == null)
                return NotFound();

            if(request.Title != null) article.Title = request.Title;
            if(request.Content != null) article.Content = request.Content;
            if(request.Tags != null) article.Tags = request.Tags;
            if(request.VectorEmbedding != null) article.VectorEmbedding = request.VectorEmbedding;

            await session.SaveChangesAsync();
            return Ok(article);
        }

        [HttpDelete("{*id}")]
        public async Task<IActionResult> Delete(string id)
        {
            using var session = _store.OpenAsyncSession();
            var article = await session.LoadAsync<Article>(id);

            if (article == null)
                return NotFound();

            session.Delete(article);
            await session.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> FullTextSearch(
            [FromQuery] string q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Query parameter 'q' is required");

            using var session = _store.OpenAsyncSession();

            QueryStatistics stats;
            var results = await session
                .Query<Articles_ByFullText.Result, Articles_ByFullText>()
                .Statistics(out stats)
                .Search(x => x.SearchText, q)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OfType<Article>()
                .ToListAsync();

            return Ok(new PageResponse<Article>
            {
                Items = results,
                TotalCount = (int)stats.TotalResults,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpPost("vector-search")]
        public async Task<IActionResult> VectorSearch([FromBody] VectorSearchRequest request)
        {
            if (request.Embedding == null || request.Embedding.Length == 0)
                return BadRequest("Embedding vector is required");

            using var session = _store.OpenAsyncSession();

            var results = await session
                .Query<Articles_ByVector.IndexEntry, Articles_ByVector>()
                .VectorSearch(
                    field => field.WithField(x => x.Vector),
                    queryVector => queryVector.ByEmbedding(request.Embedding)
                )
                .OfType<Article>()
                .Take(request.MaxResults)
                .ToListAsync();

            return Ok(results);
        }

    }
}
