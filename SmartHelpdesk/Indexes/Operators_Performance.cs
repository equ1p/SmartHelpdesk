using Raven.Client.Documents.Indexes;
using SmartHelpdesk.Models;

namespace SmartHelpdesk.Indexes
{
    public class Operators_Performance : AbstractIndexCreationTask<Ticket, Operators_Performance.Result>
    {
        public class Result
        {
            public string? OperatorId { get; set; }
            public int ClosedCount { get; set; }
            public double TotalResolutionHours { get; set; }
            public double AverageResolutionHours { get; set; }
        }

        public Operators_Performance()
        {
            Map = tickets => from t in tickets
                             where t.Status == TicketStatus.Closed
                                && t.OperatorId != null
                                && t.ClosedAt != null
                             let hours = (t.ClosedAt.Value - t.CreatedAt).TotalHours
                             select new Result
                             {
                                 OperatorId = t.OperatorId,
                                 ClosedCount = 1,
                                 TotalResolutionHours = hours,
                                 AverageResolutionHours = hours
                             };

            Reduce = results => from r in results
                                group r by r.OperatorId into g
                                let totalHours = g.Sum(x => x.TotalResolutionHours)
                                let totalCount = g.Sum(x => x.ClosedCount)
                                select new Result
                                {
                                    OperatorId = g.Key,
                                    ClosedCount = totalCount,
                                    TotalResolutionHours = totalHours,
                                    AverageResolutionHours = totalCount > 0
                                        ? totalHours / totalCount
                                        : 0
                                };
        }
    }
}
