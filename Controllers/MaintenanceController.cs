using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.ViewModels.MaintenanceManagement;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class MaintenanceController : Controller
  {
    private readonly ICraneService _craneService;
    private readonly IShiftDefinitionService _shiftService;

    public MaintenanceController(ICraneService craneService, IShiftDefinitionService shiftService)
    {
      _craneService = craneService;
      _shiftService = shiftService;
    }

    public async Task<IActionResult> Index()
    {
      var viewModel = new MaintenanceFormViewModel
      {
        AvailableCranes = await _craneService.GetAllCranesAsync(),
        ShiftDefinitions = await _shiftService.GetAllShiftDefinitionsAsync()
      };

      return View(viewModel);
    }
  }
}
