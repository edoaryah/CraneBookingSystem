using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models
{
  public class CraneUsageSummary
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public int CraneId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    // Durasi (dalam menit) untuk setiap kategori
    public int OperatingMinutes { get; set; } = 0;
    public int DelayMinutes { get; set; } = 0;
    public int StandbyMinutes { get; set; } = 0;
    public int ServiceMinutes { get; set; } = 0;
    public int BreakdownMinutes { get; set; } = 0;

    // Status finalisasi harian
    public bool IsFinal { get; set; } = false;
    public DateTime? FinalizedAt { get; set; }
    public string? FinalizedBy { get; set; }

    // Audit properties
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required]
    [StringLength(100)]
    public required string CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [StringLength(100)]
    public string? UpdatedBy { get; set; }

    // Navigation property
    [ForeignKey("CraneId")]
    public virtual Crane Crane { get; set; }

    // Helper property untuk total menit dalam sehari (24 * 60 = 1440)
    [NotMapped]
    public int TotalMinutes => OperatingMinutes + DelayMinutes + StandbyMinutes + ServiceMinutes + BreakdownMinutes;

    // Helper property untuk sisa menit yang belum tercatat (akan ditandai sebagai standby)
    [NotMapped]
    public int RemainingMinutes => 1440 - TotalMinutes;
  }
}
