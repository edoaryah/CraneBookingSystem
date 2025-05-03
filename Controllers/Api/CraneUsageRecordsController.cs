using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.DTOs.Usage;
using AspnetCoreMvcFull.Filters;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Services.Usage;
using System.Security.Claims;

namespace AspnetCoreMvcFull.Controllers.Api
{
  [Route("api/[controller]")]
  [ApiController]
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class CraneUsageRecordsController : ControllerBase
  {
    private readonly ICraneUsageRecordService _usageService;
    private readonly ILogger<CraneUsageRecordsController> _logger;

    public CraneUsageRecordsController(ICraneUsageRecordService usageService, ILogger<CraneUsageRecordsController> logger)
    {
      _usageService = usageService;
      _logger = logger;
    }

    // GET: api/CraneUsageRecords
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CraneUsageRecordDto>>> GetAllUsageRecords()
    {
      try
      {
        var records = await _usageService.GetAllUsageRecordsAsync();
        return Ok(records);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting all usage records");
        return StatusCode(500, new { message = "An error occurred while retrieving usage records" });
      }
    }

    // GET: api/CraneUsageRecords/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CraneUsageRecordDto>> GetUsageRecord(int id)
    {
      try
      {
        var record = await _usageService.GetUsageRecordByIdAsync(id);
        return Ok(record);
      }
      catch (KeyNotFoundException)
      {
        return NotFound(new { message = $"Usage record with ID {id} not found" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting usage record with ID {RecordId}", id);
        return StatusCode(500, new { message = "An error occurred while retrieving the usage record" });
      }
    }

    // GET: api/CraneUsageRecords/Booking/5
    [HttpGet("Booking/{bookingId}")]
    public async Task<ActionResult<IEnumerable<CraneUsageRecordDto>>> GetUsageRecordsByBooking(int bookingId)
    {
      try
      {
        var records = await _usageService.GetUsageRecordsByBookingIdAsync(bookingId);
        return Ok(records);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting usage records for booking with ID {BookingId}", bookingId);
        return StatusCode(500, new { message = "An error occurred while retrieving the usage records" });
      }
    }

    // GET: api/CraneUsageRecords/Summary/5
    [HttpGet("Summary/{bookingId}")]
    public async Task<ActionResult<UsageSummaryDto>> GetUsageSummary(int bookingId)
    {
      try
      {
        var summary = await _usageService.GetUsageSummaryByBookingIdAsync(bookingId);
        return Ok(summary);
      }
      catch (KeyNotFoundException)
      {
        return NotFound(new { message = $"Booking with ID {bookingId} not found" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting usage summary for booking with ID {BookingId}", bookingId);
        return StatusCode(500, new { message = "An error occurred while retrieving the usage summary" });
      }
    }

    // GET: api/CraneUsageRecords/Subcategories/0
    [HttpGet("Subcategories/{category}")]
    public async Task<ActionResult<IEnumerable<UsageSubcategoryDto>>> GetSubcategories(UsageCategory category)
    {
      try
      {
        var subcategories = await _usageService.GetSubcategoriesByCategoryAsync(category);
        return Ok(subcategories);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting subcategories for category {Category}", category);
        return StatusCode(500, new { message = "An error occurred while retrieving subcategories" });
      }
    }

    // POST: api/CraneUsageRecords
    [HttpPost]
    public async Task<ActionResult<CraneUsageRecordDto>> CreateUsageRecord(CraneUsageRecordCreateDto recordDto)
    {
      try
      {
        string createdBy = GetCurrentUsername();
        var result = await _usageService.CreateUsageRecordAsync(recordDto, createdBy);

        return CreatedAtAction(nameof(GetUsageRecord), new { id = result.Id }, result);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(new { message = ex.Message });
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error creating usage record");
        return StatusCode(500, new { message = "An error occurred while creating the usage record" });
      }
    }

    // PUT: api/CraneUsageRecords/5
    [HttpPut("{id}")]
    public async Task<ActionResult<CraneUsageRecordDto>> UpdateUsageRecord(int id, CraneUsageRecordUpdateDto recordDto)
    {
      try
      {
        string updatedBy = GetCurrentUsername();
        var result = await _usageService.UpdateUsageRecordAsync(id, recordDto, updatedBy);

        return Ok(result);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(new { message = ex.Message });
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error updating usage record with ID {RecordId}", id);
        return StatusCode(500, new { message = "An error occurred while updating the usage record" });
      }
    }

    // DELETE: api/CraneUsageRecords/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUsageRecord(int id)
    {
      try
      {
        await _usageService.DeleteUsageRecordAsync(id);
        return NoContent();
      }
      catch (KeyNotFoundException)
      {
        return NotFound(new { message = $"Usage record with ID {id} not found" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error deleting usage record with ID {RecordId}", id);
        return StatusCode(500, new { message = "An error occurred while deleting the usage record" });
      }
    }

    // Helper method to get current username
    private string GetCurrentUsername()
    {
      return User.FindFirst("ldapuser")?.Value ?? "system";
    }
  }
}
