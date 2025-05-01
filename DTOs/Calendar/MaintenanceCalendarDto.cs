namespace AspnetCoreMvcFull.DTOs
{
  public class MaintenanceCalendarDto
  {
    public int Id { get; set; }
    public required string Title { get; set; }
    public DateTime Date { get; set; }
    public List<ShiftBookingDto> Shifts { get; set; } = new List<ShiftBookingDto>();
  }
}
