using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;

namespace AspnetCoreMvcFull.Services
{
  public class ScheduleConflictService : IScheduleConflictService
  {
    private readonly AppDbContext _context;
    private readonly ILogger<ScheduleConflictService> _logger;

    public ScheduleConflictService(AppDbContext context, ILogger<ScheduleConflictService> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<bool> IsBookingConflictAsync(int craneId, DateTime date, int shiftDefinitionId, int? excludeBookingId = null)
    {
      try
      {
        // Gunakan tanggal lokal tanpa konversi
        var dateLocal = date.Date;

        var query = _context.BookingShifts
            .Include(rs => rs.Booking)
            .Where(rs => rs.Booking!.CraneId == craneId &&
                    rs.Date.Date == dateLocal &&
                    rs.ShiftDefinitionId == shiftDefinitionId);

        if (excludeBookingId.HasValue)
        {
          query = query.Where(rs => rs.BookingId != excludeBookingId.Value);
        }

        var existingBookings = await query.AnyAsync();
        return existingBookings;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error checking booking conflict for crane {CraneId}, date {Date}, shift {ShiftId}",
            craneId, date, shiftDefinitionId);
        throw;
      }
    }

    public async Task<bool> IsMaintenanceConflictAsync(int craneId, DateTime date, int shiftDefinitionId, int? excludeMaintenanceId = null)
    {
      try
      {
        // Gunakan tanggal lokal tanpa konversi
        var dateLocal = date.Date;

        var query = _context.MaintenanceScheduleShifts
            .Include(ms => ms.MaintenanceSchedule)
            .Where(ms => ms.MaintenanceSchedule!.CraneId == craneId &&
                    ms.Date.Date == dateLocal &&
                    ms.ShiftDefinitionId == shiftDefinitionId);

        if (excludeMaintenanceId.HasValue)
        {
          query = query.Where(ms => ms.MaintenanceScheduleId != excludeMaintenanceId.Value);
        }

        var existingMaintenance = await query.AnyAsync();
        return existingMaintenance;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error checking maintenance conflict for crane {CraneId}, date {Date}, shift {ShiftId}",
            craneId, date, shiftDefinitionId);
        throw;
      }
    }
  }
}
