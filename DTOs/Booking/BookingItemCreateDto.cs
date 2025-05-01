namespace AspnetCoreMvcFull.DTOs
{
  public class BookingItemCreateDto
  {
    public required string ItemName { get; set; }
    public decimal Weight { get; set; }
    public decimal Height { get; set; }
    public int Quantity { get; set; }
  }
}
