using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class CraneUsageController : Controller
  {
    public IActionResult Index(int id)
    {
      ViewData["BookingId"] = id;
      return View();
    }

    public IActionResult History()
    {
      return View();
    }
  }
}
