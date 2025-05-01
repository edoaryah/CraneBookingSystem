using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public class HazardService : IHazardService
  {
    private readonly AppDbContext _context;
    private readonly ILogger<HazardService> _logger;

    public HazardService(AppDbContext context, ILogger<HazardService> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<IEnumerable<HazardDto>> GetAllHazardsAsync()
    {
      var hazards = await _context.Hazards
          .OrderBy(h => h.Name)
          .ToListAsync();

      return hazards.Select(h => new HazardDto
      {
        Id = h.Id,
        Name = h.Name
      }).ToList();
    }

    public async Task<HazardDto> GetHazardByIdAsync(int id)
    {
      var hazard = await _context.Hazards
          .FirstOrDefaultAsync(h => h.Id == id);

      if (hazard == null)
      {
        throw new KeyNotFoundException($"Hazard with ID {id} not found");
      }

      return new HazardDto
      {
        Id = hazard.Id,
        Name = hazard.Name
      };
    }

    public async Task<HazardDto> CreateHazardAsync(HazardCreateDto hazardDto)
    {
      // Check if a hazard with the same name already exists
      if (await _context.Hazards.AnyAsync(h => h.Name == hazardDto.Name))
      {
        throw new InvalidOperationException($"Hazard with name '{hazardDto.Name}' already exists");
      }

      var hazard = new Hazard
      {
        Name = hazardDto.Name
      };

      _context.Hazards.Add(hazard);
      await _context.SaveChangesAsync();

      return new HazardDto
      {
        Id = hazard.Id,
        Name = hazard.Name
      };
    }

    public async Task<HazardDto> UpdateHazardAsync(int id, HazardUpdateDto hazardDto)
    {
      var hazard = await _context.Hazards.FindAsync(id);

      if (hazard == null)
      {
        throw new KeyNotFoundException($"Hazard with ID {id} not found");
      }

      // Check if another hazard with the same name already exists
      if (await _context.Hazards.AnyAsync(h => h.Name == hazardDto.Name && h.Id != id))
      {
        throw new InvalidOperationException($"Another hazard with name '{hazardDto.Name}' already exists");
      }

      // Update hazard properties
      hazard.Name = hazardDto.Name;

      await _context.SaveChangesAsync();

      return new HazardDto
      {
        Id = hazard.Id,
        Name = hazard.Name
      };
    }

    public async Task DeleteHazardAsync(int id)
    {
      var hazard = await _context.Hazards.FindAsync(id);

      if (hazard == null)
      {
        throw new KeyNotFoundException($"Hazard with ID {id} not found");
      }

      // Here you might want to check if the hazard is in use by other entities
      // Similar to the ShiftDefinition check for BookingShifts

      // For example:
      // if (await _context.SomeRelatedEntities.AnyAsync(e => e.HazardId == id))
      // {
      //     throw new InvalidOperationException($"Cannot delete hazard '{hazard.Name}' because it is in use");
      // }

      _context.Hazards.Remove(hazard);
      await _context.SaveChangesAsync();
    }

    public async Task<bool> HazardExistsAsync(int id)
    {
      return await _context.Hazards.AnyAsync(h => h.Id == id);
    }
  }
}
