using Claims.Application.Events;
using Claims.Application.Interfaces;
using Claims.Application.Services;
using Claims.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Xunit;

namespace Claims.Tests
{
    public class ClaimsControllerTests
    {
        [Fact]
        public async Task Get_Claims()
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(_ =>
                { });

            var client = application.CreateClient();

            var response = await client.GetAsync("/Claims");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            Assert.False(string.IsNullOrEmpty(content));

        }
        [Fact]
        public async Task Create_Throw_DamageCost_Exceeds_Limit()
        {
            var repoMock = new Mock<IClaimRepository>().Object;
            var coverServiceMock = new Mock<ICoverService>().Object;
            var auditMock = new Mock<IAuditProducerService>().Object;
            var loggerMock = new Mock<ILogger<ClaimService>>().Object;

            var service = new ClaimService(
                repoMock,
                auditMock,
                coverServiceMock,
                loggerMock);

            var claim = new Claim
            {
                DamageCost = 200000
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateAsync(claim));
        }

        [Fact]
        public async Task Create_Throw_ClaimDate_Outside_Cover()
        {
            var repoMock = new Mock<IClaimRepository>().Object;
            var coverServiceMock = new Mock<ICoverService>();
            var loggerMock = new Mock<ILogger<ClaimService>>().Object;
            var auditMock = new Mock<IAuditProducerService>().Object;

            coverServiceMock
                .Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Cover
                {
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(10)
                });

            var service = new ClaimService(
                repoMock,
                auditMock,
                coverServiceMock.Object,
                loggerMock);

            var claim = new Claim
            {
                DamageCost = 1000,
                Created = DateTime.Today.AddDays(20),
                CoverId = "1"
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateAsync(claim));
        }

        [Fact]
        public async Task Create_Save_Claim_When_Valid()
        {
            var repoMock = new Mock<IClaimRepository>();
            var coverServiceMock = new Mock<ICoverService>();
            var auditMock = new Mock<IAuditProducerService>();
            var loggerMock = new Mock<ILogger<ClaimService>>().Object;

            coverServiceMock
                .Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Cover
                {
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(30)
                });

            var service = new ClaimService(
                repoMock.Object,
                auditMock.Object,
                coverServiceMock.Object,
                loggerMock);

            var claim = new Claim
            {
                DamageCost = 1000,
                Created = DateTime.Today,
                CoverId = "1"
            };

            await service.CreateAsync(claim);

            repoMock.Verify(x => x.AddAsync(It.IsAny<Claim>()), Times.Once);
            auditMock.Verify(x => x.EnqueueAsync(It.IsAny<AuditEvent>()), Times.Once);
        }
    }
}
