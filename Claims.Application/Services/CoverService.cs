using Claims.Application.Events;
using Claims.Application.Interfaces;
using Claims.Domain;
using Claims.Infrastructure.DbContexts;
using Claims.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Claims.Application.Services
{
    public class CoverService : ICoverService
    {
        private readonly ICoverRepository _coverRepository;
        private readonly IAuditProducerService _auditService;
        private readonly ILogger<CoverService> _logger;

        private const decimal BaseDayRate = 1250m;
        public CoverService(
            ICoverRepository coverRepository,
            IAuditProducerService auditService,
            ILogger<CoverService> logger)
        {
            _coverRepository = coverRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<IEnumerable<Cover>> GetAllAsync()
        {
            return await _coverRepository.GetAllAsync();
        }

        public async Task<Cover?> GetByIdAsync(string id)
        {
            return await _coverRepository.GetByIdAsync(id);
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
            var days = (cover.EndDate.Date - cover.StartDate.Date).Days;

            if (days <= 0)
                throw new ArgumentException("End date must be after start date.");

            cover.Premium = ComputePremium(cover.Type, days);

            await _coverRepository.AddAsync(cover);

            await _auditService.EnqueueAsync(new AuditEvent { ClaimId = cover.Id, HttpRequestType = "POST" });

            _logger.LogInformation("Cover created with ID {Id}", cover.Id);

            return cover;
        }

        public async Task DeleteAsync(string id)
        {
            var cover = await GetByIdAsync(id);
            if (cover is null) return;

            await _coverRepository.DeleteAsync(id);

            await _auditService.EnqueueAsync(new AuditEvent { CoverId = id, HttpRequestType = "DELETE" });

            _logger.LogInformation("Cover deleted with ID {Id}", id);
        }

        /// <summary>
        /// Calculates premium based on duration and cover type.
        /// Applies discounts after 30 and 180 days.
        /// </summary>
        public decimal ComputePremium(CoverType coverType, int insuranceDays)
        {
            if (insuranceDays <= 0)
                throw new ArgumentException("Insurance period must be positive.");

            decimal dailyRate = BaseDayRate * GetTypeMultiplier(coverType);

            int firstPeriodDays = Math.Min(insuranceDays, 30);
            int secondPeriodDays = Math.Min(Math.Max(insuranceDays - 30, 0), 150);
            int thirdPeriodDays = Math.Max(insuranceDays - 180, 0);

            decimal firstPeriodPremium = firstPeriodDays * dailyRate;

            decimal secondPeriodPremium = secondPeriodDays * dailyRate * (1 - GetSecondPeriodDiscount(coverType));

            decimal thirdPeriodPremium = thirdPeriodDays * dailyRate * (1 - GetThirdPeriodDiscount(coverType));

            return firstPeriodPremium + secondPeriodPremium + thirdPeriodPremium;
        }
        private decimal GetTypeMultiplier(CoverType coverType)
        {
            return coverType switch
            {
                CoverType.Yacht => 1.10m,
                CoverType.PassengerShip => 1.20m,
                CoverType.Tanker => 1.50m,
                _ => 1.30m  // Covers CoverType.Other
            };
        }

        private decimal GetSecondPeriodDiscount(CoverType coverType)
        {
            return coverType == CoverType.Yacht ? 0.05m : 0.02m; // 150 days are discounted by 5% for Yacht and by 2% for other types
        }

        private decimal GetThirdPeriodDiscount(CoverType coverType)
        {
            return coverType == CoverType.Yacht ? 0.08m : 0.03m; // Remaining days are discounted by additional 3% for Yacht and by 1% for other types

        }
    }
}
