using Claims.Domain.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Claims.Application.Events
{
    public class AuditEvent
    {
        public string? ClaimId { get; set; }
        public string? CoverId { get; set; }
        public string HttpRequestType { get; set; } = default!;
        public DateTime Created { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Converts the event to a database entity for persistence
        /// </summary>
        public object ToEntity()
        {
            if (ClaimId != null)
            {
                return new ClaimAudit
                {
                    ClaimId = ClaimId,
                    HttpRequestType = HttpRequestType,
                    Created = Created
                };
            }

            if (CoverId != null)
            {
                return new CoverAudit
                {
                    CoverId = CoverId,
                    HttpRequestType = HttpRequestType,
                    Created = Created
                };
            }

            throw new InvalidOperationException("AuditEvent must have either ClaimId or CoverId");
        }
    }
}
