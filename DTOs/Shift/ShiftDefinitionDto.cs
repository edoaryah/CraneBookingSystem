namespace AspnetCoreMvcFull.DTOs
{
  public class ShiftDefinitionDto
  {
    public int Id { get; set; }
    public required string Name { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
  }
}
