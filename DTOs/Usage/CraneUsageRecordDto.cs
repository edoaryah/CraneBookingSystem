using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.DTOs.Usage
{
  public class CraneUsageRecordDto
  {
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public UsageCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int SubcategoryId { get; set; }
    public string SubcategoryName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string DurationFormatted { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
  }

  public class CraneUsageRecordCreateDto
  {
    public int BookingId { get; set; }
    public DateTime Date { get; set; }
    public UsageCategory Category { get; set; }
    public int SubcategoryId { get; set; }
    public string Duration { get; set; } = string.Empty; // Format: "HH:MM"
  }

  public class CraneUsageRecordUpdateDto
  {
    public UsageCategory Category { get; set; }
    public int SubcategoryId { get; set; }
    public string Duration { get; set; } = string.Empty; // Format: "HH:MM"
  }

  public class UsageSubcategoryDto
  {
    public int Id { get; set; }
    public UsageCategory Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
  }

  public class UsageSummaryDto
  {
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<CraneUsageRecordDto> UsageRecords { get; set; } = new List<CraneUsageRecordDto>();

    // Total hours by category
    public string TotalOperatingTime { get; set; } = "00:00:00";
    public string TotalDelayTime { get; set; } = "00:00:00";
    public string TotalStandbyTime { get; set; } = "00:00:00";
    public string TotalServiceTime { get; set; } = "00:00:00";
    public string TotalBreakdownTime { get; set; } = "00:00:00";

    // KPI metrics
    public string TotalAvailableTime { get; set; } = "00:00:00";
    public string TotalUnavailableTime { get; set; } = "00:00:00";
    public string TotalUsageTime { get; set; } = "00:00:00";
    public decimal AvailabilityPercentage { get; set; } = 0;
    public decimal UtilisationPercentage { get; set; } = 0;
  }
}
