using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Events;

namespace AspnetCoreMvcFull.Services
{
  public class MaintenanceScheduleService : IMaintenanceScheduleService
  {
    private readonly AppDbContext _context;
    private readonly ICraneService _craneService;
    private readonly IShiftDefinitionService _shiftDefinitionService;
    private readonly IScheduleConflictService _scheduleConflictService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<MaintenanceScheduleService> _logger;

    public MaintenanceScheduleService(
        AppDbContext context,
        ICraneService craneService,
        IShiftDefinitionService shiftDefinitionService,
        IScheduleConflictService scheduleConflictService,
        IEventPublisher eventPublisher,
        ILogger<MaintenanceScheduleService> logger)
    {
      _context = context;
      _craneService = craneService;
      _shiftDefinitionService = shiftDefinitionService;
      _scheduleConflictService = scheduleConflictService;
      _eventPublisher = eventPublisher;
      _logger = logger;
    }

    public async Task<IEnumerable<MaintenanceScheduleDto>> GetAllMaintenanceSchedulesAsync()
    {
      var schedules = await _context.MaintenanceSchedules
          .Include(m => m.Crane)
          .OrderByDescending(m => m.CreatedAt)
          .ToListAsync();

      return schedules.Select(m => new MaintenanceScheduleDto
      {
        Id = m.Id,
        CraneId = m.CraneId,
        CraneCode = m.Crane?.Code,
        Title = m.Title,
        StartDate = m.StartDate,
        EndDate = m.EndDate,
        Description = m.Description,
        CreatedAt = m.CreatedAt,
        CreatedBy = m.CreatedBy
      }).ToList();
    }

    public async Task<MaintenanceScheduleDetailDto> GetMaintenanceScheduleByIdAsync(int id)
    {
      var schedule = await _context.MaintenanceSchedules
          .Include(m => m.Crane)
          .Include(m => m.MaintenanceScheduleShifts)
            .ThenInclude(ms => ms.ShiftDefinition)
          .FirstOrDefaultAsync(m => m.Id == id);

      if (schedule == null)
      {
        throw new KeyNotFoundException($"Maintenance schedule with ID {id} not found");
      }

      return new MaintenanceScheduleDetailDto
      {
        Id = schedule.Id,
        CraneId = schedule.CraneId,
        CraneCode = schedule.Crane?.Code,
        Title = schedule.Title,
        StartDate = schedule.StartDate,
        EndDate = schedule.EndDate,
        Description = schedule.Description,
        CreatedAt = schedule.CreatedAt,
        CreatedBy = schedule.CreatedBy,
        Shifts = schedule.MaintenanceScheduleShifts.Select(s => new MaintenanceScheduleShiftDto
        {
          Id = s.Id,
          Date = s.Date,
          ShiftDefinitionId = s.ShiftDefinitionId,
          ShiftName = s.ShiftName ?? s.ShiftDefinition?.Name,
          StartTime = s.ShiftStartTime != default ? s.ShiftStartTime : s.ShiftDefinition?.StartTime,
          EndTime = s.ShiftEndTime != default ? s.ShiftEndTime : s.ShiftDefinition?.EndTime
        }).ToList()
      };
    }

    public async Task<IEnumerable<MaintenanceScheduleDto>> GetMaintenanceSchedulesByCraneIdAsync(int craneId)
    {
      if (!await _craneService.CraneExistsAsync(craneId))
      {
        throw new KeyNotFoundException($"Crane with ID {craneId} not found");
      }

      var schedules = await _context.MaintenanceSchedules
          .Include(m => m.Crane)
          .Where(m => m.CraneId == craneId)
          .OrderByDescending(m => m.CreatedAt)
          .ToListAsync();

      return schedules.Select(m => new MaintenanceScheduleDto
      {
        Id = m.Id,
        CraneId = m.CraneId,
        CraneCode = m.Crane?.Code,
        Title = m.Title,
        StartDate = m.StartDate,
        EndDate = m.EndDate,
        Description = m.Description,
        CreatedAt = m.CreatedAt,
        CreatedBy = m.CreatedBy
      }).ToList();
    }

