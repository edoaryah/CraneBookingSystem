namespace AspnetCoreMvcFull.DTOs
{
  public class MaintenanceScheduleCreateDto
  {
    public int CraneId { get; set; }
    public required string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }
    public required string CreatedBy { get; set; }
    public List<DailyShiftSelectionDto> ShiftSelections { get; set; } = new List<DailyShiftSelectionDto>();
  }
}
