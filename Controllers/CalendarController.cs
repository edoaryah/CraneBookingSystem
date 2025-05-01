using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Services.Auth;
using AspnetCoreMvcFull.ViewModels.BookingManagement;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class CalendarController : Controller
  {
    private readonly ICraneService _craneService;
    private readonly IShiftDefinitionService _shiftService;
    private readonly IAuthService _authService;

    public CalendarController(ICraneService craneService, IShiftDefinitionService shiftService, IAuthService authService)
    {
      _craneService = craneService;
      _shiftService = shiftService;
      _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
      // Dapatkan username dari Identity
      string username = User.Identity?.Name ?? "";

      // Ambil department langsung dari claims (seperti yang dilakukan di navbar)
      string department = User.FindFirst("department")?.Value ?? "";

      // Set data untuk view
      ViewData["UserName"] = username;
      ViewData["UserDepartment"] = department;

      // Gunakan ViewModel yang sama dengan BookingForm
      var viewModel = new BookingFormViewModel
      {
        AvailableCranes = await _craneService.GetAllCranesAsync(),
        ShiftDefinitions = await _shiftService.GetAllShiftDefinitionsAsync()
      };

      return View(viewModel);
    }
  }
}
