using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.ViewModels.HazardManagement;

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

    public async Task<IEnumerable<HazardViewModel>> GetAllHazardsAsync()
    {
      var hazards = await _context.Hazards
          .OrderBy(h => h.Name)
          .ToListAsync();

      return hazards.Select(h => new HazardViewModel
      {
        Id = h.Id,
        Name = h.Name
      }).ToList();
    }

    public async Task<HazardViewModel> GetHazardByIdAsync(int id)
    {
      var hazard = await _context.Hazards
          .FirstOrDefaultAsync(h => h.Id == id);

      if (hazard == null)
      {
        throw new KeyNotFoundException($"Hazard with ID {id} not found");
      }

      return new HazardViewModel
      {
        Id = hazard.Id,
        Name = hazard.Name
      };
    }

    public async Task<HazardViewModel> CreateHazardAsync(HazardCreateViewModel hazardViewModel)
    {
      // Check if a hazard with the same name already exists
      if (await _context.Hazards.AnyAsync(h => h.Name == hazardViewModel.Name))
      {
        throw new InvalidOperationException($"Hazard with name '{hazardViewModel.Name}' already exists");
      }

      var hazard = new Hazard
      {
        Name = hazardViewModel.Name
      };

      _context.Hazards.Add(hazard);
      await _context.SaveChangesAsync();

      return new HazardViewModel
      {
        Id = hazard.Id,
        Name = hazard.Name
      };
    }

    public async Task<HazardViewModel> UpdateHazardAsync(int id, HazardUpdateViewModel hazardViewModel)
    {
      var hazard = await _context.Hazards.FindAsync(id);

      if (hazard == null)
      {
        throw new KeyNotFoundException($"Hazard with ID {id} not found");
      }

      // Check if another hazard with the same name already exists
      if (await _context.Hazards.AnyAsync(h => h.Name == hazardViewModel.Name && h.Id != id))
      {
        throw new InvalidOperationException($"Another hazard with name '{hazardViewModel.Name}' already exists");
      }

      // Update hazard properties
      hazard.Name = hazardViewModel.Name;

      await _context.SaveChangesAsync();

      return new HazardViewModel
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

      // Check if the hazard is in use by any bookings
      if (await _context.BookingHazards.AnyAsync(bh => bh.HazardId == id))
      {
        throw new InvalidOperationException($"Cannot delete hazard '{hazard.Name}' because it is in use by existing bookings");
      }

      _context.Hazards.Remove(hazard);
      await _context.SaveChangesAsync();
    }

    public async Task<bool> HazardExistsAsync(int id)
    {
      return await _context.Hazards.AnyAsync(h => h.Id == id);
    }
  }
}
