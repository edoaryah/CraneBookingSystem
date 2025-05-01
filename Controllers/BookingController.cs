using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Services.Auth;
using AspnetCoreMvcFull.ViewModels.BookingManagement;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class BookingController : Controller
  {
    private readonly ICraneService _craneService;
    private readonly IAuthService _authService;

    public BookingController(ICraneService craneService, IAuthService authService)
    {
      _craneService = craneService;
      _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
      // Dapatkan username dari Identity
      string username = User.Identity?.Name ?? "";

      // Ambil department langsung dari claims (seperti yang dilakukan di navbar)
      string department = User.FindFirst("department")?.Value ?? "";

      // Set data untuk form
      ViewData["UserName"] = username;
      ViewData["UserDepartment"] = department;

      var viewModel = new BookingFormViewModel
      {
        AvailableCranes = await _craneService.GetAllCranesAsync()
      };

      return View(viewModel);
    }
  }
}