    public async Task<MaintenanceScheduleDetailDto> CreateMaintenanceScheduleAsync(MaintenanceScheduleCreateDto maintenanceDto)
    {
      try
      {
        _logger.LogInformation("Creating maintenance schedule for crane {CraneId}", maintenanceDto.CraneId);

        // Validate crane exists
        if (!await _craneService.CraneExistsAsync(maintenanceDto.CraneId))
        {
          throw new KeyNotFoundException($"Crane with ID {maintenanceDto.CraneId} not found");
        }

        // Gunakan tanggal lokal tanpa konversi UTC
        var startDate = maintenanceDto.StartDate.Date;
        var endDate = maintenanceDto.EndDate.Date;

        // Validate date range
        if (startDate > endDate)
        {
          throw new ArgumentException("Start date must be before or equal to end date");
        }

        // Validate shift selections
        if (maintenanceDto.ShiftSelections == null || !maintenanceDto.ShiftSelections.Any())
        {
          throw new ArgumentException("At least one shift selection is required");
        }

        // Check if all dates in the range have shift selections
        var dateRange = Enumerable.Range(0, (endDate - startDate).Days + 1)
            .Select(d => startDate.AddDays(d))
            .ToList();

        var selectedDates = maintenanceDto.ShiftSelections
            .Select(s => s.Date.Date)
            .ToList();

        if (!dateRange.All(d => selectedDates.Contains(d)))
        {
          throw new ArgumentException("All dates in the range must have shift selections");
        }

        // Validate each shift selection has at least one shift selected
        foreach (var selection in maintenanceDto.ShiftSelections)
        {
          if (selection.SelectedShiftIds == null || !selection.SelectedShiftIds.Any())
          {
            throw new ArgumentException($"At least one shift must be selected for date {selection.Date.ToShortDateString()}");
          }
        }

        // Create maintenance schedule
        var schedule = new MaintenanceSchedule
        {
          CraneId = maintenanceDto.CraneId,
          Title = maintenanceDto.Title,
          StartDate = startDate,
          EndDate = endDate,
          Description = maintenanceDto.Description,
          CreatedAt = DateTime.Now,
          CreatedBy = maintenanceDto.CreatedBy
        };

        _context.MaintenanceSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        // Create shift selections with historical data
        foreach (var selection in maintenanceDto.ShiftSelections)
        {
          var dateLocal = selection.Date.Date;

          foreach (var shiftId in selection.SelectedShiftIds)
          {
            // Dapatkan informasi shift saat ini
            var shiftDefinition = await _context.ShiftDefinitions.FindAsync(shiftId);
            if (shiftDefinition == null)
            {
              throw new KeyNotFoundException($"Shift definition with ID {shiftId} not found");
            }

            var scheduleShift = new MaintenanceScheduleShift
            {
              MaintenanceScheduleId = schedule.Id,
              Date = dateLocal,
              ShiftDefinitionId = shiftId,
              // Simpan juga data historis shift
              ShiftName = shiftDefinition.Name,
              ShiftStartTime = shiftDefinition.StartTime,
              ShiftEndTime = shiftDefinition.EndTime
            };

            _context.MaintenanceScheduleShifts.Add(scheduleShift);
          }
        }

        await _context.SaveChangesAsync();

        // Publish event untuk relokasi booking yang terdampak
        await _eventPublisher.PublishAsync(new CraneMaintenanceEvent
        {
          CraneId = schedule.CraneId,
          MaintenanceStartTime = schedule.StartDate,
          MaintenanceEndTime = schedule.EndDate,
          Reason = $"Scheduled Maintenance: {schedule.Title}"
        });

        // Return the created maintenance schedule with details
        return await GetMaintenanceScheduleByIdAsync(schedule.Id);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error creating maintenance schedule: {Message}", ex.Message);
        throw;
      }
    }

