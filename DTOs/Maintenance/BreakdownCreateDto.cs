namespace AspnetCoreMvcFull.DTOs
{
  public class BreakdownCreateDto
  {
    public DateTime UrgentStartTime { get; set; } = DateTime.Now;
    public int EstimatedUrgentDays { get; set; }
    public int EstimatedUrgentHours { get; set; }
    public required string Reasons { get; set; }
  }
}
