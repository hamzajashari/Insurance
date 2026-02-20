using Claims.Application.Interfaces;
using Claims.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

/// <summary>
/// Provides endpoints for managing insurance covers.
/// </summary>
[ApiController]
[Route("[controller]")]
public class CoversController : ControllerBase
{
    private readonly ICoverService _coverService;

    public CoversController(ICoverService coverService)
    {
        _coverService = coverService;
    }
    /// <summary>
    /// Computes the insurance premium for a given period and cover type.
    /// </summary>
    /// <param name="startDate">Insurance start date.</param>
    /// <param name="endDate">Insurance end date.</param>
    /// <param name="coverType">Type of cover.</param>
    /// <returns>Calculated premium.</returns>
    [HttpPost("compute")]
    public ActionResult ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        if (endDate < startDate)
            return BadRequest("End date must be after start date.");

        // Calculate insurance days (inclusive)
        int insuranceDays = (endDate.Date - startDate.Date).Days + 1; 
        decimal premium = _coverService.ComputePremium(coverType, insuranceDays);

        return Ok(premium);
    }

    /// <summary>
    /// Retrieves all covers.
    /// </summary>
    /// <returns>List of covers.</returns>
    [HttpGet]
    public async Task<ActionResult> GetAsync()
        => Ok(await _coverService.GetAllAsync());

    /// <summary>
    /// Retrieves a cover by its identifier.
    /// </summary>
    /// <param name="id">Cover identifier.</param>
    /// <returns>The cover if found; otherwise 404.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetAsync(string id)
    {
        var cover = await _coverService.GetByIdAsync(id);
        if (cover == null) return NotFound();
        return Ok(cover);
    }

    /// <summary>
    /// Creates a new cover and calculates its premium.
    /// </summary>
    /// <param name="cover">Cover data.</param>
    /// <returns>The created cover.</returns>
    [HttpPost]
    public async Task<ActionResult> CreateAsync(Cover cover)
        => Ok(await _coverService.CreateAsync(cover));

    /// <summary>
    /// Deletes a cover by its identifier.
    /// </summary>
    /// <param name="id">Cover identifier.</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        await _coverService.DeleteAsync(id);
        return NoContent();
    }
}