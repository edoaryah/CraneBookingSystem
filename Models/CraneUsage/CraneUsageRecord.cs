// using System;
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace AspnetCoreMvcFull.Models
// {
//   public class CraneUsageRecord
//   {
//     [Key]
//     public int Id { get; set; }

//     [Required]
//     public int CraneId { get; set; }

//     // Relasi opsional dengan Booking atau MaintenanceSchedule
//     public int? BookingId { get; set; }
//     public int? MaintenanceScheduleId { get; set; }

//     [Required]
//     public DateTime StartTime { get; set; }

//     [Required]
//     public DateTime EndTime { get; set; }

//     [Required]
//     public UsageCategory Category { get; set; }  // Menggunakan enum UsageCategory yang sudah ada

//     public int? UsageSubcategoryId { get; set; }  // Relasi ke UsageSubcategory

//     [StringLength(500)]
//     public string? Notes { get; set; }

//     // Status finalisasi (untuk menandakan aktivitas setiap hari sudah final)
//     public bool IsFinal { get; set; } = false;
//     public DateTime? FinalizedAt { get; set; }
//     public string? FinalizedBy { get; set; }

//     // Audit properties
//     [Required]
//     public DateTime CreatedAt { get; set; } = DateTime.Now;

//     [Required]
//     [StringLength(100)]
//     public required string CreatedBy { get; set; }

//     public DateTime? UpdatedAt { get; set; }

//     [StringLength(100)]
//     public string? UpdatedBy { get; set; }

//     // Navigation properties
//     [ForeignKey("CraneId")]
//     public virtual Crane Crane { get; set; }

//     [ForeignKey("BookingId")]
//     public virtual Booking? Booking { get; set; }

//     [ForeignKey("MaintenanceScheduleId")]
//     public virtual MaintenanceSchedule? MaintenanceSchedule { get; set; }

//     [ForeignKey("UsageSubcategoryId")]
//     public virtual UsageSubcategory? UsageSubcategory { get; set; }

//     // Computed property untuk mendapatkan tanggal (untuk pengelompokan harian)
//     [NotMapped]
//     public DateTime Date => StartTime.Date;

//     // Computed property untuk menghitung durasi dalam menit
//     [NotMapped]
//     public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;
//   }
// }

// Models/CraneUsageRecord.cs
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{
  public class CraneUsageRecord
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public int CraneId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    public UsageCategory Category { get; set; }

    [Required]
    public int UsageSubcategoryId { get; set; }

    // Booking or Maintenance reference (optional references)
    public int? BookingId { get; set; }
    public int? MaintenanceScheduleId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    // Operator yang menjalankan crane
    [StringLength(100)]
    public string? OperatorName { get; set; }

    // Audit fields
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required]
    [StringLength(100)]
    public required string CreatedBy { get; set; }

    // Navigation properties
    public virtual Crane? Crane { get; set; }
    public virtual Booking? Booking { get; set; }
    public virtual MaintenanceSchedule? MaintenanceSchedule { get; set; }
    public virtual UsageSubcategory? UsageSubcategory { get; set; }
  }
}
