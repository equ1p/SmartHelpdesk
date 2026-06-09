using Raven.Client.Documents.Indexes;
using SmartHelpdesk.Models;

namespace SmartHelpdesk.Indexes
{
    public class Tickets_WithSlaStatus : AbstractIndexCreationTask<Ticket, Tickets_WithSlaStatus.Result>
    {
        public class Result
        {
            public string Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public TicketStatus Status { get; set; }
            public TicketPriority Priority { get; set; }
            public string ClientId { get; set; } = string.Empty;
            public string? OperatorId { get; set; }
            public DateTime CreatedAt { get; set; }

            public double ResolutionHours { get; set; }
            public bool IsSlaViolated { get; set; }
        }
        
        public Tickets_WithSlaStatus()
        {
            Map = tickets => from t in tickets
                             let endTime = t.ClosedAt ?? DateTime.UtcNow
                             let hours = (endTime - t.CreatedAt).TotalHours
                             select new Result
                             {
                                 Id = t.Id,
                                 Title = t.Title,
                                 Status = t.Status,
                                 Priority = t.Priority,
                                 ClientId = t.ClientId,
                                 OperatorId = t.OperatorId,
                                 CreatedAt = t.CreatedAt,

                                 ResolutionHours = hours,
                                 IsSlaViolated = hours > t.SlaHours && t.Status != TicketStatus.Closed
                             };
            StoreAllFields(FieldStorage.Yes);
        }

    }
}
