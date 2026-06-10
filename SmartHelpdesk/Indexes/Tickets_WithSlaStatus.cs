using Raven.Client.Documents.Indexes;
using SmartHelpdesk.Models;

namespace SmartHelpdesk.Indexes
{
    public class Tickets_WithSlaStatus : AbstractIndexCreationTask<Ticket, Tickets_WithSlaStatus.Result>
    {
        public class Result
        {
            public string Id { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public TicketStatus Status { get; set; }
            public TicketPriority Priority { get; set; }
            public string ClientId { get; set; } = string.Empty;
            public string? OperatorId { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ClosedAt { get; set; }
            public int SlaHours { get; set; }
        }
        
        public Tickets_WithSlaStatus()
        {
            Map = tickets => from t in tickets
                             select new Result
                             {
                                 Id = t.Id,
                                 Title = t.Title,
                                 Status = t.Status,
                                 Priority = t.Priority,
                                 ClientId = t.ClientId,
                                 OperatorId = t.OperatorId,
                                 CreatedAt = t.CreatedAt,
                                 ClosedAt = t.ClosedAt,
                                 SlaHours = t.SlaHours
                             };
            StoreAllFields(FieldStorage.Yes);
        }

    }
}
