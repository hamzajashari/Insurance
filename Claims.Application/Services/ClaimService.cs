using Claims.Application.Events;
using Claims.Application.Interfaces;
using Claims.Domain;
using Claims.Infrastructure;
using Claims.Infrastructure.DbContexts;
using Microsoft.Extensions.Logging;

namespace Claims.Application.Services
{
    /// <summary>
    /// Handles business logic related to claims.
    /// </summary>
    public class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IAuditService _auditService;
        private readonly ICoverService _coverService;
        private readonly ILogger<ClaimService> _logger;

        public ClaimService(
            IClaimRepository claimRepository,
            IAuditService auditService,
            ICoverService coverService,
            ILogger<ClaimService> logger)
        {
            _claimRepository = claimRepository;
            _auditService = auditService;
            _coverService = coverService;
            _logger = logger;
        }

        public async Task<IEnumerable<Claim>> GetAllAsync()
        {
            return await _claimRepository.GetAllAsync();
        }

        public async Task<Claim?> GetByIdAsync(string id)
        {
            return await _claimRepository.GetByIdAsync(id);
        }

        public async Task<Claim> CreateAsync(Claim claim)
        {
            // Validate damage cost
            if (claim.DamageCost > 100_000)
                throw new ArgumentException("Damage cost cannot exceed 100,000");

            // Validate Created date within related Cover
            var cover = await _coverService.GetByIdAsync(claim.CoverId);
            if (cover == null)
                throw new ArgumentException("Related cover not found");

            if (claim.Created < cover.StartDate || claim.Created > cover.EndDate)
                throw new ArgumentException("Claim date must be within the cover period");

            claim.Id = Guid.NewGuid().ToString();

            await _claimRepository.AddAsync(claim);

            _auditService.Enqueue(new AuditEvent { ClaimId = claim.Id, HttpRequestType = "POST" });

            _logger.LogInformation("Claim created with ID {Id}", claim.Id);

            return claim;
        }

        public async Task DeleteAsync(string id)
        {
            _auditService.Enqueue(new AuditEvent { ClaimId = id, HttpRequestType = "DELETE" });

            await _claimRepository.DeleteAsync(id);

            _logger.LogInformation("Claim deleted with ID {Id}", id);
        }
    }
}