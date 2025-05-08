// ViewModels/CraneUsage/CraneUsageFormViewModel.cs
using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.ViewModels.CraneUsage
{
  public class CraneUsageFormViewModel
  {
    [Required(ErrorMessage = "Crane harus dipilih")]
    [Display(Name = "Crane")]
    public int CraneId { get; set; }

    [Required(ErrorMessage = "Tanggal harus diisi")]
    [Display(Name = "Tanggal")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    [Display(Name = "Operator")]
    [StringLength(100)]
    public string? OperatorName { get; set; }

    // List of time entries
    public List<CraneUsageEntryViewModel> Entries { get; set; } = new List<CraneUsageEntryViewModel>();

    // Dropdowns
    public List<SelectListItem> CraneList { get; set; } = new List<SelectListItem>();
  }
}
