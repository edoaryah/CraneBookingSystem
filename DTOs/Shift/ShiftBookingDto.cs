namespace AspnetCoreMvcFull.DTOs
{
  public class ShiftBookingDto
  {
    public int ShiftDefinitionId { get; set; }
    public string? ShiftName { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
  }
}
