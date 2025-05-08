// ViewModels/CraneUsage/CraneUsageRecordViewModel.cs
using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.ViewModels.CraneUsage
{
  public class CraneUsageRecordViewModel
  {
    public int Id { get; set; }

    [Required(ErrorMessage = "Crane harus dipilih")]
    [Display(Name = "Crane")]
    public int CraneId { get; set; }

    [Required(ErrorMessage = "Waktu mulai harus diisi")]
    [Display(Name = "Waktu Mulai")]
    [DataType(DataType.DateTime)]
    public DateTime StartTime { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Waktu selesai harus diisi")]
    [Display(Name = "Waktu Selesai")]
    [DataType(DataType.DateTime)]
    public DateTime EndTime { get; set; } = DateTime.Now.AddHours(1);

    [Required(ErrorMessage = "Kategori harus dipilih")]
    [Display(Name = "Kategori")]
    public UsageCategory Category { get; set; }

    [Required(ErrorMessage = "Subkategori harus dipilih")]
    [Display(Name = "Subkategori")]
    public int UsageSubcategoryId { get; set; }

    [Display(Name = "Booking")]
    public int? BookingId { get; set; }

    [Display(Name = "Jadwal Maintenance")]
    public int? MaintenanceScheduleId { get; set; }

    [Display(Name = "Catatan")]
    [StringLength(500)]
    public string? Notes { get; set; }

    [Display(Name = "Operator")]
    [StringLength(100)]
    public string? OperatorName { get; set; }

    // Properti tambahan untuk dropdown
    public string CraneName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string SubcategoryName { get; set; } = string.Empty;
    public string? BookingNumber { get; set; }
    public string? MaintenanceTitle { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    // Dropdown lists untuk form
    public List<SelectListItem> CraneList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> CategoryList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> SubcategoryList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> BookingList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> MaintenanceList { get; set; } = new List<SelectListItem>();
  }

  public class CraneUsageFilterViewModel
  {
    public int? CraneId { get; set; }
    public DateTime? StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; } = DateTime.Today.AddDays(1).AddSeconds(-1);
    public UsageCategory? Category { get; set; }
    public List<SelectListItem> CraneList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> CategoryList { get; set; } = new List<SelectListItem>();
  }

  public class CraneUsageListViewModel
  {
    public List<CraneUsageRecordViewModel> UsageRecords { get; set; } = new List<CraneUsageRecordViewModel>();
    public CraneUsageFilterViewModel Filter { get; set; } = new CraneUsageFilterViewModel();
  }
}
