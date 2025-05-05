// Tambahkan file Services/CraneUsage/CraneUsageService.cs

using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.ViewModels.CraneUsage;

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

    public async Task<IEnumerable<CraneUsageRecordViewModel>> GetAllUsageRecordsAsync()
    {
      try
      {
        var records = await _context.CraneUsageRecords
            .Include(r => r.Booking)
            .OrderByDescending(r => r.Date)
            .ToListAsync();

        return records.Select(r => MapRecordToViewModel(r)).ToList();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting all usage records");
        throw;
      }
    }

    public async Task<IEnumerable<CraneUsageRecordViewModel>> GetUsageRecordsByBookingIdAsync(int bookingId)
    {
      try
      {
        var records = await _context.CraneUsageRecords
            .Where(r => r.BookingId == bookingId)
            .Include(r => r.Booking)
            .OrderByDescending(r => r.Date)
            .ToListAsync();

        return records.Select(r => MapRecordToViewModel(r)).ToList();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting usage records for booking {BookingId}", bookingId);
        throw;
      }
    }

    public async Task<CraneUsageRecordViewModel> GetUsageRecordByIdAsync(int id)
    {
      try
      {
        var record = await _context.CraneUsageRecords
            .Include(r => r.Booking)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (record == null)
        {
          throw new KeyNotFoundException($"Usage record with ID {id} not found");
        }

        return MapRecordToViewModel(record);
      }
      catch (Exception ex) when (!(ex is KeyNotFoundException))
      {
        _logger.LogError(ex, "Error getting usage record with ID {RecordId}", id);
        throw;
      }
    }

    public async Task<CraneUsageRecordViewModel> CreateUsageRecordAsync(CraneUsageRecordCreateViewModel viewModel, string createdBy)
    {
      try
      {
        // Validate booking exists
        var booking = await _context.Bookings.FindAsync(viewModel.BookingId);
        if (booking == null)
        {
          throw new KeyNotFoundException($"Booking with ID {viewModel.BookingId} not found");
        }

        // Validate subcategory exists
        var subcategory = await _context.UsageSubcategories.FindAsync(viewModel.SubcategoryId);
        if (subcategory == null)
        {
          throw new KeyNotFoundException($"Subcategory with ID {viewModel.SubcategoryId} not found");
        }

        // Parse start time
        if (!TryParseTimeSpan(viewModel.StartTime, out TimeSpan startTime))
        {
          throw new ArgumentException("Invalid start time format. Expected format is HH:MM");
        }

        // Parse end time
        if (!TryParseTimeSpan(viewModel.EndTime, out TimeSpan endTime))
        {
          throw new ArgumentException("Invalid end time format. Expected format is HH:MM");
        }

        // Create new record
        var record = new CraneUsageRecord
        {
          BookingId = viewModel.BookingId,
          Date = viewModel.Date.Date, // Use only the date part
          Category = viewModel.Category,
          SubcategoryId = viewModel.SubcategoryId,
          StartTime = startTime,
          EndTime = endTime,
          CreatedAt = DateTime.Now,
          CreatedBy = createdBy
        };

        _context.CraneUsageRecords.Add(record);
        await _context.SaveChangesAsync();

        return await GetUsageRecordByIdAsync(record.Id);
      }
      catch (Exception ex) when (!(ex is KeyNotFoundException || ex is ArgumentException))
      {
        _logger.LogError(ex, "Error creating usage record for booking {BookingId}", viewModel.BookingId);
        throw;
      }
    }

    public async Task<CraneUsageRecordViewModel> UpdateUsageRecordAsync(int id, CraneUsageRecordUpdateViewModel viewModel, string updatedBy)
    {
      try
      {
        var record = await _context.CraneUsageRecords.FindAsync(id);
        if (record == null)
        {
          throw new KeyNotFoundException($"Usage record with ID {id} not found");
        }

        // Validate subcategory exists
        var subcategory = await _context.UsageSubcategories.FindAsync(viewModel.SubcategoryId);
        if (subcategory == null)
        {
          throw new KeyNotFoundException($"Subcategory with ID {viewModel.SubcategoryId} not found");
        }

        // Parse start time
        if (!TryParseTimeSpan(viewModel.StartTime, out TimeSpan startTime))
        {
          throw new ArgumentException("Invalid start time format. Expected format is HH:MM");
        }

        // Parse end time
        if (!TryParseTimeSpan(viewModel.EndTime, out TimeSpan endTime))
        {
          throw new ArgumentException("Invalid end time format. Expected format is HH:MM");
        }

        // Update record
        record.Category = viewModel.Category;
        record.SubcategoryId = viewModel.SubcategoryId;
        record.StartTime = startTime;
        record.EndTime = endTime;
        record.UpdatedAt = DateTime.Now;
        record.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();

        return await GetUsageRecordByIdAsync(record.Id);
      }
      catch (Exception ex) when (!(ex is KeyNotFoundException || ex is ArgumentException))
      {
        _logger.LogError(ex, "Error updating usage record with ID {RecordId}", id);
        throw;
      }
    }

    public async Task DeleteUsageRecordAsync(int id)
    {
      try
      {
        var record = await _context.CraneUsageRecords.FindAsync(id);
        if (record == null)
        {
          throw new KeyNotFoundException($"Usage record with ID {id} not found");
        }

        _context.CraneUsageRecords.Remove(record);
        await _context.SaveChangesAsync();
      }
      catch (Exception ex) when (!(ex is KeyNotFoundException))
      {
        _logger.LogError(ex, "Error deleting usage record with ID {RecordId}", id);
        throw;
      }
    }

    public async Task<UsageSummaryViewModel> GetUsageSummaryByBookingIdAsync(int bookingId)
    {
      try
      {
        // Get booking details
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null)
        {
          throw new KeyNotFoundException($"Booking with ID {bookingId} not found");
        }

        // Get all usage records for this booking
        var records = await GetUsageRecordsByBookingIdAsync(bookingId);
        var recordsList = records.ToList();

        // Initialize summary
        var summary = new UsageSummaryViewModel
        {
          BookingId = bookingId,
          BookingNumber = booking.BookingNumber,
          Date = booking.StartDate,
          UsageRecords = recordsList
        };

        // Calculate totals by category
        TimeSpan operatingTime = TimeSpan.Zero;
        TimeSpan delayTime = TimeSpan.Zero;
        TimeSpan standbyTime = TimeSpan.Zero;
        TimeSpan serviceTime = TimeSpan.Zero;
        TimeSpan breakdownTime = TimeSpan.Zero;

        foreach (var record in recordsList)
        {
          switch (record.Category)
          {
            case UsageCategory.Operating:
              operatingTime = operatingTime.Add(record.Duration);
              break;
            case UsageCategory.Delay:
              delayTime = delayTime.Add(record.Duration);
              break;
            case UsageCategory.Standby:
              standbyTime = standbyTime.Add(record.Duration);
              break;
            case UsageCategory.Service:
              serviceTime = serviceTime.Add(record.Duration);
              break;
            case UsageCategory.Breakdown:
              breakdownTime = breakdownTime.Add(record.Duration);
              break;
          }
        }

        // Set category totals
        summary.TotalOperatingTime = operatingTime.ToString(@"hh\:mm\:ss");
        summary.TotalDelayTime = delayTime.ToString(@"hh\:mm\:ss");
        summary.TotalStandbyTime = standbyTime.ToString(@"hh\:mm\:ss");
        summary.TotalServiceTime = serviceTime.ToString(@"hh\:mm\:ss");
        summary.TotalBreakdownTime = breakdownTime.ToString(@"hh\:mm\:ss");

        // Calculate KPI metrics
        TimeSpan totalUnavailableTime = serviceTime.Add(breakdownTime);
        TimeSpan totalAvailableTime = operatingTime.Add(delayTime).Add(standbyTime);

        summary.TotalUnavailableTime = totalUnavailableTime.ToString(@"hh\:mm\:ss");
        summary.TotalAvailableTime = totalAvailableTime.ToString(@"hh\:mm\:ss");
        summary.TotalUsageTime = operatingTime.ToString(@"hh\:mm\:ss");

        // Calculate total minutes for percentage calculations
        double totalMinutes = (totalAvailableTime + totalUnavailableTime).TotalMinutes;
        double availableMinutes = totalAvailableTime.TotalMinutes;
        double operatingMinutes = operatingTime.TotalMinutes;

        // Calculate percentages
        if (totalMinutes > 0)
        {
          summary.AvailabilityPercentage = (decimal)(availableMinutes / totalMinutes * 100);
        }

        if (availableMinutes > 0)
        {
          summary.UtilisationPercentage = (decimal)(operatingMinutes / availableMinutes * 100);
        }

        return summary;
      }
      catch (Exception ex) when (!(ex is KeyNotFoundException))
      {
        _logger.LogError(ex, "Error getting usage summary for booking {BookingId}", bookingId);
        throw;
      }
    }

    public async Task<IEnumerable<UsageSubcategoryViewModel>> GetSubcategoriesByCategoryAsync(UsageCategory category)
    {
      try
      {
        var subcategories = await _context.UsageSubcategories
            .Where(s => s.Category == category && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return subcategories.Select(s => new UsageSubcategoryViewModel
        {
          Id = s.Id,
          Category = s.Category,
          Name = s.Name,
          Description = s.Description,
          IsActive = s.IsActive
        }).ToList();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting subcategories for category {Category}", category);
        throw;
      }
    }

    // Helper method to map entity to ViewModel
    private CraneUsageRecordViewModel MapRecordToViewModel(CraneUsageRecord record)
    {
      // Get subcategory name
      string subcategoryName = "Unknown";
      var subcategory = _context.UsageSubcategories.Find(record.SubcategoryId);
      if (subcategory != null)
      {
        subcategoryName = subcategory.Name;
      }

      // Calculate duration (it's automatically calculated in the model)
      TimeSpan duration = record.Duration;

      return new CraneUsageRecordViewModel
      {
        Id = record.Id,
        BookingId = record.BookingId,
        BookingNumber = record.Booking?.BookingNumber ?? "Unknown",
        Date = record.Date,
        Category = record.Category,
        CategoryName = record.Category.ToString(),
        SubcategoryId = record.SubcategoryId,
        SubcategoryName = subcategoryName,
        StartTime = record.StartTime,
        StartTimeFormatted = FormatTimeSpan(record.StartTime),
        EndTime = record.EndTime,
        EndTimeFormatted = FormatTimeSpan(record.EndTime),
        Duration = duration,
        DurationFormatted = FormatTimeSpan(duration),
        CreatedAt = record.CreatedAt,
        CreatedBy = record.CreatedBy,
        UpdatedAt = record.UpdatedAt,
        UpdatedBy = record.UpdatedBy
      };
    }

    // Helper method to parse timespan from HH:MM format
    private bool TryParseTimeSpan(string timeString, out TimeSpan result)
    {
      result = TimeSpan.Zero;

      if (string.IsNullOrEmpty(timeString))
      {
        return false;
      }

      // Try parse HH:MM format
      string[] parts = timeString.Split(':');
      if (parts.Length != 2)
      {
        return false;
      }

      if (!int.TryParse(parts[0], out int hours) || !int.TryParse(parts[1], out int minutes))
      {
        return false;
      }

      result = new TimeSpan(hours, minutes, 0);
      return true;
    }

    // Helper method to format timespan as HH:MM
    private string FormatTimeSpan(TimeSpan timeSpan)
    {
      return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}";
    }
  }
}
