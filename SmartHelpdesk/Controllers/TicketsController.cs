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
    public class TicketsController : ControllerBase
    {
        private readonly IDocumentStore _store;

        public TicketsController(IDocumentStore store)
        {
            _store = store;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] TicketStatus? status,
            [FromQuery] TicketPriority? priority,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            using var session = _store.OpenAsyncSession();

            QueryStatistics stats;
            var query = session.Query<Ticket>()
                .Statistics(out stats);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);


        }
    }
}
