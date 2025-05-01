namespace AspnetCoreMvcFull.DTOs
{
  public class BookingItemDto
  {
    public int Id { get; set; }
    public required string ItemName { get; set; }
    public decimal Weight { get; set; }
    public decimal Height { get; set; }
    public int Quantity { get; set; }
  }
}
