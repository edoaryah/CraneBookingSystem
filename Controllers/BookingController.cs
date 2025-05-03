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
    private readonly IShiftDefinitionService _shiftService;
    private readonly IHazardService _hazardService;

    public BookingController(ICraneService craneService, IShiftDefinitionService shiftService, IHazardService hazardService)
    {
      _craneService = craneService;
      _shiftService = shiftService;
      _hazardService = hazardService;
    }

    public async Task<IActionResult> Index()
    {
      var viewModel = new BookingFormViewModel
      {
        AvailableCranes = await _craneService.GetAllCranesAsync(),
        ShiftDefinitions = await _shiftService.GetAllShiftDefinitionsAsync(),
        AvailableHazards = await _hazardService.GetAllHazardsAsync(),
      };
      return View(viewModel);
    }
  }
}
