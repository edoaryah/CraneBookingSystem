using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.DTOs.Usage;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services.Usage
{
  public class CraneUsageRecordService : ICraneUsageRecordService
  {
    private readonly AppDbContext _context;
    private readonly ILogger<CraneUsageRecordService> _logger;

    public CraneUsageRecordService(AppDbContext context, ILogger<CraneUsageRecordService> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<IEnumerable<CraneUsageRecordDto>> GetAllUsageRecordsAsync()
    {
      try
      {
        var records = await _context.CraneUsageRecords
            .Include(r => r.Booking)
            .OrderByDescending(r => r.Date)
            .ToListAsync();

        return records.Select(r => MapRecordToDto(r)).ToList();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting all usage records");
        throw;
      }
    }

    public async Task<IEnumerable<CraneUsageRecordDto>> GetUsageRecordsByBookingIdAsync(int bookingId)
    {
      try
      {
        var records = await _context.CraneUsageRecords
            .Where(r => r.BookingId == bookingId)
            .Include(r => r.Booking)
            .OrderByDescending(r => r.Date)
            .ToListAsync();

        return records.Select(r => MapRecordToDto(r)).ToList();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting usage records for booking {BookingId}", bookingId);
        throw;
      }
    }

    public async Task<CraneUsageRecordDto> GetUsageRecordByIdAsync(int id)
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

        return MapRecordToDto(record);
      }
      catch (Exception ex) when (!(ex is KeyNotFoundException))
      {
        _logger.LogError(ex, "Error getting usage record with ID {RecordId}", id);
        throw;
      }
    }

    public async Task<CraneUsageRecordDto> CreateUsageRecordAsync(CraneUsageRecordCreateDto recordDto, string createdBy)
    {
      try
      {
        // Validate booking exists
        var booking = await _context.Bookings.FindAsync(recordDto.BookingId);
        if (booking == null)
        {
          throw new KeyNotFoundException($"Booking with ID {recordDto.BookingId} not found");
        }

        // Validate subcategory exists
        var subcategory = await _context.UsageSubcategories.FindAsync(recordDto.SubcategoryId);
        if (subcategory == null)
        {
          throw new KeyNotFoundException($"Subcategory with ID {recordDto.SubcategoryId} not found");
        }

        // Parse duration
        if (!TryParseTimeSpan(recordDto.Duration, out TimeSpan duration))
        {
          throw new ArgumentException("Invalid duration format. Expected format is HH:MM");
        }

        // Create new record
        var record = new CraneUsageRecord
        {
          BookingId = recordDto.BookingId,
          Date = recordDto.Date.Date, // Use only the date part
          Category = recordDto.Category,
          SubcategoryId = recordDto.SubcategoryId,
          Duration = duration,
          CreatedAt = DateTime.Now,
          CreatedBy = createdBy
        };

        _context.CraneUsageRecords.Add(record);
        await _context.SaveChangesAsync();

        return await GetUsageRecordByIdAsync(record.Id);
      }
      catch (Exception ex) when (!(ex is KeyNotFoundException || ex is ArgumentException))
      {
        _logger.LogError(ex, "Error creating usage record for booking {BookingId}", recordDto.BookingId);
        throw;
      }
    }

    public async Task<CraneUsageRecordDto> UpdateUsageRecordAsync(int id, CraneUsageRecordUpdateDto recordDto, string updatedBy)
    {
      try
      {
        var record = await _context.CraneUsageRecords.FindAsync(id);
        if (record == null)
        {
          throw new KeyNotFoundException($"Usage record with ID {id} not found");
        }

        // Validate subcategory exists
        var subcategory = await _context.UsageSubcategories.FindAsync(recordDto.SubcategoryId);
        if (subcategory == null)
        {
          throw new KeyNotFoundException($"Subcategory with ID {recordDto.SubcategoryId} not found");
        }

        // Parse duration
        if (!TryParseTimeSpan(recordDto.Duration, out TimeSpan duration))
        {
          throw new ArgumentException("Invalid duration format. Expected format is HH:MM");
        }

        // Update record
        record.Category = recordDto.Category;
        record.SubcategoryId = recordDto.SubcategoryId;
        record.Duration = duration;
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

    public async Task<UsageSummaryDto> GetUsageSummaryByBookingIdAsync(int bookingId)
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
        var summary = new UsageSummaryDto
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

    public async Task<IEnumerable<UsageSubcategoryDto>> GetSubcategoriesByCategoryAsync(UsageCategory category)
    {
      try
      {
        var subcategories = await _context.UsageSubcategories
            .Where(s => s.Category == category && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return subcategories.Select(s => new UsageSubcategoryDto
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

    // Helper method to map entity to DTO
    private CraneUsageRecordDto MapRecordToDto(CraneUsageRecord record)
    {
      // Get subcategory name
      string subcategoryName = "Unknown";
      var subcategory = _context.UsageSubcategories.Find(record.SubcategoryId);
      if (subcategory != null)
      {
        subcategoryName = subcategory.Name;
      }

      return new CraneUsageRecordDto
      {
        Id = record.Id,
        BookingId = record.BookingId,
        BookingNumber = record.Booking?.BookingNumber ?? "Unknown",
        Date = record.Date,
        Category = record.Category,
        CategoryName = record.Category.ToString(),
        SubcategoryId = record.SubcategoryId,
        SubcategoryName = subcategoryName,
        Duration = record.Duration,
        DurationFormatted = FormatTimeSpan(record.Duration),
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
