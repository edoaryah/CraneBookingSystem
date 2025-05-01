namespace AspnetCoreMvcFull.DTOs
{
  // Existing HazardDto
  public class HazardDto
  {
    public int Id { get; set; }
    public required string Name { get; set; }
  }

  // For Creating a new Hazard
  public class HazardCreateDto
  {
    public required string Name { get; set; }
  }

  // For Updating an existing Hazard
  public class HazardUpdateDto
  {
    public required string Name { get; set; }
  }
}
