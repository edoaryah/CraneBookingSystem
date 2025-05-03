// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization;
// using AspnetCoreMvcFull.DTOs;
// using AspnetCoreMvcFull.Services;
// using AspnetCoreMvcFull.Filters;
// using Microsoft.AspNetCore.Authentication.JwtBearer;

// namespace AspnetCoreMvcFull.Controllers.Api
// {
//   [Route("api/[controller]")]
//   [ApiController]
//   [ServiceFilter(typeof(AuthorizationFilter))]
//   public class HazardsController : ControllerBase
//   {
//     private readonly IHazardService _hazardService;

//     public HazardsController(IHazardService hazardService)
//     {
//       _hazardService = hazardService;
//     }

//     // GET: api/Hazards
//     [HttpGet]
//     public async Task<ActionResult<IEnumerable<HazardDto>>> GetAllHazards()
//     {
//       var hazards = await _hazardService.GetAllHazardsAsync();
//       return Ok(hazards);
//     }

//     // GET: api/Hazards/5
//     [HttpGet("{id}")]
//     public async Task<ActionResult<HazardDto>> GetHazard(int id)
//     {
//       var hazard = await _hazardService.GetHazardByIdAsync(id);
//       return Ok(hazard);
//     }

//     // POST: api/Hazards
//     [HttpPost]
//     public async Task<ActionResult<HazardDto>> CreateHazard(HazardCreateDto hazardDto)
//     {
//       var result = await _hazardService.CreateHazardAsync(hazardDto);
//       return CreatedAtAction(nameof(GetHazard), new { id = result.Id }, result);
//     }

//     // PUT: api/Hazards/5
//     [HttpPut("{id}")]
//     public async Task<ActionResult<HazardDto>> UpdateHazard(int id, HazardUpdateDto hazardDto)
//     {
//       var result = await _hazardService.UpdateHazardAsync(id, hazardDto);
//       return Ok(result);
//     }

//     // DELETE: api/Hazards/5
//     [HttpDelete("{id}")]
//     public async Task<IActionResult> DeleteHazard(int id)
//     {
//       await _hazardService.DeleteHazardAsync(id);
//       return NoContent();
//     }
//   }
// }
