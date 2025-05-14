// Services/CraneUsage/CraneUsageService.cs
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.ViewModels.CraneUsage;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Services.CraneUsage
{
  public class CraneUsageService : ICraneUsageService
  {
    private readonly AppDbContext _context;
    private readonly ILogger<CraneUsageService> _logger;

    public CraneUsageService(AppDbContext context, ILogger<CraneUsageService> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<List<CraneUsageEntryViewModel>> GetCraneUsageEntriesForDateAsync(int craneId, DateTime date)
    {
      // Dapatkan record untuk crane dan tanggal ini
      var record = await _context.CraneUsageRecords
          .Include(r => r.Entries)
              .ThenInclude(e => e.UsageSubcategory)
          .Include(r => r.Entries)
              .ThenInclude(e => e.Booking)
          .Include(r => r.Entries)
              .ThenInclude(e => e.MaintenanceSchedule)
          .FirstOrDefaultAsync(r => r.CraneId == craneId && r.Date.Date == date.Date);

      if (record == null)
      {
        return new List<CraneUsageEntryViewModel>();
      }

      return record.Entries.Select(e => new CraneUsageEntryViewModel
      {
        Id = e.Id,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        Category = e.Category,
        UsageSubcategoryId = e.UsageSubcategoryId,
        BookingId = e.BookingId,
        MaintenanceScheduleId = e.MaintenanceScheduleId,
        Notes = e.Notes,
        CategoryName = e.Category.ToString(),
        SubcategoryName = e.UsageSubcategory?.Name ?? string.Empty,
        BookingNumber = e.Booking?.BookingNumber,
        MaintenanceTitle = e.MaintenanceSchedule?.Title
      }).OrderBy(e => e.StartTime).ToList();
    }

    public async Task<bool> SaveCraneUsageFormAsync(CraneUsageFormViewModel viewModel, string userName)
    {
      // Validate that there are no time conflicts
      if (!ValidateNoTimeConflicts(viewModel.Entries))
      {
        return false;
      }

      using var transaction = await _context.Database.BeginTransactionAsync();

      try
      {
        // Find or create record
        var record = await _context.CraneUsageRecords
            .FirstOrDefaultAsync(r => r.CraneId == viewModel.CraneId && r.Date.Date == viewModel.Date.Date);

        if (record == null)
        {
          // Create new record
          record = new CraneUsageRecord
          {
            CraneId = viewModel.CraneId,
            Date = viewModel.Date.Date,
            OperatorName = viewModel.OperatorName,
            CreatedAt = DateTime.Now,
            CreatedBy = userName
          };

          _context.CraneUsageRecords.Add(record);
          await _context.SaveChangesAsync();
        }
        else
        {
          // Update operator name
          record.OperatorName = viewModel.OperatorName;
          await _context.SaveChangesAsync();
        }

        // Get existing entries for this record
        var existingEntries = await _context.CraneUsageEntries
            .Where(e => e.CraneUsageRecordId == record.Id)
            .ToListAsync();

        // Find entries to delete (in existing but not in viewModel)
        var entriesToDelete = existingEntries
            .Where(e => !viewModel.Entries.Any(ve => ve.Id == e.Id))
            .ToList();

        // Delete entries
        foreach (var entry in entriesToDelete)
        {
          _context.CraneUsageEntries.Remove(entry);
        }

        // Find entries to update or add
        foreach (var viewModelEntry in viewModel.Entries)
        {
          if (viewModelEntry.Id > 0)
          {
            // Update existing entry
            var existingEntry = existingEntries.FirstOrDefault(e => e.Id == viewModelEntry.Id);
            if (existingEntry != null)
            {
              existingEntry.StartTime = viewModelEntry.StartTime;
              existingEntry.EndTime = viewModelEntry.EndTime;
              existingEntry.Category = viewModelEntry.Category;
              existingEntry.UsageSubcategoryId = viewModelEntry.UsageSubcategoryId;
              existingEntry.BookingId = viewModelEntry.BookingId;
              existingEntry.MaintenanceScheduleId = viewModelEntry.MaintenanceScheduleId;
              existingEntry.Notes = viewModelEntry.Notes;
            }
          }
          else
          {
            // Add new entry
            var newEntry = new CraneUsageEntry
            {
              CraneUsageRecordId = record.Id,
              StartTime = viewModelEntry.StartTime,
              EndTime = viewModelEntry.EndTime,
              Category = viewModelEntry.Category,
              UsageSubcategoryId = viewModelEntry.UsageSubcategoryId,
              BookingId = viewModelEntry.BookingId,
              MaintenanceScheduleId = viewModelEntry.MaintenanceScheduleId,
              Notes = viewModelEntry.Notes
            };

            _context.CraneUsageEntries.Add(newEntry);
          }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error saving crane usage form");
        await transaction.RollbackAsync();
        return false;
      }
    }

    public async Task<bool> AddCraneUsageEntryAsync(int craneId, DateTime date, CraneUsageEntryViewModel entry, string userName)
    {
      // Find or create record
      var record = await _context.CraneUsageRecords
          .FirstOrDefaultAsync(r => r.CraneId == craneId && r.Date.Date == date.Date);

      if (record == null)
      {
        // Create new record
        record = new CraneUsageRecord
        {
          CraneId = craneId,
          Date = date.Date,
          OperatorName = userName,
          CreatedAt = DateTime.Now,
          CreatedBy = userName
        };

        _context.CraneUsageRecords.Add(record);
        await _context.SaveChangesAsync();
      }

      // Create new entry
      var newEntry = new CraneUsageEntry
      {
        CraneUsageRecordId = record.Id,
        StartTime = entry.StartTime,
        EndTime = entry.EndTime,
        Category = entry.Category,
        UsageSubcategoryId = entry.UsageSubcategoryId,
        BookingId = entry.BookingId,
        MaintenanceScheduleId = entry.MaintenanceScheduleId,
        Notes = entry.Notes
      };

      _context.CraneUsageEntries.Add(newEntry);
      await _context.SaveChangesAsync();

      return true;
    }

    // Di CraneUsageService.cs
    public async Task<bool> UpdateCraneUsageEntryAsync(CraneUsageEntryViewModel entry)
    {
      try
      {
        _logger.LogInformation($"Service - Update entry dengan ID: {entry.Id}");
        _logger.LogInformation($"Service - Waktu yang diterima - Start: {entry.StartTime}, End: {entry.EndTime}");

        var existingEntry = await _context.CraneUsageEntries.FindAsync(entry.Id);
        if (existingEntry == null)
        {
          _logger.LogWarning($"Service - Entry dengan ID {entry.Id} tidak ditemukan");
          return false;
        }

        // PERBAIKAN: Pastikan konversi waktu benar
        // Parse waktu dengan format yang benar
        string startTimeStr = entry.StartTime.ToString();
        string endTimeStr = entry.EndTime.ToString();

        // Pastikan format HH:mm
        if (startTimeStr.Count(c => c == ':') > 1)
          startTimeStr = string.Join(":", startTimeStr.Split(':').Take(2));
        if (endTimeStr.Count(c => c == ':') > 1)
          endTimeStr = string.Join(":", endTimeStr.Split(':').Take(2));

        TimeSpan startTime, endTime;

        // Gunakan TryParse untuk menghindari error
        if (!TimeSpan.TryParse(startTimeStr, out startTime) ||
            !TimeSpan.TryParse(endTimeStr, out endTime))
        {
          _logger.LogError($"Service - Gagal parsing waktu: {startTimeStr} atau {endTimeStr}");
          return false;
        }

        _logger.LogInformation($"Service - Waktu setelah parsing - Start: {startTime}, End: {endTime}");

        // Bandingkan waktu dengan benar
        if (startTime >= endTime)
        {
          _logger.LogWarning($"Service - Validasi waktu: {startTime} >= {endTime}");
          return false;
        }

        // Update entry dengan nilai waktu yang benar
        existingEntry.StartTime = startTime;
        existingEntry.EndTime = endTime;
        existingEntry.Category = entry.Category;
        existingEntry.UsageSubcategoryId = entry.UsageSubcategoryId;

        // Pastikan BookingId tetap ada
        if (entry.BookingId.HasValue)
        {
          existingEntry.BookingId = entry.BookingId;
        }

        existingEntry.Notes = entry.Notes;

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Service - Entry berhasil diupdate");
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Service - Error updating crane usage entry");
        return false;
      }
    }

    public async Task<bool> DeleteCraneUsageEntryAsync(int entryId)
    {
      var entry = await _context.CraneUsageEntries.FindAsync(entryId);
      if (entry == null)
      {
        return false;
      }

      _context.CraneUsageEntries.Remove(entry);
      await _context.SaveChangesAsync();

      // Check if there are no more entries for this record
      var recordId = entry.CraneUsageRecordId;
      var entriesCount = await _context.CraneUsageEntries
          .CountAsync(e => e.CraneUsageRecordId == recordId);

      if (entriesCount == 0)
      {
        // Delete the record if no entries remain
        var record = await _context.CraneUsageRecords.FindAsync(recordId);
        if (record != null)
        {
          _context.CraneUsageRecords.Remove(record);
          await _context.SaveChangesAsync();
        }
      }

      return true;
    }

    public async Task<CraneUsageEntryViewModel> GetCraneUsageEntryByIdAsync(int id)
    {
      var entry = await _context.CraneUsageEntries
          .Include(e => e.UsageSubcategory)
          .Include(e => e.Booking)
          .Include(e => e.MaintenanceSchedule)
          .FirstOrDefaultAsync(e => e.Id == id);

      if (entry == null)
      {
        return new CraneUsageEntryViewModel();
      }

      return new CraneUsageEntryViewModel
      {
        Id = entry.Id,
        StartTime = entry.StartTime,
        EndTime = entry.EndTime,
        Category = entry.Category,
        UsageSubcategoryId = entry.UsageSubcategoryId,
        BookingId = entry.BookingId,
        MaintenanceScheduleId = entry.MaintenanceScheduleId,
        Notes = entry.Notes,
        CategoryName = entry.Category.ToString(),
        SubcategoryName = entry.UsageSubcategory?.Name ?? string.Empty,
        BookingNumber = entry.Booking?.BookingNumber,
        MaintenanceTitle = entry.MaintenanceSchedule?.Title
      };
    }

    public async Task<CraneUsageEntryViewModel> GetCraneUsageEntryByTimeAsync(int craneId, DateTime date, TimeSpan startTime, TimeSpan endTime)
    {
      var record = await _context.CraneUsageRecords
          .FirstOrDefaultAsync(r => r.CraneId == craneId && r.Date.Date == date.Date);

      if (record == null)
      {
        return new CraneUsageEntryViewModel();
      }

      var entry = await _context.CraneUsageEntries
          .Include(e => e.UsageSubcategory)
          .Include(e => e.Booking)
          .Include(e => e.MaintenanceSchedule)
          .FirstOrDefaultAsync(e => e.CraneUsageRecordId == record.Id &&
                                    e.StartTime == startTime &&
                                    e.EndTime == endTime);

      if (entry == null)
      {
        return new CraneUsageEntryViewModel();
      }

      return new CraneUsageEntryViewModel
      {
        Id = entry.Id,
        StartTime = entry.StartTime,
        EndTime = entry.EndTime,
        Category = entry.Category,
        UsageSubcategoryId = entry.UsageSubcategoryId,
        BookingId = entry.BookingId,
        MaintenanceScheduleId = entry.MaintenanceScheduleId,
        Notes = entry.Notes,
        CategoryName = entry.Category.ToString(),
        SubcategoryName = entry.UsageSubcategory?.Name ?? string.Empty,
        BookingNumber = entry.Booking?.BookingNumber,
        MaintenanceTitle = entry.MaintenanceSchedule?.Title
      };
    }

    public bool ValidateNoTimeConflicts(List<CraneUsageEntryViewModel> existingEntries)
    {
      for (int i = 0; i < existingEntries.Count; i++)
      {
        for (int j = i + 1; j < existingEntries.Count; j++)
        {
          if (TimeSpansOverlap(existingEntries[i].StartTime, existingEntries[i].EndTime,
                              existingEntries[j].StartTime, existingEntries[j].EndTime))
          {
            return false;
          }
        }
      }
      return true;
    }

    public bool ValidateNoTimeConflicts(List<CraneUsageEntryViewModel> existingEntries, CraneUsageEntryViewModel newEntry)
    {
      _logger.LogInformation($"Validasi konflik waktu untuk entry baru: {newEntry.StartTime} - {newEntry.EndTime}");

      // Parse waktu entry baru
      string startTimeStr = newEntry.StartTime.ToString();
      string endTimeStr = newEntry.EndTime.ToString();

      // Format HH:mm
      if (startTimeStr.Count(c => c == ':') > 1)
        startTimeStr = string.Join(":", startTimeStr.Split(':').Take(2));
      if (endTimeStr.Count(c => c == ':') > 1)
        endTimeStr = string.Join(":", endTimeStr.Split(':').Take(2));

      TimeSpan newStart = TimeSpan.Parse(startTimeStr);
      TimeSpan newEnd = TimeSpan.Parse(endTimeStr);

      foreach (var entry in existingEntries)
      {
        // Parse waktu entry yang ada
        string existingStartStr = entry.StartTime.ToString();
        string existingEndStr = entry.EndTime.ToString();

        // Format HH:mm
        if (existingStartStr.Count(c => c == ':') > 1)
          existingStartStr = string.Join(":", existingStartStr.Split(':').Take(2));
        if (existingEndStr.Count(c => c == ':') > 1)
          existingEndStr = string.Join(":", existingEndStr.Split(':').Take(2));

        TimeSpan existingStart = TimeSpan.Parse(existingStartStr);
        TimeSpan existingEnd = TimeSpan.Parse(existingEndStr);

        if (newStart < existingEnd && existingStart < newEnd)
        {
          _logger.LogWarning($"Konflik waktu: Baru {newStart}-{newEnd} vs Existing {existingStart}-{existingEnd}");
          return false;
        }
      }

      return true;
    }

    // Pastikan fungsi di service yang memvalidasi overlap sudah benar
    private bool TimeSpansOverlap(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
    {
      return start1 < end2 && start2 < end1;
    }

    public async Task<List<SelectListItem>> GetSubcategoriesByCategoryAsync(UsageCategory category)
    {
      var subcategories = await _context.UsageSubcategories
          .Where(s => s.Category == category && s.IsActive)
          .OrderBy(s => s.Name)
          .Select(s => new SelectListItem
          {
            Value = s.Id.ToString(),
            Text = s.Name
          })
          .ToListAsync();

      return subcategories;
    }

    public async Task<List<SelectListItem>> GetAvailableBookingsAsync(int craneId, DateTime startTime, DateTime endTime, int? currentEntryId = null)
    {
      var bookings = await _context.Bookings
          .Where(b => b.CraneId == craneId &&
                     b.Status == BookingStatus.PICApproved &&
                     ((b.StartDate <= startTime && b.EndDate >= startTime) ||
                     (b.StartDate <= endTime && b.EndDate >= endTime) ||
                     (b.StartDate >= startTime && b.EndDate <= endTime)))
          .Select(b => new SelectListItem
          {
            Value = b.Id.ToString(),
            Text = $"{b.BookingNumber} - {b.Name} ({b.StartDate:dd/MM/yyyy HH:mm} - {b.EndDate:dd/MM/yyyy HH:mm})"
          })
          .ToListAsync();

      return bookings;
    }

    public async Task<List<SelectListItem>> GetAvailableMaintenanceSchedulesAsync(int craneId, DateTime startTime, DateTime endTime, int? currentEntryId = null)
    {
      var maintenance = await _context.MaintenanceSchedules
          .Where(m => m.CraneId == craneId &&
                     ((m.StartDate <= startTime && m.EndDate >= startTime) ||
                     (m.StartDate <= endTime && m.EndDate >= endTime) ||
                     (m.StartDate >= startTime && m.EndDate <= endTime)))
          .Select(m => new SelectListItem
          {
            Value = m.Id.ToString(),
            Text = $"{m.Title} ({m.StartDate:dd/MM/yyyy HH:mm} - {m.EndDate:dd/MM/yyyy HH:mm})"
          })
          .ToListAsync();

      return maintenance;
    }

    public async Task<CraneUsageVisualizationViewModel> GetUsageVisualizationDataAsync(int craneId, DateTime date)
    {
      var crane = await _context.Cranes.FindAsync(craneId);
      if (crane == null)
      {
        throw new KeyNotFoundException($"Crane with ID {craneId} not found");
      }

      var viewModel = new CraneUsageVisualizationViewModel
      {
        CraneId = craneId,
        Date = date,
        CraneName = crane.Code,
        CraneList = await _context.Cranes
              .OrderBy(c => c.Code)
              .Select(c => new SelectListItem
              {
                Value = c.Id.ToString(),
                Text = $"{c.Code} - {c.Capacity} Ton"
              })
              .ToListAsync()
      };

      // Initialize hourly data with default (Standby)
      var hourlyData = new List<HourlyUsageData>();
      for (int hour = 0; hour < 24; hour++)
      {
        hourlyData.Add(new HourlyUsageData
        {
          Hour = hour,
          Category = "Standby",
          ColorCode = GetCategoryColorCode(UsageCategory.Standby)
        });
      }

      // Load entries for this crane and date
      var entries = await GetCraneUsageEntriesForDateAsync(craneId, date);

      // Process entries
      foreach (var entry in entries)
      {
        var startHour = (int)entry.StartTime.TotalHours;
        var endHour = (int)entry.EndTime.TotalHours;

        for (int hour = startHour; hour < endHour; hour++)
        {
          if (hour >= 0 && hour < 24) // Ensure hour is within range
          {
            hourlyData[hour].Category = entry.CategoryName;
            hourlyData[hour].SubcategoryName = entry.SubcategoryName;
            hourlyData[hour].ColorCode = GetCategoryColorCode(entry.Category);
            hourlyData[hour].Notes = entry.Notes ?? string.Empty;

            if (!string.IsNullOrEmpty(entry.BookingNumber))
            {
              hourlyData[hour].BookingNumber = entry.BookingNumber;
            }

            if (!string.IsNullOrEmpty(entry.MaintenanceTitle))
            {
              hourlyData[hour].MaintenanceTitle = entry.MaintenanceTitle;
            }
          }
        }
      }

      viewModel.HourlyData = hourlyData;
      return viewModel;
    }

    // Di Services/CraneUsage/CraneUsageService.cs, perbaiki method GetFilteredUsageRecordsAsync

    public async Task<CraneUsageListViewModel> GetFilteredUsageRecordsAsync(CraneUsageFilterViewModel filter)
    {
      var query = _context.CraneUsageEntries
          .Include(e => e.CraneUsageRecord)
              .ThenInclude(r => r!.Crane) // Use null-forgiving operator
          .Include(e => e.UsageSubcategory)
          .Include(e => e.Booking)
          .Include(e => e.MaintenanceSchedule)
          .AsQueryable();

      if (filter.CraneId.HasValue && filter.CraneId > 0)
      {
        query = query.Where(e => e.CraneUsageRecord != null && e.CraneUsageRecord.CraneId == filter.CraneId);
      }

      if (filter.StartDate.HasValue)
      {
        var startDate = filter.StartDate.Value.Date;
        query = query.Where(e => e.CraneUsageRecord != null && e.CraneUsageRecord.Date >= startDate);
      }

      if (filter.EndDate.HasValue)
      {
        var endDate = filter.EndDate.Value.Date.AddDays(1).AddSeconds(-1);
        query = query.Where(e => e.CraneUsageRecord != null && e.CraneUsageRecord.Date <= endDate);
      }

      if (filter.Category.HasValue)
      {
        query = query.Where(e => e.Category == filter.Category);
      }

      // Order by date and time with null checks
      query = query.OrderByDescending(e => e.CraneUsageRecord != null ? e.CraneUsageRecord.Date : DateTime.MinValue)
                   .ThenBy(e => e.StartTime);

      var entries = await query.ToListAsync();

      var viewModel = new CraneUsageListViewModel
      {
        UsageRecords = entries.Select(e => new CraneUsageEntryViewModel
        {
          Id = e.Id,
          StartTime = e.StartTime,
          EndTime = e.EndTime,
          Category = e.Category,
          UsageSubcategoryId = e.UsageSubcategoryId,
          BookingId = e.BookingId,
          MaintenanceScheduleId = e.MaintenanceScheduleId,
          Notes = e.Notes ?? string.Empty,
          CategoryName = e.Category.ToString(),
          SubcategoryName = e.UsageSubcategory?.Name ?? string.Empty,
          BookingNumber = e.Booking?.BookingNumber ?? string.Empty,
          MaintenanceTitle = e.MaintenanceSchedule?.Title ?? string.Empty,
          // Tambahkan properti yang dibutuhkan view Index
          CraneId = e.CraneUsageRecord?.CraneId ?? 0,
          CraneName = e.CraneUsageRecord?.Crane?.Code ?? string.Empty,
          Date = e.CraneUsageRecord?.Date ?? DateTime.MinValue,
          OperatorName = e.CraneUsageRecord?.OperatorName ?? string.Empty
        }).ToList(),
        Filter = filter
      };

      // Populate filter dropdowns
      filter.CraneList = await _context.Cranes
          .OrderBy(c => c.Code)
          .Select(c => new SelectListItem
          {
            Value = c.Id.ToString(),
            Text = $"{c.Code} - {c.Capacity} Ton"
          })
          .ToListAsync();

      filter.CategoryList = Enum.GetValues(typeof(UsageCategory))
          .Cast<UsageCategory>()
          .Select(c => new SelectListItem
          {
            Value = ((int)c).ToString(),
            Text = c.ToString()
          })
          .ToList();

      return viewModel;
    }

    // Helper methods
    private string GetCategoryColorCode(UsageCategory category)
    {
      return category switch
      {
        UsageCategory.Operating => "#28a745", // Green
        UsageCategory.Delay => "#ffc107",     // Yellow
        UsageCategory.Standby => "#6c757d",   // Gray
        UsageCategory.Service => "#17a2b8",   // Cyan
        UsageCategory.Breakdown => "#dc3545", // Red
        _ => "#6c757d"                        // Default Gray
      };
    }

    public async Task<bool> SaveBookingUsageFormAsync(BookingUsageFormViewModel viewModel, string userName)
    {
      // Konversi ke CraneUsageFormViewModel untuk memanfaatkan kode yang sudah ada
      var craneViewModel = new CraneUsageFormViewModel
      {
        CraneId = viewModel.CraneId,
        Date = viewModel.Date,
        OperatorName = viewModel.OperatorName,
        Entries = viewModel.Entries
      };

      // Gunakan metode yang sudah ada
      return await SaveCraneUsageFormAsync(craneViewModel, userName);
    }
  }
}
