// Services/CraneUsage/CraneUsageService.cs
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.ViewModels.CraneUsage;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AspnetCoreMvcFull.Services.CraneUsage
{
  public class CraneUsageService : ICraneUsageService
  {
    private readonly AppDbContext _context;

    public CraneUsageService(AppDbContext context)
    {
      _context = context;
    }

    public async Task<CraneUsageListViewModel> GetFilteredUsageRecordsAsync(CraneUsageFilterViewModel filter)
    {
      var query = _context.CraneUsageRecords
          .Include(r => r.Crane)
          .Include(r => r.UsageSubcategory)
          .Include(r => r.Booking)
          .Include(r => r.MaintenanceSchedule)
          .AsQueryable();

      // Apply filters
      if (filter.CraneId.HasValue && filter.CraneId > 0)
      {
        query = query.Where(r => r.CraneId == filter.CraneId);
      }

      if (filter.StartDate.HasValue)
      {
        query = query.Where(r => r.StartTime >= filter.StartDate.Value);
      }

      if (filter.EndDate.HasValue)
      {
        query = query.Where(r => r.StartTime <= filter.EndDate.Value);
      }

      if (filter.Category.HasValue)
      {
        query = query.Where(r => r.Category == filter.Category.Value);
      }

      // Order by most recent first
      query = query.OrderByDescending(r => r.StartTime);

      var records = await query.ToListAsync();

      var viewModel = new CraneUsageListViewModel
      {
        UsageRecords = records.Select(MapToViewModel).ToList(),
        Filter = filter
      };

      // Populate filter dropdowns
      filter.CraneList = await GetCraneListAsync();
      filter.CategoryList = GetCategoryList();

      return viewModel;
    }

    public async Task<List<CraneUsageRecordViewModel>> GetAllUsageRecordsAsync(CraneUsageFilterViewModel filter)
    {
      var query = _context.CraneUsageRecords
          .Include(r => r.Crane)
          .Include(r => r.UsageSubcategory)
          .Include(r => r.Booking)
          .Include(r => r.MaintenanceSchedule)
          .AsQueryable();

      // Apply filters
      if (filter.CraneId.HasValue && filter.CraneId > 0)
      {
        query = query.Where(r => r.CraneId == filter.CraneId);
      }

      if (filter.StartDate.HasValue)
      {
        query = query.Where(r => r.StartTime >= filter.StartDate.Value);
      }

      if (filter.EndDate.HasValue)
      {
        query = query.Where(r => r.EndTime <= filter.EndDate.Value);
      }

      if (filter.Category.HasValue)
      {
        query = query.Where(r => r.Category == filter.Category.Value);
      }

      var records = await query.ToListAsync();
      return records.Select(MapToViewModel).ToList();
    }

    public async Task<CraneUsageRecordViewModel> GetUsageRecordByIdAsync(int id)
    {
      var record = await _context.CraneUsageRecords
          .Include(r => r.Crane)
          .Include(r => r.UsageSubcategory)
          .Include(r => r.Booking)
          .Include(r => r.MaintenanceSchedule)
          .FirstOrDefaultAsync(r => r.Id == id);

      if (record == null)
      {
        return new CraneUsageRecordViewModel();
      }

      var viewModel = MapToViewModel(record);

      // Populate dropdowns for form
      viewModel.CraneList = await GetCraneListAsync();
      viewModel.CategoryList = GetCategoryList();
      viewModel.SubcategoryList = await GetSubcategoriesByCategoryAsync(record.Category);
      viewModel.BookingList = await GetAvailableBookingsAsync(record.CraneId, record.StartTime, record.EndTime, id);
      viewModel.MaintenanceList = await GetAvailableMaintenanceSchedulesAsync(record.CraneId, record.StartTime, record.EndTime, id);

      return viewModel;
    }

    public async Task<CraneUsageRecordViewModel> CreateUsageRecordAsync(CraneUsageRecordViewModel model, string createdBy)
    {
      var record = new CraneUsageRecord
      {
        CraneId = model.CraneId,
        StartTime = model.StartTime,
        EndTime = model.EndTime,
        Category = model.Category,
        UsageSubcategoryId = model.UsageSubcategoryId,
        BookingId = model.BookingId,
        MaintenanceScheduleId = model.MaintenanceScheduleId,
        Notes = model.Notes,
        OperatorName = model.OperatorName,
        CreatedAt = DateTime.Now,
        CreatedBy = createdBy
      };

      _context.CraneUsageRecords.Add(record);
      await _context.SaveChangesAsync();

      return MapToViewModel(record);
    }

    public async Task<CraneUsageRecordViewModel> UpdateUsageRecordAsync(CraneUsageRecordViewModel model, string updatedBy)
    {
      var record = await _context.CraneUsageRecords.FindAsync(model.Id);
      if (record == null)
      {
        throw new KeyNotFoundException($"Usage record with ID {model.Id} not found");
      }

      // Update properties
      record.CraneId = model.CraneId;
      record.StartTime = model.StartTime;
      record.EndTime = model.EndTime;
      record.Category = model.Category;
      record.UsageSubcategoryId = model.UsageSubcategoryId;
      record.BookingId = model.BookingId;
      record.MaintenanceScheduleId = model.MaintenanceScheduleId;
      record.Notes = model.Notes;
      record.OperatorName = model.OperatorName;

      await _context.SaveChangesAsync();

      return MapToViewModel(record);
    }

    public async Task<bool> DeleteUsageRecordAsync(int id)
    {
      var record = await _context.CraneUsageRecords.FindAsync(id);
      if (record == null)
      {
        return false;
      }

      _context.CraneUsageRecords.Remove(record);
      await _context.SaveChangesAsync();
      return true;
    }

    public async Task<CraneUsageVisualizationViewModel> GetUsageVisualizationDataAsync(int craneId, DateTime date)
    {
      var startDate = date.Date;
      var endDate = startDate.AddDays(1);

      var crane = await _context.Cranes.FindAsync(craneId);
      if (crane == null)
      {
        throw new KeyNotFoundException($"Crane with ID {craneId} not found");
      }

      // Get all usage records for the specified crane and date
      var usageRecords = await _context.CraneUsageRecords
          .Include(r => r.UsageSubcategory)
          .Include(r => r.Booking)
          .Include(r => r.MaintenanceSchedule)
          .Where(r => r.CraneId == craneId &&
                    ((r.StartTime >= startDate && r.StartTime < endDate) ||
                     (r.EndTime > startDate && r.EndTime <= endDate) ||
                     (r.StartTime <= startDate && r.EndTime >= endDate)))
          .OrderBy(r => r.StartTime)
          .ToListAsync();

      var viewModel = new CraneUsageVisualizationViewModel
      {
        CraneId = craneId,
        Date = date,
        CraneName = crane.Code,
        CraneList = await GetCraneListAsync()
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

      // Process each usage record and update hourly data
      foreach (var record in usageRecords)
      {
        var recordStartHour = GetHourInDate(record.StartTime, startDate);
        var recordEndHour = GetHourInDate(record.EndTime, startDate);

        for (int hour = recordStartHour; hour < recordEndHour; hour++)
        {
          if (hour >= 0 && hour < 24) // Ensure hour is within range
          {
            hourlyData[hour].Category = record.Category.ToString();
            hourlyData[hour].SubcategoryName = record.UsageSubcategory?.Name ?? string.Empty;
            hourlyData[hour].ColorCode = GetCategoryColorCode(record.Category);
            hourlyData[hour].Notes = record.Notes ?? string.Empty;

            if (record.BookingId.HasValue && record.Booking != null)
            {
              hourlyData[hour].BookingNumber = record.Booking.BookingNumber;
            }

            if (record.MaintenanceScheduleId.HasValue && record.MaintenanceSchedule != null)
            {
              hourlyData[hour].MaintenanceTitle = record.MaintenanceSchedule.Title;
            }
          }
        }
      }

      viewModel.HourlyData = hourlyData;
      return viewModel;
    }

    public async Task<List<SelectListItem>> GetSubcategoriesByCategoryAsync(UsageCategory category)
    {
      var subcategories = await _context.UsageSubcategories
          .Where(s => s.Category == category && s.IsActive)
          .OrderBy(s => s.Name)
          .ToListAsync();

      return subcategories.Select(s => new SelectListItem
      {
        Value = s.Id.ToString(),
        Text = s.Name
      }).ToList();
    }

    public async Task<List<SelectListItem>> GetAvailableBookingsAsync(int craneId, DateTime startTime, DateTime endTime, int? currentRecordId = null)
    {
      var overlappingBookings = await _context.Bookings
          .Where(b => b.CraneId == craneId &&
                    b.Status == BookingStatus.PICApproved &&
                    ((b.StartDate <= startTime && b.EndDate >= startTime) ||
                     (b.StartDate <= endTime && b.EndDate >= endTime) ||
                     (b.StartDate >= startTime && b.EndDate <= endTime)))
          .ToListAsync();

      return overlappingBookings.Select(b => new SelectListItem
      {
        Value = b.Id.ToString(),
        Text = $"{b.BookingNumber} - {b.Name} ({b.StartDate:dd/MM/yyyy HH:mm} - {b.EndDate:dd/MM/yyyy HH:mm})"
      }).ToList();
    }

    public async Task<List<SelectListItem>> GetAvailableMaintenanceSchedulesAsync(int craneId, DateTime startTime, DateTime endTime, int? currentRecordId = null)
    {
      var overlappingMaintenance = await _context.MaintenanceSchedules
          .Where(m => m.CraneId == craneId &&
                    ((m.StartDate <= startTime && m.EndDate >= startTime) ||
                     (m.StartDate <= endTime && m.EndDate >= endTime) ||
                     (m.StartDate >= startTime && m.EndDate <= endTime)))
          .ToListAsync();

      return overlappingMaintenance.Select(m => new SelectListItem
      {
        Value = m.Id.ToString(),
        Text = $"{m.Title} ({m.StartDate:dd/MM/yyyy HH:mm} - {m.EndDate:dd/MM/yyyy HH:mm})"
      }).ToList();
    }

    // Helper methods
    private async Task<List<SelectListItem>> GetCraneListAsync()
    {
      var cranes = await _context.Cranes.OrderBy(c => c.Code).ToListAsync();
      return cranes.Select(c => new SelectListItem
      {
        Value = c.Id.ToString(),
        Text = $"{c.Code} - {c.Capacity} Ton"
      }).ToList();
    }

    private List<SelectListItem> GetCategoryList()
    {
      return Enum.GetValues(typeof(UsageCategory))
          .Cast<UsageCategory>()
          .Select(c => new SelectListItem
          {
            Value = ((int)c).ToString(),
            Text = c.ToString()
          }).ToList();
    }

    private CraneUsageRecordViewModel MapToViewModel(CraneUsageRecord record)
    {
      return new CraneUsageRecordViewModel
      {
        Id = record.Id,
        CraneId = record.CraneId,
        StartTime = record.StartTime,
        EndTime = record.EndTime,
        Category = record.Category,
        UsageSubcategoryId = record.UsageSubcategoryId,
        BookingId = record.BookingId,
        MaintenanceScheduleId = record.MaintenanceScheduleId,
        Notes = record.Notes,
        OperatorName = record.OperatorName,
        CraneName = record.Crane?.Code ?? string.Empty,
        CategoryName = record.Category.ToString(),
        SubcategoryName = record.UsageSubcategory?.Name ?? string.Empty,
        BookingNumber = record.Booking?.BookingNumber,
        MaintenanceTitle = record.MaintenanceSchedule?.Title,
        CreatedAt = record.CreatedAt,
        CreatedBy = record.CreatedBy
      };
    }

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

    private int GetHourInDate(DateTime time, DateTime baseDate)
    {
      // If time is before base date, return 0
      if (time < baseDate)
      {
        return 0;
      }

      // If time is after base date + 1 day, return 24
      if (time >= baseDate.AddDays(1))
      {
        return 24;
      }

      // Calculate hour within the day
      var diff = time - baseDate;
      return (int)Math.Floor(diff.TotalHours);
    }
  }
}
