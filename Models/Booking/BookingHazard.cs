using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{
  public class BookingHazard
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public int BookingId { get; set; }

    [Required]
    public int HazardId { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Hazard? Hazard { get; set; }
  }
}
