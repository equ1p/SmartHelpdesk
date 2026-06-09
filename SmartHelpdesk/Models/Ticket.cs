namespace SmartHelpdesk.Models
{
    public class Ticket
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string? OperatorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int SlaHours { get; set; } = 25;
    }

    public enum TicketStatus
    {
        Open,
        InProgress,
        Closed
    }

    public enum TicketPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}
