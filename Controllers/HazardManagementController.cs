// Controllers/HazardManagementController.cs
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.ViewModels.HazardManagement;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class HazardManagementController : Controller
  {
    private readonly IHazardService _hazardService;
    private readonly ILogger<HazardManagementController> _logger;

    public HazardManagementController(IHazardService hazardService, ILogger<HazardManagementController> logger)
    {
      _hazardService = hazardService;
      _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
      try
      {
        var hazards = await _hazardService.GetAllHazardsAsync();

        var viewModel = new HazardListViewModel
        {
          Hazards = hazards,
          SuccessMessage = TempData["HazardSuccessMessage"] as string,
          ErrorMessage = TempData["HazardErrorMessage"] as string
        };

        // Hapus TempData setelah digunakan, ini akan mencegah
        // pesan muncul kembali saat halaman di-refresh
        TempData.Remove("HazardSuccessMessage");
        TempData.Remove("HazardErrorMessage");

        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading hazards");
        TempData["HazardErrorMessage"] = "Error loading hazards: " + ex.Message;
        return View(new HazardListViewModel { ErrorMessage = ex.Message });
      }
    }

    public IActionResult Create()
    {
      return View(new HazardCreateViewModel { Name = string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HazardCreateViewModel viewModel)
    {
      if (ModelState.IsValid)
      {
        try
        {
          await _hazardService.CreateHazardAsync(viewModel);

          TempData["HazardSuccessMessage"] = "Hazard created successfully";
          return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
          ModelState.AddModelError("", ex.Message);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error creating hazard");
          ModelState.AddModelError("", $"Error creating hazard: {ex.Message}");
        }
      }

      return View(viewModel);
    }

    public async Task<IActionResult> Edit(int id)
    {
      try
      {
        var hazard = await _hazardService.GetHazardByIdAsync(id);

        var viewModel = new HazardUpdateViewModel
        {
          Name = hazard.Name
        };

        return View(viewModel);
      }
      catch (KeyNotFoundException)
      {
        return NotFound();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading hazard with ID {id}", id);
        TempData["HazardErrorMessage"] = "Error loading hazard: " + ex.Message;
        return RedirectToAction(nameof(Index));
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, HazardUpdateViewModel viewModel)
    {
      if (ModelState.IsValid)
      {
        try
        {
          await _hazardService.UpdateHazardAsync(id, viewModel);

          TempData["HazardSuccessMessage"] = "Hazard updated successfully";
          return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
          return NotFound();
        }
        catch (InvalidOperationException ex)
        {
          ModelState.AddModelError("", ex.Message);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error updating hazard with ID {id}", id);
          ModelState.AddModelError("", $"Error updating hazard: {ex.Message}");
        }
      }

      return View(viewModel);
    }

    public async Task<IActionResult> Delete(int id)
    {
      try
      {
        var hazard = await _hazardService.GetHazardByIdAsync(id);
        return View(hazard);
      }
      catch (KeyNotFoundException)
      {
        return NotFound();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading hazard with ID {id} for deletion", id);
        TempData["HazardErrorMessage"] = "Error loading hazard: " + ex.Message;
        return RedirectToAction(nameof(Index));
      }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      try
      {
        await _hazardService.DeleteHazardAsync(id);
        TempData["HazardSuccessMessage"] = "Hazard deleted successfully";
        return RedirectToAction(nameof(Index));
      }
      catch (KeyNotFoundException)
      {
        return NotFound();
      }
      catch (InvalidOperationException ex)
      {
        TempData["HazardErrorMessage"] = ex.Message;
        return RedirectToAction(nameof(Delete), new { id });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error deleting hazard with ID {id}", id);
        TempData["HazardErrorMessage"] = "Error deleting hazard: " + ex.Message;
        return RedirectToAction(nameof(Index));
      }
    }

    public async Task<IActionResult> Details(int id)
    {
      try
      {
        var hazard = await _hazardService.GetHazardByIdAsync(id);
        return View(hazard);
      }
      catch (KeyNotFoundException)
      {
        return NotFound();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading hazard details for ID {id}", id);
        TempData["HazardErrorMessage"] = "Error loading hazard details: " + ex.Message;
        return RedirectToAction(nameof(Index));
      }
    }
  }
}
