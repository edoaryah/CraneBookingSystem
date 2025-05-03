// reset
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models
{
  public class CraneUsageRecord
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public int BookingId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public UsageCategory Category { get; set; }

    [Required]
    public int SubcategoryId { get; set; }

    [Required]
    public TimeSpan Duration { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [StringLength(100)]
    public required string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }

    [StringLength(100)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual Booking? Booking { get; set; }
  }
}
