namespace AspnetCoreMvcFull.DTOs
{
  public class MaintenanceScheduleShiftDto
  {
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int ShiftDefinitionId { get; set; }
    public string? ShiftName { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
  }
}
