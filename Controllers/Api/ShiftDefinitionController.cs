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
  public class ShiftDefinitionsController : ControllerBase
  {
    private readonly IShiftDefinitionService _shiftDefinitionService;

    public ShiftDefinitionsController(IShiftDefinitionService shiftDefinitionService)
    {
      _shiftDefinitionService = shiftDefinitionService;
    }

    // GET: api/ShiftDefinitions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShiftDefinitionDto>>> GetShiftDefinitions()
    {
      var shifts = await _shiftDefinitionService.GetAllShiftDefinitionsAsync();
      return Ok(shifts);
    }

    // GET: api/ShiftDefinitions/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ShiftDefinitionDto>> GetShiftDefinition(int id)
    {
      var shift = await _shiftDefinitionService.GetShiftDefinitionByIdAsync(id);
      return Ok(shift);
    }

    // POST: api/ShiftDefinitions
    [HttpPost]
    public async Task<ActionResult<ShiftDefinitionDto>> CreateShiftDefinition(ShiftDefinitionCreateDto shiftDto)
    {
      var result = await _shiftDefinitionService.CreateShiftDefinitionAsync(shiftDto);
      return CreatedAtAction(nameof(GetShiftDefinition), new { id = result.Id }, result);
    }

    // PUT: api/ShiftDefinitions/5
    [HttpPut("{id}")]
    public async Task<ActionResult<ShiftDefinitionDto>> UpdateShiftDefinition(int id, ShiftDefinitionUpdateDto shiftDto)
    {
      var result = await _shiftDefinitionService.UpdateShiftDefinitionAsync(id, shiftDto);
      return Ok(result);
    }

    // DELETE: api/ShiftDefinitions/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteShiftDefinition(int id)
    {
      await _shiftDefinitionService.DeleteShiftDefinitionAsync(id);
      return NoContent();
    }
  }
}
