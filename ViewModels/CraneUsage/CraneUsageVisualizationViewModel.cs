// ViewModels/CraneUsage/CraneUsageVisualizationViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.ViewModels.CraneUsage
{
  public class CraneUsageVisualizationViewModel
  {
    [Required(ErrorMessage = "Crane harus dipilih")]
    public int CraneId { get; set; }

    [Required(ErrorMessage = "Tanggal harus diisi")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    public string CraneName { get; set; } = string.Empty;

    // Data untuk visualisasi
    public List<HourlyUsageData> HourlyData { get; set; } = new List<HourlyUsageData>();

    // Dropdown untuk crane
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> CraneList { get; set; } = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
  }

  public class HourlyUsageData
  {
    public int Hour { get; set; }
    public string Category { get; set; } = "Standby"; // Default to Standby
    public string SubcategoryName { get; set; } = string.Empty;
    public string ColorCode { get; set; } = "#CCCCCC"; // Default color for Standby
    public string BookingNumber { get; set; } = string.Empty;
    public string MaintenanceTitle { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
  }
}
