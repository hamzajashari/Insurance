using Claims.Application.Interfaces;
using Claims.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimService _claimService;

        public ClaimsController(IClaimService claimService)
        {
            _claimService = claimService;
        }
        /// <summary>
        /// Retrieves all claims.
        /// </summary>
        /// <returns>List of claims.</returns>
        [HttpGet]
        public async Task<IEnumerable<Claim>> GetAsync()
            => await _claimService.GetAllAsync();

        /// <summary>
        /// Retrieves a claim by its identifier.
        /// </summary>
        /// <param name="id">The claim identifier.</param>
        /// <returns>The claim if found; otherwise 404.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Claim>> GetAsync(string id)
        {
            var claim = await _claimService.GetByIdAsync(id);
            if (claim == null)
                return NotFound();

            return Ok(claim);
        }
        /// <summary>
        /// Creates a new claim.
        /// </summary>
        /// <param name="claim">Claim data.</param>
        /// <returns>The created claim.</returns>
        [HttpPost]
        public async Task<ActionResult> CreateAsync(Claim claim)
        {
            var created = await _claimService.CreateAsync(claim);
            return Ok(created);
        }
        /// <summary>
        /// Deletes a claim by its identifier.
        /// </summary>
        /// <param name="id">The claim identifier.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            await _claimService.DeleteAsync(id);
            return NoContent();
        }
    }
}