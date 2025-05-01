namespace AspnetCoreMvcFull.DTOs
{
  public class DailyShiftSelectionDto
  {
    public DateTime Date { get; set; }
    public List<int> SelectedShiftIds { get; set; } = new List<int>();
  }
}
