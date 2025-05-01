// DTOs/Booking/BookingDetailDto.cs
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.DTOs
{
  public class BookingDetailDto
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

    // Status approval
    public BookingStatus Status { get; set; }

    // Manager approval info
    public string? ManagerName { get; set; }
    public DateTime? ManagerApprovalTime { get; set; }
    public string? ManagerRejectReason { get; set; }

    // PIC approval info
    public string? ApprovedByPIC { get; set; }
    public DateTime? ApprovedAtByPIC { get; set; }
    public string? PICRejectReason { get; set; }

    // PIC completion info
    public string? DoneByPIC { get; set; }
    public DateTime? DoneAt { get; set; }

    // Cancellation properties
    public BookingCancelledBy CancelledBy { get; set; }
    public string? CancelledByName { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelledReason { get; set; }

    // Revision tracking
    public int RevisionCount { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public string LastModifiedBy { get; set; } = string.Empty;

    public List<BookingShiftDto> Shifts { get; set; } = new List<BookingShiftDto>();
    public List<BookingItemDto> Items { get; set; } = new List<BookingItemDto>();
    public List<HazardDto> SelectedHazards { get; set; } = new List<HazardDto>();
    public string? CustomHazard { get; set; }
  }
}
