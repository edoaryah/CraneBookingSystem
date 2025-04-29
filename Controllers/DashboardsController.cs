using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers;

[Authorize]
// [ServiceFilter(typeof(AuthorizationFilter))]
public class DashboardsController : Controller
{
  // [RequireRole("admin")]
  public IActionResult Index() => View();
}
