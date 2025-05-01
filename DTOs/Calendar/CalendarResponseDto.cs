namespace AspnetCoreMvcFull.DTOs
{
  public class CalendarResponseDto
  {
    public required WeekRangeDto WeekRange { get; set; }
    public List<CraneBookingsDto> Cranes { get; set; } = new List<CraneBookingsDto>();
  }
}
