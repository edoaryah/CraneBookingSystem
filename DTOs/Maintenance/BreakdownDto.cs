namespace AspnetCoreMvcFull.DTOs
{
  public class BreakdownDto
  {
    public int Id { get; set; }
    public int CraneId { get; set; }
    public DateTime UrgentStartTime { get; set; }
    public int EstimatedUrgentDays { get; set; }
    public int EstimatedUrgentHours { get; set; }
    public DateTime UrgentEndTime { get; set; }
    public DateTime? ActualUrgentEndTime { get; set; }
    public string? HangfireJobId { get; set; }
    public required string Reasons { get; set; }
  }
}
