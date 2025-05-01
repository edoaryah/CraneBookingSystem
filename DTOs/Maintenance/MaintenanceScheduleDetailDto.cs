namespace AspnetCoreMvcFull.DTOs
{
  public class MaintenanceScheduleDetailDto
  {
    public int Id { get; set; }
    public int CraneId { get; set; }
    public string? CraneCode { get; set; }
    public required string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string CreatedBy { get; set; }
    public List<MaintenanceScheduleShiftDto> Shifts { get; set; } = new List<MaintenanceScheduleShiftDto>();
  }
}
