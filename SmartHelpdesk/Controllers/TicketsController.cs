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

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            var results = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PageResponse<Ticket>
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

            var ticket = await session
                .Include<Ticket>(t => t.ClientId)
                .Include<Ticket>(t => t.OperatorId!)
                .LoadAsync<Ticket>(id);

            if (ticket == null)
                return NotFound();

            var client = await session.LoadAsync<User>(ticket.ClientId);
            var operatorUser = ticket.OperatorId != null
                ? await session.LoadAsync<User>(ticket.OperatorId)
                : null;

            return Ok(new TicketDetailsResponse
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                ClosedAt = ticket.ClosedAt,
                SlaHours = ticket.SlaHours,
                ClientId = ticket.ClientId,
                ClientName = client?.Name,
                OperatorId = ticket.OperatorId,
                OperatorName = operatorUser?.Name
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTicketRequest request)
        {
            using var session = _store.OpenAsyncSession();

            var ticket = new Ticket
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                Status = TicketStatus.Open,
                ClientId = request.ClientId,
                CreatedAt = DateTime.UtcNow,
            };

            await session.StoreAsync(ticket);
            await session.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        }

        [HttpPut("{*id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateTicketRequest request)
        {
            using var session = _store.OpenAsyncSession();
            var ticket = await session.LoadAsync<Ticket>(id);

            if (ticket == null) 
                return NotFound();

            if (request.Title != null) ticket.Title = request.Title;
            if (request.Description != null) ticket.Description = request.Description;
            if (request.Priority.HasValue) ticket.Priority = request.Priority.Value;
            if (request.OperatorId != null) ticket.OperatorId = request.OperatorId;

            if (request.Status.HasValue)
            {
                ticket.Status = request.Status.Value;

                if (request.Status.Value == TicketStatus.Closed && ticket.ClosedAt == null)
                    ticket.ClosedAt = DateTime.UtcNow;
            }

            await session.SaveChangesAsync();
            return Ok(ticket);
        }

        [HttpGet("sla-report")] 
        public async Task<IActionResult> GetSlaReport([FromQuery] bool? violatedOnly)
        {
            using var session = _store.OpenAsyncSession();

            var query = session.Query<Tickets_WithSlaStatus.Result, Tickets_WithSlaStatus>();

            var results = await query
                .ProjectInto<Tickets_WithSlaStatus.Result>()
                .ToListAsync();

            if (violatedOnly == true)
            {
                var now = DateTime.UtcNow;
                results = results.Where(x =>
                {
                    var endTime = x.ClosedAt ?? now;
                    var hours = (endTime - x.CreatedAt).TotalHours;
                    return hours > x.SlaHours && x.Status != TicketStatus.Closed;
                }).ToList();
            }

            return Ok(results);
        }
    }
}
