using Claims.Application.Interfaces;
using Claims.Domain;
using Claims.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Claims.Application.Services
{
    public class CoverService : ICoverService
    {
        private readonly ClaimsContext _claimsContext;
        private readonly IAuditService _auditService;
        private readonly ILogger<CoverService> _logger;

        public CoverService(
            ClaimsContext claimsContext,
            IAuditService auditService,
            ILogger<CoverService> logger)
        {
            _claimsContext = claimsContext;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<IEnumerable<Cover>> GetAllAsync()
        {
            return await _claimsContext.Covers.ToListAsync();
        }

        public async Task<Cover?> GetByIdAsync(string id)
        {
            return await _claimsContext.Covers
                .Where(c => c.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<Cover> CreateAsync(Cover cover)
        {
            // Validate start date
            if (cover.StartDate < DateTime.Today)
                throw new ArgumentException("Cover start date cannot be in the past");

            // Validate maximum period
            if ((cover.EndDate - cover.StartDate).TotalDays > 365)
                throw new ArgumentException("Cover period cannot exceed 1 year");

            cover.Id = Guid.NewGuid().ToString();

            // Business rule: calculate premium here
            cover.Premium = ComputePremium(cover.StartDate, cover.EndDate, cover.Type);

            _claimsContext.Covers.Add(cover);
            await _claimsContext.SaveChangesAsync();

            await _auditService.AuditCoverAsync(cover.Id, "POST");

            _logger.LogInformation("Cover created with ID {Id}", cover.Id);

            return cover;
        }

        public async Task DeleteAsync(string id)
        {
            var cover = await GetByIdAsync(id);
            if (cover is null) return;

            _claimsContext.Covers.Remove(cover);
            await _claimsContext.SaveChangesAsync();

            await _auditService.AuditCoverAsync(id, "DELETE");

            _logger.LogInformation("Cover deleted with ID {Id}", id);
        }

        /// <summary>
        /// Calculates premium based on duration and cover type.
        /// Applies discounts after 30 and 180 days.
        /// </summary>
        public decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
        {
            var multiplier = coverType switch
            {
                CoverType.Yacht => 1.1m,
                CoverType.PassengerShip => 1.2m,
                CoverType.Tanker => 1.5m,
                _ => 1.3m
            };

            var premiumPerDay = 1250 * multiplier;
            var insuranceLength = (endDate - startDate).TotalDays;
            var totalPremium = 0m;

            for (var i = 0; i < insuranceLength; i++)
            {
                if (i < 30)
                    totalPremium += premiumPerDay;
                else if (i < 180)
                    totalPremium += coverType == CoverType.Yacht
                        ? premiumPerDay * 0.95m
                        : premiumPerDay * 0.98m;
                else
                    totalPremium += coverType == CoverType.Yacht
                        ? premiumPerDay * 0.92m
                        : premiumPerDay * 0.97m;
            }

            return totalPremium;
        }
    }
}
