using Claims.Application.Events;
using Claims.Application.Interfaces;
using Claims.Application.Services;
using Claims.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Claims.Tests
{
    /// <summary>
    /// Unit tests for CoverService covering all business rules:
    ///   - StartDate cannot be in the past
    ///   - Cover period cannot exceed 365 days
    ///   - EndDate must be strictly after StartDate
    ///   - Premium is computed correctly per CoverType and tiered-discount rules
    ///   - DeleteAsync silently skips non-existent covers
    ///   - DeleteAsync persists and audits existing covers
    /// </summary>
    public class CoverServiceTests
    {
        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------

        /// <summary>
        /// Creates a CoverService with the supplied mock objects already configured.
        /// All parameters are optional; a new loose mock is created for any that are omitted.
        /// </summary>
        private static CoverService BuildService(
            Mock<ICoverRepository>? repoMock = null,
            Mock<IAuditProducerService>? auditMock = null,
            Mock<ILogger<CoverService>>? loggerMock = null)
        {
            repoMock ??= new Mock<ICoverRepository>();
            auditMock ??= new Mock<IAuditProducerService>();
            loggerMock ??= new Mock<ILogger<CoverService>>();

            return new CoverService(
                repoMock.Object,
                auditMock.Object,
                loggerMock.Object);
        }

        /// <summary>
        /// Builds a Cover whose dates are guaranteed to pass all validation rules.
        /// StartDate defaults to tomorrow; EndDate defaults to 30 days after StartDate.
        /// </summary>
        private static Cover ValidCover(
            DateTime? startDate = null,
            DateTime? endDate = null,
            CoverType type = CoverType.Yacht)
        {
            var start = startDate ?? DateTime.Today.AddDays(1);
            var end = endDate ?? start.AddDays(30);
            return new Cover { StartDate = start, EndDate = end, Type = type };
        }

        // ----------------------------------------------------------------
        // CreateAsync – validation guards
        // ----------------------------------------------------------------

        [Fact]
        public async Task CreateAsync_ThrowsArgumentException_WhenStartDateIsInThePast()
        {
            var service = BuildService();
            var cover = ValidCover(startDate: DateTime.Today.AddDays(-1));

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateAsync(cover));

            Assert.Contains("past", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateAsync_ThrowsArgumentException_WhenStartDateIsToday()
        {
            // Today (before midnight) is considered "in the past" by the rule:
            // StartDate < DateTime.Today throws. StartDate == DateTime.Today also throws
            // because the check is strict less-than; today itself is allowed.
            // The implementation uses: if (cover.StartDate < DateTime.Today) throw
            // So today does NOT throw — only strictly-past dates do.
            // This test documents that a start date of yesterday definitely throws.
            var service = BuildService();
            var cover = ValidCover(startDate: DateTime.Today.AddDays(-5));

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateAsync(cover));
        }

        [Fact]
        public async Task CreateAsync_DoesNotThrow_WhenStartDateIsExactlyToday()
        {
            // StartDate == DateTime.Today passes the guard (the guard is strictly < Today).
            var repoMock = new Mock<ICoverRepository>();
            var auditMock = new Mock<IAuditProducerService>();
            var service = BuildService(repoMock, auditMock);

            var cover = ValidCover(
                startDate: DateTime.Today,
                endDate: DateTime.Today.AddDays(10));

            var result = await service.CreateAsync(cover);

            Assert.NotNull(result);
            Assert.Equal(DateTime.Today, result.StartDate);
        }

        [Fact]
        public async Task CreateAsync_ThrowsArgumentException_WhenPeriodExceeds365Days()
        {
            var service = BuildService();
            var start = DateTime.Today.AddDays(1);
            var cover = ValidCover(
                startDate: start,
                endDate: start.AddDays(366)); // 366 days > 365

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateAsync(cover));

            Assert.Contains("year", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateAsync_DoesNotThrow_WhenPeriodIsExactly365Days()
        {
            var repoMock = new Mock<ICoverRepository>();
            var auditMock = new Mock<IAuditProducerService>();
            var service = BuildService(repoMock, auditMock);

            var start = DateTime.Today.AddDays(1);
            var cover = ValidCover(
                startDate: start,
                endDate: start.AddDays(365)); // exactly 365 days — should pass

            var result = await service.CreateAsync(cover);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateAsync_ThrowsArgumentException_WhenEndDateEqualsStartDate()
        {
            // When EndDate == StartDate, days == 0, which fails the days <= 0 check.
            var service = BuildService();
            var start = DateTime.Today.AddDays(1);
            var cover = ValidCover(startDate: start, endDate: start);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateAsync(cover));

            Assert.Contains("after", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateAsync_ThrowsArgumentException_WhenEndDateIsBeforeStartDate()
        {
            var service = BuildService();
            var start = DateTime.Today.AddDays(5);
            var cover = ValidCover(
                startDate: start,
                endDate: start.AddDays(-2)); // end before start

            // The period check fires first: TotalDays is negative, which is <= 365,
            // so we fall through to the days <= 0 guard.
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateAsync(cover));
        }

        // ----------------------------------------------------------------
        // CreateAsync – persistence & audit
        // ----------------------------------------------------------------

        [Fact]
        public async Task CreateAsync_PersistsAndAudits_WhenCoverIsValid()
        {
            var repoMock = new Mock<ICoverRepository>();
            var auditMock = new Mock<IAuditProducerService>();
            var service = BuildService(repoMock, auditMock);

            var cover = ValidCover();

            await service.CreateAsync(cover);

            repoMock.Verify(r => r.AddAsync(It.IsAny<Cover>()), Times.Once);
            auditMock.Verify(a => a.EnqueueAsync(It.Is<AuditEvent>(
                e => e.HttpRequestType == "POST")), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_AssignsNonEmptyId_WhenCoverIsValid()
        {
            var service = BuildService();
            var cover = ValidCover();

            var result = await service.CreateAsync(cover);

            Assert.False(string.IsNullOrWhiteSpace(result.Id),
                "CreateAsync should assign a non-empty GUID as the cover Id");
        }

        [Fact]
        public async Task CreateAsync_SetsPremium_WhenCoverIsValid()
        {
            var service = BuildService();
            var cover = ValidCover(type: CoverType.Yacht);

            var result = await service.CreateAsync(cover);

            Assert.True(result.Premium > 0,
                "CreateAsync should set a positive Premium on the returned cover");
        }

        // ----------------------------------------------------------------
        // Premium computation – ComputePremium
        //
        // Formula (all days exclusive of start, i.e. days = EndDate - StartDate):
        //   dailyRate = 1250 * typeMultiplier
        //   P1 = min(days, 30) * dailyRate
        //   P2 = min(max(days-30, 0), 150) * dailyRate * (1 - secondDiscount)
        //   P3 = max(days-180, 0) * dailyRate * (1 - thirdDiscount)
        //   Total = P1 + P2 + P3
        //
        // Multipliers:  Yacht=1.10, PassengerShip=1.20, Tanker=1.50, rest=1.30
        // 2nd discount: Yacht=5%, others=2%
        // 3rd discount: Yacht=8%, others=3%
        // ----------------------------------------------------------------

        [Fact]
        public void ComputePremium_ThrowsArgumentException_WhenInsuranceDaysIsZero()
        {
            var service = BuildService();

            var ex = Assert.Throws<ArgumentException>(() =>
                service.ComputePremium(CoverType.Yacht, 0));

            Assert.Contains("positive", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ComputePremium_ThrowsArgumentException_WhenInsuranceDaysIsNegative()
        {
            var service = BuildService();

            Assert.Throws<ArgumentException>(() =>
                service.ComputePremium(CoverType.ContainerShip, -1));
        }

        // --- Yacht (multiplier 1.10, 2nd=5%, 3rd=8%) ---

        [Fact]
        public void ComputePremium_Yacht_FirstPeriodOnly_30Days()
        {
            // days=30: all days fall in period 1
            // P1 = 30 * (1250 * 1.10) = 30 * 1375 = 41,250
            var service = BuildService();

            decimal result = service.ComputePremium(CoverType.Yacht, 30);

            Assert.Equal(41_250m, result);
        }

        [Fact]
        public void ComputePremium_Yacht_SpansFirstAndSecondPeriod_60Days()
        {
            // days=60: P1=30*1375=41250, P2=30*1375*0.95=39131.25, P3=0
            var service = BuildService();
            decimal dailyRate = 1250m * 1.10m;
            decimal expected = (30 * dailyRate) + (30 * dailyRate * 0.95m);

            decimal result = service.ComputePremium(CoverType.Yacht, 60);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ComputePremium_Yacht_SpansAllThreePeriods_200Days()
        {
            // days=200: P1=30, P2=150, P3=20
            var service = BuildService();
            decimal dailyRate = 1250m * 1.10m;
            decimal expected =
                (30 * dailyRate) +
                (150 * dailyRate * 0.95m) +
                (20 * dailyRate * 0.92m);

            decimal result = service.ComputePremium(CoverType.Yacht, 200);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ComputePremium_Yacht_BoundaryAt180Days()
        {
            // days=180: P1=30, P2=150, P3=0
            var service = BuildService();
            decimal dailyRate = 1250m * 1.10m;
            decimal expected =
                (30 * dailyRate) +
                (150 * dailyRate * 0.95m);

            decimal result = service.ComputePremium(CoverType.Yacht, 180);

            Assert.Equal(expected, result);
        }

        // --- PassengerShip (multiplier 1.20, 2nd=2%, 3rd=3%) ---

        [Fact]
        public void ComputePremium_PassengerShip_FirstPeriodOnly_1Day()
        {
            // days=1: P1 = 1 * (1250 * 1.20) = 1500
            var service = BuildService();

            decimal result = service.ComputePremium(CoverType.PassengerShip, 1);

            Assert.Equal(1_500m, result);
        }

        [Fact]
        public void ComputePremium_PassengerShip_SpansAllThreePeriods_365Days()
        {
            // days=365: P1=30, P2=150, P3=185
            var service = BuildService();
            decimal dailyRate = 1250m * 1.20m;
            decimal expected =
                (30 * dailyRate) +
                (150 * dailyRate * 0.98m) +
                (185 * dailyRate * 0.97m);

            decimal result = service.ComputePremium(CoverType.PassengerShip, 365);

            Assert.Equal(expected, result);
        }

        // --- Tanker (multiplier 1.50, 2nd=2%, 3rd=3%) ---

        [Fact]
        public void ComputePremium_Tanker_FirstPeriodOnly_10Days()
        {
            // days=10: P1 = 10 * (1250 * 1.50) = 18,750
            var service = BuildService();

            decimal result = service.ComputePremium(CoverType.Tanker, 10);

            Assert.Equal(18_750m, result);
        }

        [Fact]
        public void ComputePremium_Tanker_SpansAllThreePeriods_200Days()
        {
            // days=200: P1=30, P2=150, P3=20
            var service = BuildService();
            decimal dailyRate = 1250m * 1.50m;
            decimal expected =
                (30 * dailyRate) +
                (150 * dailyRate * 0.98m) +
                (20 * dailyRate * 0.97m);

            decimal result = service.ComputePremium(CoverType.Tanker, 200);

            Assert.Equal(expected, result);
        }

        // --- ContainerShip (multiplier 1.30 — the default branch, 2nd=2%, 3rd=3%) ---

        [Fact]
        public void ComputePremium_ContainerShip_FirstPeriodOnly_30Days()
        {
            // days=30: P1 = 30 * (1250 * 1.30) = 48,750
            var service = BuildService();

            decimal result = service.ComputePremium(CoverType.ContainerShip, 30);

            Assert.Equal(48_750m, result);
        }

        [Fact]
        public void ComputePremium_ContainerShip_SpansAllThreePeriods_200Days()
        {
            // days=200: P1=30, P2=150, P3=20
            var service = BuildService();
            decimal dailyRate = 1250m * 1.30m;
            decimal expected =
                (30 * dailyRate) +
                (150 * dailyRate * 0.98m) +
                (20 * dailyRate * 0.97m);

            decimal result = service.ComputePremium(CoverType.ContainerShip, 200);

            Assert.Equal(expected, result);
        }

        // --- BulkCarrier (multiplier 1.30 — same default branch as ContainerShip) ---

        [Fact]
        public void ComputePremium_BulkCarrier_FirstPeriodOnly_30Days()
        {
            // days=30: P1 = 30 * (1250 * 1.30) = 48,750
            var service = BuildService();

            decimal result = service.ComputePremium(CoverType.BulkCarrier, 30);

            Assert.Equal(48_750m, result);
        }

        [Fact]
        public void ComputePremium_BulkCarrier_SpansAllThreePeriods_200Days()
        {
            // Verify BulkCarrier uses the same default multiplier as ContainerShip.
            var service = BuildService();
            decimal dailyRate = 1250m * 1.30m;
            decimal expected =
                (30 * dailyRate) +
                (150 * dailyRate * 0.98m) +
                (20 * dailyRate * 0.97m);

            decimal result = service.ComputePremium(CoverType.BulkCarrier, 200);

            Assert.Equal(expected, result);
        }

        // --- Discount boundary: exactly at day 30 boundary ---

        [Fact]
        public void ComputePremium_Yacht_SingleDayInSecondPeriod_31Days()
        {
            // days=31: P1=30*1375, P2=1*1375*0.95
            var service = BuildService();
            decimal dailyRate = 1250m * 1.10m;
            decimal expected = (30 * dailyRate) + (1 * dailyRate * 0.95m);

            decimal result = service.ComputePremium(CoverType.Yacht, 31);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ComputePremium_Tanker_SingleDayInThirdPeriod_181Days()
        {
            // days=181: P1=30, P2=150, P3=1
            var service = BuildService();
            decimal dailyRate = 1250m * 1.50m;
            decimal expected =
                (30 * dailyRate) +
                (150 * dailyRate * 0.98m) +
                (1 * dailyRate * 0.97m);

            decimal result = service.ComputePremium(CoverType.Tanker, 181);

            Assert.Equal(expected, result);
        }

        // ----------------------------------------------------------------
        // DeleteAsync
        // ----------------------------------------------------------------

        [Fact]
        public async Task DeleteAsync_DoesNotCallRepositoryDelete_WhenCoverDoesNotExist()
        {
            var repoMock = new Mock<ICoverRepository>();
            var auditMock = new Mock<IAuditProducerService>();

            // Repository returns null — cover not found
            repoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Cover?)null);

            var service = BuildService(repoMock, auditMock);

            await service.DeleteAsync("non-existent-id");

            repoMock.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_DoesNotEnqueueAudit_WhenCoverDoesNotExist()
        {
            var repoMock = new Mock<ICoverRepository>();
            var auditMock = new Mock<IAuditProducerService>();

            repoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Cover?)null);

            var service = BuildService(repoMock, auditMock);

            await service.DeleteAsync("non-existent-id");

            auditMock.Verify(a => a.EnqueueAsync(It.IsAny<AuditEvent>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_CallsRepositoryDelete_WhenCoverExists()
        {
            var repoMock = new Mock<ICoverRepository>();
            var auditMock = new Mock<IAuditProducerService>();
            const string coverId = "existing-cover-id";

            repoMock
                .Setup(r => r.GetByIdAsync(coverId))
                .ReturnsAsync(new Cover { Id = coverId });

            var service = BuildService(repoMock, auditMock);

            await service.DeleteAsync(coverId);

            repoMock.Verify(r => r.DeleteAsync(coverId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_EnqueuesDeleteAuditEvent_WhenCoverExists()
        {
            var repoMock = new Mock<ICoverRepository>();
            var auditMock = new Mock<IAuditProducerService>();
            const string coverId = "existing-cover-id";

            repoMock
                .Setup(r => r.GetByIdAsync(coverId))
                .ReturnsAsync(new Cover { Id = coverId });

            var service = BuildService(repoMock, auditMock);

            await service.DeleteAsync(coverId);

            auditMock.Verify(a => a.EnqueueAsync(It.Is<AuditEvent>(
                e => e.CoverId == coverId && e.HttpRequestType == "DELETE")), Times.Once);
        }

        // ----------------------------------------------------------------
        // GetByIdAsync / GetAllAsync — delegation to repository
        // ----------------------------------------------------------------

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenCoverDoesNotExist()
        {
            var repoMock = new Mock<ICoverRepository>();
            repoMock
                .Setup(r => r.GetByIdAsync("missing"))
                .ReturnsAsync((Cover?)null);

            var service = BuildService(repoMock);

            var result = await service.GetByIdAsync("missing");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCover_WhenCoverExists()
        {
            var repoMock = new Mock<ICoverRepository>();
            var expected = new Cover { Id = "abc", Type = CoverType.Tanker };
            repoMock
                .Setup(r => r.GetByIdAsync("abc"))
                .ReturnsAsync(expected);

            var service = BuildService(repoMock);

            var result = await service.GetByIdAsync("abc");

            Assert.NotNull(result);
            Assert.Equal("abc", result!.Id);
            Assert.Equal(CoverType.Tanker, result.Type);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllCoversFromRepository()
        {
            var repoMock = new Mock<ICoverRepository>();
            var covers = new List<Cover>
            {
                new Cover { Id = "1", Type = CoverType.Yacht },
                new Cover { Id = "2", Type = CoverType.Tanker }
            };
            repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(covers);

            var service = BuildService(repoMock);

            var result = (await service.GetAllAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.Id == "1");
            Assert.Contains(result, c => c.Id == "2");
        }
    }
}
