using AspnetCoreMvcFull.ViewModels.CraneManagement;

namespace AspnetCoreMvcFull.ViewModels.BookingManagement
{
  /// <summary>
  /// ViewModel untuk menampilkan form booking crane
  /// </summary>
  public class BookingFormViewModel
  {
    // Data crane untuk dropdown
    public IEnumerable<CraneViewModel> AvailableCranes { get; set; } = new List<CraneViewModel>();

    // Properti lain bisa ditambahkan sesuai kebutuhan untuk tahap berikutnya
    // public IEnumerable<HazardViewModel> AvailableHazards { get; set; } = new List<HazardViewModel>();
    // public IEnumerable<ShiftDefinitionViewModel> ShiftDefinitions { get; set; } = new List<ShiftDefinitionViewModel>();
  }
}
