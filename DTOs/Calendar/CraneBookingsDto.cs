namespace AspnetCoreMvcFull.DTOs
{
  public class CraneBookingsDto
  {
    public required string CraneId { get; set; }
    public int Capacity { get; set; }
    public List<BookingCalendarDto> Bookings { get; set; } = new List<BookingCalendarDto>();
    public List<MaintenanceCalendarDto> MaintenanceSchedules { get; set; } = new List<MaintenanceCalendarDto>();
  }
}
