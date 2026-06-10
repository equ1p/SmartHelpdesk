using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using SmartHelpdesk.Indexes;

namespace SmartHelpdesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController :ControllerBase
    {
        private readonly IDocumentStore _store;

        public AnalyticsController(IDocumentStore store)
        {
            _store = store;
        }

        [HttpGet("operators")]
        public async Task<IActionResult> GetOperatorPerformance()
        {
            using var session = _store.OpenAsyncSession();

            var results = await session
                .Query<Operators_Performance.Result, Operators_Performance>()
                .OrderByDescending(x => x.ClosedCount)
                .ToListAsync();

            return Ok(results);
        }
    }
}
