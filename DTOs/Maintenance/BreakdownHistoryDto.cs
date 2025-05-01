// DTOs/Breakdown/BreakdownHistoryDto.cs
namespace AspnetCoreMvcFull.DTOs
{
  public class BreakdownHistoryDto
  {
    public int Id { get; set; }
    public int CraneId { get; set; }
    public string CraneCode { get; set; } = string.Empty;
    public int CraneCapacity { get; set; }
    public DateTime UrgentStartTime { get; set; }
    public DateTime UrgentEndTime { get; set; }
    public DateTime? ActualUrgentEndTime { get; set; }
    public int EstimatedUrgentDays { get; set; }
    public int EstimatedUrgentHours { get; set; }
    public string Reasons { get; set; } = string.Empty;
  }
}
