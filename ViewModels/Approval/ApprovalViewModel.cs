// [ViewModels/ApprovalViewModel.cs]
// ViewModel untuk halaman approval.
using AspnetCoreMvcFull.DTOs;

namespace AspnetCoreMvcFull.ViewModels
{
  public class ApprovalViewModel
  {
    public int BookingId { get; set; }
    public string BadgeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public BookingDetailDto? BookingDetails { get; set; }
    public string RejectReason { get; set; } = string.Empty;
  }
}
