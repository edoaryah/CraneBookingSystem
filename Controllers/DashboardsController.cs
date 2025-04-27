using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Controllers;

[Authorize]
public class DashboardsController : Controller
{
  public IActionResult Index() => View();
}
