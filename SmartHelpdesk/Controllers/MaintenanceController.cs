using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;

namespace SmartHelpdesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IDocumentStore _store;

        public MaintenanceController(IDocumentStore store)
        {
            _store = store;
        }

        [HttpPost("close-stale-tickets")]
        public async Task<IActionResult> CloseStaleTickets([FromQuery] int staleDays = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-staleDays);

            var operation = await _store.Operations.SendAsync(new PatchByQueryOperation(
                new IndexQuery
                {
                    Query = @"
                        from Tickets
                        where Status != 'Closed' and CreatedAt < $cutoffDate
                        update{
                            this.Status = 'Closed';
                            this.ClosedAt = new Date().toISOString();
                        }",
                    QueryParameters = new Raven.Client.Parameters
                    {
                        {"cutoffDate", cutoffDate}
                    }
                }));

            await operation.WaitForCompletionAsync();

            return Ok(new
            {
                Message = $"Stale tickets older than {staleDays} days have been closed",
                CutoffDate = cutoffDate
            });
        }
    }
}