    public async Task<MaintenanceScheduleDetailDto> UpdateMaintenanceScheduleAsync(int id, MaintenanceScheduleUpdateDto maintenanceDto)
    {
      try
      {
        _logger.LogInformation("Updating maintenance schedule ID: {Id}", id);

        var schedule = await _context.MaintenanceSchedules
            .Include(m => m.MaintenanceScheduleShifts)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (schedule == null)
        {
          throw new KeyNotFoundException($"Maintenance schedule with ID {id} not found");
        }

        // Validate crane exists if changing crane
        if (schedule.CraneId != maintenanceDto.CraneId &&
            !await _craneService.CraneExistsAsync(maintenanceDto.CraneId))
        {
          throw new KeyNotFoundException($"Crane with ID {maintenanceDto.CraneId} not found");
        }

        // Gunakan tanggal lokal tanpa konversi UTC
        var startDate = maintenanceDto.StartDate.Date;
        var endDate = maintenanceDto.EndDate.Date;

        // Validate date range
        if (startDate > endDate)
        {
          throw new ArgumentException("Start date must be before or equal to end date");
        }

        // Validate shift selections
        if (maintenanceDto.ShiftSelections == null || !maintenanceDto.ShiftSelections.Any())
        {
          throw new ArgumentException("At least one shift selection is required");
        }

        // Check if all dates in the range have shift selections
        var dateRange = Enumerable.Range(0, (endDate - startDate).Days + 1)
            .Select(d => startDate.AddDays(d))
            .ToList();

        var selectedDates = maintenanceDto.ShiftSelections
            .Select(s => s.Date.Date)
            .ToList();

        if (!dateRange.All(d => selectedDates.Contains(d)))
        {
          throw new ArgumentException("All dates in the range must have shift selections");
        }

        // Validate each shift selection has at least one shift selected
        foreach (var selection in maintenanceDto.ShiftSelections)
        {
          if (selection.SelectedShiftIds == null || !selection.SelectedShiftIds.Any())
          {
            throw new ArgumentException($"At least one shift must be selected for date {selection.Date.ToShortDateString()}");
          }

          // Gunakan tanggal lokal untuk pengecekan konflik
          var dateLocal = selection.Date.Date;

          // Check for scheduling conflicts for each selected shift
          foreach (var shiftId in selection.SelectedShiftIds)
          {
            // Verify the shift definition exists
            if (!await _shiftDefinitionService.ShiftDefinitionExistsAsync(shiftId))
            {
              throw new KeyNotFoundException($"Shift definition with ID {shiftId} not found");
            }

            // Cek apakah ada konflik dengan booking yang ada
            bool hasBookingConflict = await _scheduleConflictService.IsBookingConflictAsync(
                maintenanceDto.CraneId,
                dateLocal,
                shiftId);

            if (hasBookingConflict)
            {
              // Get shift name for better error message
              var shift = await _context.ShiftDefinitions.FindAsync(shiftId);
              throw new InvalidOperationException($"Scheduling conflict with existing booking detected for date {dateLocal.ToShortDateString()} and shift {shift?.Name ?? shiftId.ToString()}");
            }

            // Cek apakah ada konflik dengan maintenance schedule lain
            bool hasMaintenanceConflict = await _scheduleConflictService.IsMaintenanceConflictAsync(
                maintenanceDto.CraneId,
                dateLocal,
                shiftId,
                id);

            if (hasMaintenanceConflict)
            {
              // Get shift name for better error message
              var shift = await _context.ShiftDefinitions.FindAsync(shiftId);
              throw new InvalidOperationException($"Scheduling conflict with existing maintenance schedule detected for date {dateLocal.ToShortDateString()} and shift {shift?.Name ?? shiftId.ToString()}");
            }
          }
        }

        // Capture previous values for event
        var previousCraneId = schedule.CraneId;
        var previousStartDate = schedule.StartDate;
        var previousEndDate = schedule.EndDate;

        // Update maintenance schedule
        schedule.CraneId = maintenanceDto.CraneId;
        schedule.Title = maintenanceDto.Title;
        schedule.StartDate = startDate;
        schedule.EndDate = endDate;
        schedule.Description = maintenanceDto.Description;
        // CreatedAt dan CreatedBy tidak diubah

        // Remove existing shift selections
        _context.MaintenanceScheduleShifts.RemoveRange(schedule.MaintenanceScheduleShifts);

        // Create new shift selections with historical data
        foreach (var selection in maintenanceDto.ShiftSelections)
        {
          var dateLocal = selection.Date.Date;

          foreach (var shiftId in selection.SelectedShiftIds)
          {
            // Dapatkan informasi shift saat ini
            var shiftDefinition = await _context.ShiftDefinitions.FindAsync(shiftId);
            if (shiftDefinition == null)
            {
              throw new KeyNotFoundException($"Shift definition with ID {shiftId} not found");
            }

            var scheduleShift = new MaintenanceScheduleShift
            {
              MaintenanceScheduleId = schedule.Id,
              Date = dateLocal,
              ShiftDefinitionId = shiftId,
              // Simpan juga data historis shift
              ShiftName = shiftDefinition.Name,
              ShiftStartTime = shiftDefinition.StartTime,
              ShiftEndTime = shiftDefinition.EndTime
            };

            _context.MaintenanceScheduleShifts.Add(scheduleShift);
          }
        }

        await _context.SaveChangesAsync();

        // Jika ada perubahan pada crane, tanggal, atau shift yang mempengaruhi booking,
        // publish event untuk relokasi booking yang terdampak
        if (previousCraneId != schedule.CraneId ||
            previousStartDate != schedule.StartDate ||
            previousEndDate != schedule.EndDate)
        {
          // Jika crane berubah, handle relokasi untuk crane lama
          if (previousCraneId != schedule.CraneId)
          {
            // Tidak perlu merelokasi booking pada crane lama
          }

          // Publish event untuk relokasi booking yang terdampak pada crane baru
          await _eventPublisher.PublishAsync(new CraneMaintenanceEvent
          {
            CraneId = schedule.CraneId,
            MaintenanceStartTime = schedule.StartDate,
            MaintenanceEndTime = schedule.EndDate,
            Reason = $"Updated Scheduled Maintenance: {schedule.Title}"
          });
        }

        // Return the updated maintenance schedule with details
        return await GetMaintenanceScheduleByIdAsync(schedule.Id);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error updating maintenance schedule: {Message}", ex.Message);
        throw;
      }
    }

    public async Task DeleteMaintenanceScheduleAsync(int id)
    {
      try
      {
        _logger.LogInformation("Deleting maintenance schedule ID: {Id}", id);

        var schedule = await _context.MaintenanceSchedules
            .Include(m => m.MaintenanceScheduleShifts)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (schedule == null)
        {
          throw new KeyNotFoundException($"Maintenance schedule with ID {id} not found");
        }

        // Remove all associated shifts
        _context.MaintenanceScheduleShifts.RemoveRange(schedule.MaintenanceScheduleShifts);

        // Remove the maintenance schedule
        _context.MaintenanceSchedules.Remove(schedule);

        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error deleting maintenance schedule: {Message}", ex.Message);
        throw;
      }
    }

    public async Task<bool> IsShiftMaintenanceConflictAsync(int craneId, DateTime date, int shiftDefinitionId, int? excludeMaintenanceId = null)
    {
      return await _scheduleConflictService.IsMaintenanceConflictAsync(craneId, date, shiftDefinitionId, excludeMaintenanceId);
    }

    public async Task<bool> MaintenanceScheduleExistsAsync(int id)
    {
      return await _context.MaintenanceSchedules.AnyAsync(m => m.Id == id);
    }
  }
}
