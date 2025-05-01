using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public class ShiftDefinitionService : IShiftDefinitionService
  {
    private readonly AppDbContext _context;
    private readonly ILogger<ShiftDefinitionService> _logger;

    public ShiftDefinitionService(AppDbContext context, ILogger<ShiftDefinitionService> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<IEnumerable<ShiftDefinitionDto>> GetAllShiftDefinitionsAsync()
    {
      var shifts = await _context.ShiftDefinitions
          .OrderBy(s => s.StartTime)
          .ToListAsync();

      return shifts.Select(s => new ShiftDefinitionDto
      {
        Id = s.Id,
        Name = s.Name,
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        Category = s.Category,
        IsActive = s.IsActive
      }).ToList();
    }

    public async Task<ShiftDefinitionDto> GetShiftDefinitionByIdAsync(int id)
    {
      var shift = await _context.ShiftDefinitions.FindAsync(id);

      if (shift == null)
      {
        throw new KeyNotFoundException($"Shift definition with ID {id} not found");
      }

      return new ShiftDefinitionDto
      {
        Id = shift.Id,
        Name = shift.Name,
        StartTime = shift.StartTime,
        EndTime = shift.EndTime,
        Category = shift.Category,
        IsActive = shift.IsActive
      };
    }

    public async Task<ShiftDefinitionDto> CreateShiftDefinitionAsync(ShiftDefinitionCreateDto shiftDto)
    {
      // Validate time range
      if (shiftDto.StartTime >= shiftDto.EndTime)
      {
        throw new ArgumentException("Start time must be before end time");
      }

      // Check for overlapping shifts
      var overlappingShifts = await _context.ShiftDefinitions
          .Where(s => (shiftDto.StartTime < s.EndTime && shiftDto.EndTime > s.StartTime) &&
                 (shiftDto.Category == s.Category || string.IsNullOrEmpty(shiftDto.Category)))
          .ToListAsync();

      if (overlappingShifts.Any())
      {
        throw new InvalidOperationException($"The new shift overlaps with existing shifts: {string.Join(", ", overlappingShifts.Select(s => s.Name))}");
      }

      var shift = new ShiftDefinition
      {
        Name = shiftDto.Name,
        StartTime = shiftDto.StartTime,
        EndTime = shiftDto.EndTime,
        Category = shiftDto.Category,
        IsActive = shiftDto.IsActive
      };

      _context.ShiftDefinitions.Add(shift);
      await _context.SaveChangesAsync();

      return new ShiftDefinitionDto
      {
        Id = shift.Id,
        Name = shift.Name,
        StartTime = shift.StartTime,
        EndTime = shift.EndTime,
        Category = shift.Category,
        IsActive = shift.IsActive
      };
    }

    public async Task<ShiftDefinitionDto> UpdateShiftDefinitionAsync(int id, ShiftDefinitionUpdateDto shiftDto)
    {
      var shift = await _context.ShiftDefinitions.FindAsync(id);

      if (shift == null)
      {
        throw new KeyNotFoundException($"Shift definition with ID {id} not found");
      }

      // Validate time range
      if (shiftDto.StartTime >= shiftDto.EndTime)
      {
        throw new ArgumentException("Start time must be before end time");
      }

      // Check for overlapping shifts (excluding the current one)
      var overlappingShifts = await _context.ShiftDefinitions
          .Where(s => s.Id != id &&
                 (shiftDto.StartTime < s.EndTime && shiftDto.EndTime > s.StartTime) &&
                 (shiftDto.Category == s.Category || string.IsNullOrEmpty(shiftDto.Category)))
          .ToListAsync();

      if (overlappingShifts.Any())
      {
        throw new InvalidOperationException($"The updated shift overlaps with existing shifts: {string.Join(", ", overlappingShifts.Select(s => s.Name))}");
      }

      // Update shift properties
      shift.Name = shiftDto.Name;
      shift.StartTime = shiftDto.StartTime;
      shift.EndTime = shiftDto.EndTime;
      shift.Category = shiftDto.Category;
      shift.IsActive = shiftDto.IsActive;

      await _context.SaveChangesAsync();

      return new ShiftDefinitionDto
      {
        Id = shift.Id,
        Name = shift.Name,
        StartTime = shift.StartTime,
        EndTime = shift.EndTime,
        Category = shift.Category,
        IsActive = shift.IsActive
      };
    }

    public async Task DeleteShiftDefinitionAsync(int id)
    {
      var shift = await _context.ShiftDefinitions
          .Include(s => s.BookingShifts)
          .FirstOrDefaultAsync(s => s.Id == id);

      if (shift == null)
      {
        throw new KeyNotFoundException($"Shift definition with ID {id} not found");
      }

      // Check if shift is in use
      if (shift.BookingShifts.Any())
      {
        throw new InvalidOperationException($"Cannot delete shift '{shift.Name}' because it is in use by existing bookings");
      }

      _context.ShiftDefinitions.Remove(shift);
      await _context.SaveChangesAsync();
    }

    public async Task<bool> ShiftDefinitionExistsAsync(int id)
    {
      return await _context.ShiftDefinitions.AnyAsync(s => s.Id == id);
    }
  }
}
