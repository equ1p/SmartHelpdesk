using SmartHelpdesk.Models;

namespace SmartHelpdesk.Dto
{
    public class CreateTicketRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketPriority Priority { get; set; }
        public string ClientId { get; set; } = string.Empty;
    }

    public class UpdateTicketRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TicketStatus? Status { get; set; }
        public TicketPriority? Priority { get; set; }
        public string? OperatorId { get; set; }
    }

    public class TicketDetailsResponse
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int SlaHours { get; set; }

        public string ClientId { get; set; } = string.Empty;
        public string? ClientName { get; set; }
        public string? OperatorId { get; set; }
        public string? OperatorName { get; set; }
    }
}

