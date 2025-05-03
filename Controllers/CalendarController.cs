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

    public CalendarController(ICraneService craneService, IShiftDefinitionService shiftService)
    {
      _craneService = craneService;
      _shiftService = shiftService;
    }

    public async Task<IActionResult> Index()
    {
      var viewModel = new BookingFormViewModel
      {
        AvailableCranes = await _craneService.GetAllCranesAsync(),
        ShiftDefinitions = await _shiftService.GetAllShiftDefinitionsAsync()
      };

      return View(viewModel);
    }
  }
}
