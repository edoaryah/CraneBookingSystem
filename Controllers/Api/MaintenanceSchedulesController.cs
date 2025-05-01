// 7. Buat controller untuk MaintenanceSchedule API

// Controllers/Api/MaintenanceSchedulesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AspnetCoreMvcFull.Controllers.Api
{
  [Route("api/[controller]")]
  [ApiController]
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class MaintenanceSchedulesController : ControllerBase
  {
    private readonly IMaintenanceScheduleService _maintenanceService;

    public MaintenanceSchedulesController(IMaintenanceScheduleService maintenanceService)
    {
      _maintenanceService = maintenanceService;
    }

    // GET: api/MaintenanceSchedules
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MaintenanceScheduleDto>>> GetMaintenanceSchedules()
    {
      var schedules = await _maintenanceService.GetAllMaintenanceSchedulesAsync();
      return Ok(schedules);
    }

    // GET: api/MaintenanceSchedules/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MaintenanceScheduleDetailDto>> GetMaintenanceSchedule(int id)
    {
      var schedule = await _maintenanceService.GetMaintenanceScheduleByIdAsync(id);
      return Ok(schedule);
    }

    // GET: api/MaintenanceSchedules/Crane/5
    [HttpGet("Crane/{craneId}")]
    public async Task<ActionResult<IEnumerable<MaintenanceScheduleDto>>> GetMaintenanceSchedulesByCrane(int craneId)
    {
      var schedules = await _maintenanceService.GetMaintenanceSchedulesByCraneIdAsync(craneId);
      return Ok(schedules);
    }

    // POST: api/MaintenanceSchedules
    [HttpPost]
    public async Task<ActionResult<MaintenanceScheduleDetailDto>> CreateMaintenanceSchedule(MaintenanceScheduleCreateDto scheduleDto)
    {
      var result = await _maintenanceService.CreateMaintenanceScheduleAsync(scheduleDto);
      return CreatedAtAction(nameof(GetMaintenanceSchedule), new { id = result.Id }, result);
    }

    // PUT: api/MaintenanceSchedules/5
    [HttpPut("{id}")]
    public async Task<ActionResult<MaintenanceScheduleDetailDto>> UpdateMaintenanceSchedule(int id, MaintenanceScheduleUpdateDto scheduleDto)
    {
      var result = await _maintenanceService.UpdateMaintenanceScheduleAsync(id, scheduleDto);
      return Ok(result);
    }

    // DELETE: api/MaintenanceSchedules/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMaintenanceSchedule(int id)
    {
      await _maintenanceService.DeleteMaintenanceScheduleAsync(id);
      return NoContent();
    }

    // Endpoint untuk pengecekan konflik shift dengan maintenance
    // GET: api/MaintenanceSchedules/CheckShiftConflict?craneId=1&date=2025-04-01&shiftDefinitionId=2
    [HttpGet("CheckShiftConflict")]
    public async Task<ActionResult<bool>> CheckShiftConflict(
        [FromQuery] int craneId,
        [FromQuery] DateTime date,
        [FromQuery] int shiftDefinitionId,
        [FromQuery] int? excludeMaintenanceId = null)
    {
      var hasConflict = await _maintenanceService.IsShiftMaintenanceConflictAsync(
          craneId, date, shiftDefinitionId, excludeMaintenanceId);

      return Ok(hasConflict);
    }
  }
}
