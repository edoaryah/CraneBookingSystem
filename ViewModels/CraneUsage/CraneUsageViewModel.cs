// Tambahkan file ViewModels/CraneUsage/CraneUsageViewModel.cs

using AspnetCoreMvcFull.Models;
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.ViewModels.CraneUsage
{
  // ViewModel untuk menampilkan record penggunaan crane
  public class CraneUsageRecordViewModel
  {
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public UsageCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int SubcategoryId { get; set; }
    public string SubcategoryName { get; set; } = string.Empty;

    // Fields waktu
    public TimeSpan StartTime { get; set; }
    public string StartTimeFormatted { get; set; } = string.Empty;
    public TimeSpan EndTime { get; set; }
    public string EndTimeFormatted { get; set; } = string.Empty;

    // Durasi (dikalkulasi)
    public TimeSpan Duration { get; set; }
    public string DurationFormatted { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
  }

  // ViewModel untuk membuat record penggunaan crane baru
  public class CraneUsageRecordCreateViewModel
  {
    [Required]
    public int BookingId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required]
    public UsageCategory Category { get; set; }

    [Required]
    public int SubcategoryId { get; set; }

    [Required]
    [DataType(DataType.Time)]
    public string StartTime { get; set; } = string.Empty; // Format: "HH:MM"

    [Required]
    [DataType(DataType.Time)]
    public string EndTime { get; set; } = string.Empty; // Format: "HH:MM"
  }

  // ViewModel untuk memperbarui record penggunaan crane
  public class CraneUsageRecordUpdateViewModel
  {
    [Required]
    public UsageCategory Category { get; set; }

    [Required]
    public int SubcategoryId { get; set; }

    [Required]
    [DataType(DataType.Time)]
    public string StartTime { get; set; } = string.Empty; // Format: "HH:MM"

    [Required]
    [DataType(DataType.Time)]
    public string EndTime { get; set; } = string.Empty; // Format: "HH:MM"
  }

  // ViewModel untuk subcategory penggunaan
  public class UsageSubcategoryViewModel
  {
    public int Id { get; set; }
    public UsageCategory Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
  }

  // ViewModel untuk ringkasan penggunaan
  public class UsageSummaryViewModel
  {
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<CraneUsageRecordViewModel> UsageRecords { get; set; } = new List<CraneUsageRecordViewModel>();

    // Total jam berdasarkan kategori
    public string TotalOperatingTime { get; set; } = "00:00:00";
    public string TotalDelayTime { get; set; } = "00:00:00";
    public string TotalStandbyTime { get; set; } = "00:00:00";
    public string TotalServiceTime { get; set; } = "00:00:00";
    public string TotalBreakdownTime { get; set; } = "00:00:00";

    // Metrik KPI
    public string TotalAvailableTime { get; set; } = "00:00:00";
    public string TotalUnavailableTime { get; set; } = "00:00:00";
    public string TotalUsageTime { get; set; } = "00:00:00";
    public decimal AvailabilityPercentage { get; set; } = 0;
    public decimal UtilisationPercentage { get; set; } = 0;
  }
}
