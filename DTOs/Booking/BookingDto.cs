namespace AspnetCoreMvcFull.DTOs
{
  public class BookingDto
  {
    public int Id { get; set; }
    public required string BookingNumber { get; set; }
    public required string Name { get; set; }
    public required string Department { get; set; }
    public int CraneId { get; set; }
    public string? CraneCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime SubmitTime { get; set; }
    public string? Location { get; set; }
    public string? ProjectSupervisor { get; set; }
    public string? CostCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Description { get; set; }
  }
}
