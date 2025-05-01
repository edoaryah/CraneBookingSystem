namespace AspnetCoreMvcFull.DTOs
{
  public class ShiftDefinitionUpdateDto
  {
    public required string Name { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
  }
}
