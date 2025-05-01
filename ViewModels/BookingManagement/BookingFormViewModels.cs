using AspnetCoreMvcFull.ViewModels.CraneManagement;
using AspnetCoreMvcFull.ViewModels.ShiftManagement;


namespace AspnetCoreMvcFull.ViewModels.BookingManagement
{
  /// <summary>
  /// ViewModel untuk menampilkan form booking crane
  /// </summary>
  public class BookingFormViewModel
  {
    // Data crane untuk dropdown
    public IEnumerable<CraneViewModel> AvailableCranes { get; set; } = new List<CraneViewModel>();

    // Data shift definitions untuk shift table
    public IEnumerable<ShiftViewModel> ShiftDefinitions { get; set; } = new List<ShiftViewModel>();

    // Properti lain bisa ditambahkan sesuai kebutuhan untuk tahap berikutnya
    // public IEnumerable<HazardViewModel> AvailableHazards { get; set; } = new List<HazardViewModel>();
  }
}
