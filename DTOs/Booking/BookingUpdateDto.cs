namespace AspnetCoreMvcFull.DTOs
{
  public class BookingUpdateDto
  {
    public required string Name { get; set; }
    public required string Department { get; set; }
    public int CraneId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public string? ProjectSupervisor { get; set; }
    public string? CostCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Description { get; set; }
    public List<DailyShiftSelectionDto> ShiftSelections { get; set; } = new List<DailyShiftSelectionDto>();
    public List<BookingItemCreateDto> Items { get; set; } = new List<BookingItemCreateDto>();
    public List<int>? HazardIds { get; set; } = new List<int>();
    public string? CustomHazard { get; set; }
  }
}
