using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.ViewModels.CraneManagement;

namespace AspnetCoreMvcFull.Controllers.Api
{
  [Route("api/[controller]")]
  [ApiController]
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class CranesController : ControllerBase
  {
    private readonly ICraneService _craneService;
    private readonly ILogger<CranesController> _logger;

    public CranesController(ICraneService craneService, ILogger<CranesController> logger)
    {
      _craneService = craneService;
      _logger = logger;
    }

    // GET: api/Cranes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CraneViewModel>>> GetAllCranes()
    {
      try
      {
        var cranes = await _craneService.GetAllCranesAsync();
        return Ok(cranes);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting all cranes");
        return StatusCode(500, new { message = "An error occurred while retrieving cranes" });
      }
    }

    // GET: api/Cranes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CraneDetailViewModel>> GetCrane(int id)
    {
      try
      {
        var crane = await _craneService.GetCraneByIdAsync(id);
        return Ok(crane);
      }
      catch (KeyNotFoundException)
      {
        return NotFound(new { message = $"Crane with ID {id} not found" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting crane with ID {CraneId}", id);
        return StatusCode(500, new { message = "An error occurred while retrieving the crane" });
      }
    }

    // GET: api/Cranes/Breakdowns/5
    [HttpGet("Breakdowns/{craneId}")]
    public async Task<ActionResult<IEnumerable<BreakdownViewModel>>> GetCraneBreakdowns(int craneId)
    {
      try
      {
        var breakdowns = await _craneService.GetCraneBreakdownsAsync(craneId);
        return Ok(breakdowns);
      }
      catch (KeyNotFoundException)
      {
        return NotFound(new { message = $"Crane with ID {craneId} not found" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting breakdowns for crane with ID {CraneId}", craneId);
        return StatusCode(500, new { message = "An error occurred while retrieving crane breakdowns" });
      }
    }

    // GET: api/Cranes/Breakdowns
    [HttpGet("Breakdowns")]
    public async Task<ActionResult<IEnumerable<BreakdownHistoryViewModel>>> GetAllBreakdowns()
    {
      try
      {
        var breakdowns = await _craneService.GetAllBreakdownsAsync();
        return Ok(breakdowns);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting all breakdowns");
        return StatusCode(500, new { message = "An error occurred while retrieving breakdowns" });
      }
    }
  }
}
