// Controllers/CraneUsageController.cs
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Services.CraneUsage;
using AspnetCoreMvcFull.ViewModels.CraneUsage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspnetCoreMvcFull.Controllers
{
  [Authorize]
  public class CraneUsageController : Controller
  {
    private readonly ICraneUsageService _craneUsageService;
    private readonly AppDbContext _context;
    private readonly ILogger<CraneUsageController> _logger;

    public CraneUsageController(
        ICraneUsageService craneUsageService,
        AppDbContext context,
        ILogger<CraneUsageController> logger)
    {
      _craneUsageService = craneUsageService;
      _context = context;
      _logger = logger;
    }

    // GET: CraneUsage
    public async Task<IActionResult> Index(CraneUsageFilterViewModel filter)
    {
      try
      {
        var viewModel = await _craneUsageService.GetFilteredUsageRecordsAsync(filter);
        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error retrieving crane usage records");
        TempData["ErrorMessage"] = "Terjadi kesalahan saat mengambil data penggunaan crane.";
        return View(new CraneUsageListViewModel());
      }
    }

    // GET: CraneUsage/Form
    public async Task<IActionResult> Form(int craneId = 0, DateTime? date = null)
    {
      try
      {
        // Default date to today if not specified
        var viewDate = date ?? DateTime.Today;

        // If crane not specified, get the first available crane
        if (craneId == 0)
        {
          var firstCrane = await _context.Cranes.OrderBy(c => c.Code).FirstOrDefaultAsync();
          if (firstCrane != null)
          {
            craneId = firstCrane.Id;
          }
        }

        // Get current user name from claims
        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        // Create form view model
        var viewModel = new CraneUsageFormViewModel
        {
          CraneId = craneId,
          Date = viewDate,
          OperatorName = userName,
          CraneList = await GetCraneListAsync()
        };

        // If we have a valid crane and date, load existing entries
        if (craneId > 0)
        {
          var entries = await _craneUsageService.GetCraneUsageEntriesForDateAsync(craneId, viewDate);
          viewModel.Entries = entries;

          // If we have entries, use the operator name from the record
          if (entries.Any())
          {
            var record = await _context.CraneUsageRecords
                .FirstOrDefaultAsync(r => r.CraneId == craneId && r.Date.Date == viewDate.Date);

            if (record != null)
            {
              viewModel.OperatorName = record.OperatorName;
            }
          }
        }

        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading crane usage form");
        TempData["ErrorMessage"] = "Terjadi kesalahan saat memuat form penggunaan crane.";
        return RedirectToAction(nameof(Index));
      }
    }

    // POST: CraneUsage/Form
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Form(CraneUsageFormViewModel viewModel)
    {
      try
      {
        if (ModelState.IsValid)
        {
          // Get current user info from claims
          var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

          // Try to save the form
          var success = await _craneUsageService.SaveCraneUsageFormAsync(viewModel, userName);

          if (success)
          {
            TempData["SuccessMessage"] = "Data penggunaan crane berhasil disimpan.";
            return RedirectToAction(nameof(Form), new { craneId = viewModel.CraneId, date = viewModel.Date.ToString("yyyy-MM-dd") });
          }
          else
          {
            TempData["ErrorMessage"] = "Terdapat konflik waktu pada entri penggunaan. Pastikan waktu tidak tumpang tindih.";
          }
        }

        // If we got this far, something failed, redisplay form
        viewModel.CraneList = await GetCraneListAsync();

        // Reload entries in case validation failed
        var entries = await _craneUsageService.GetCraneUsageEntriesForDateAsync(viewModel.CraneId, viewModel.Date);
        viewModel.Entries = entries;

        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error saving crane usage form");
        TempData["ErrorMessage"] = "Terjadi kesalahan saat menyimpan data penggunaan crane.";

        // Repopulate form
        viewModel.CraneList = await GetCraneListAsync();
        return View(viewModel);
      }
    }

    // AJAX: CraneUsage/AddEntry
    [HttpPost]
    public async Task<IActionResult> AddEntry(CraneUsageEntryViewModel entry, int craneId, DateTime date, string operatorName)
    {
      try
      {
        // Validate time format
        if (entry.StartTime >= entry.EndTime)
        {
          return Json(new { success = false, message = "Waktu mulai harus lebih awal dari waktu selesai." });
        }

        // Check for conflicts with existing entries
        var existingEntries = await _craneUsageService.GetCraneUsageEntriesForDateAsync(craneId, date);
        if (!_craneUsageService.ValidateNoTimeConflicts(existingEntries, entry))
        {
          return Json(new { success = false, message = "Terdapat konflik waktu dengan entri yang sudah ada." });
        }

        // Add new entry
        var result = await _craneUsageService.AddCraneUsageEntryAsync(craneId, date, entry, operatorName);

        if (result)
        {
          // Get the complete entry with ID and navigation properties
          var updatedEntry = await _craneUsageService.GetCraneUsageEntryByTimeAsync(craneId, date, entry.StartTime, entry.EndTime);
          return Json(new { success = true, entry = updatedEntry });
        }
        else
        {
          return Json(new { success = false, message = "Gagal menambahkan entri penggunaan crane." });
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error adding crane usage entry");
        return Json(new { success = false, message = "Terjadi kesalahan saat menambahkan entri penggunaan crane." });
      }
    }

    // AJAX: CraneUsage/UpdateEntry
    [HttpPost]
    public async Task<IActionResult> UpdateEntry(CraneUsageEntryViewModel entry)
    {
      try
      {
        // Validate time format
        if (entry.StartTime >= entry.EndTime)
        {
          return Json(new { success = false, message = "Waktu mulai harus lebih awal dari waktu selesai." });
        }

        // Get existing entry
        var existingEntry = await _context.CraneUsageEntries.FindAsync(entry.Id);
        if (existingEntry == null)
        {
          return Json(new { success = false, message = "Entri tidak ditemukan." });
        }

        // Get record
        var record = await _context.CraneUsageRecords.FindAsync(existingEntry.CraneUsageRecordId);
        if (record == null)
        {
          return Json(new { success = false, message = "Record tidak ditemukan." });
        }

        // Check for conflicts with other entries
        var allEntries = await _craneUsageService.GetCraneUsageEntriesForDateAsync(record.CraneId, record.Date);
        var otherEntries = allEntries.Where(e => e.Id != entry.Id).ToList();

        if (!_craneUsageService.ValidateNoTimeConflicts(otherEntries, entry))
        {
          return Json(new { success = false, message = "Terdapat konflik waktu dengan entri yang sudah ada." });
        }

        // Update entry
        var result = await _craneUsageService.UpdateCraneUsageEntryAsync(entry);

        if (result)
        {
          // Get the updated entry with navigation properties
          var updatedEntry = await _craneUsageService.GetCraneUsageEntryByIdAsync(entry.Id);
          return Json(new { success = true, entry = updatedEntry });
        }
        else
        {
          return Json(new { success = false, message = "Gagal memperbarui entri penggunaan crane." });
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error updating crane usage entry");
        return Json(new { success = false, message = "Terjadi kesalahan saat memperbarui entri penggunaan crane." });
      }
    }

    // AJAX: CraneUsage/DeleteEntry
    [HttpPost]
    public async Task<IActionResult> DeleteEntry(int id)
    {
      try
      {
        var result = await _craneUsageService.DeleteCraneUsageEntryAsync(id);

        return Json(new { success = result });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error deleting crane usage entry");
        return Json(new { success = false, message = "Terjadi kesalahan saat menghapus entri penggunaan crane." });
      }
    }

    // AJAX: CraneUsage/GetSubcategories
    [HttpGet]
    public async Task<IActionResult> GetSubcategories(UsageCategory category)
    {
      try
      {
        var subcategories = await _craneUsageService.GetSubcategoriesByCategoryAsync(category);
        return Json(subcategories);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting subcategories for category {Category}", category);
        return Json(new List<SelectListItem>());
      }
    }

    // AJAX: CraneUsage/GetRelatedBookings
    [HttpGet]
    public async Task<IActionResult> GetRelatedBookings(int craneId, DateTime date, TimeSpan startTime, TimeSpan endTime)
    {
      try
      {
        var startDateTime = date.Date.Add(startTime);
        var endDateTime = date.Date.Add(endTime);

        var bookings = await _craneUsageService.GetAvailableBookingsAsync(craneId, startDateTime, endDateTime);
        return Json(bookings);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting related bookings");
        return Json(new List<SelectListItem>());
      }
    }

    // AJAX: CraneUsage/GetRelatedMaintenance
    [HttpGet]
    public async Task<IActionResult> GetRelatedMaintenance(int craneId, DateTime date, TimeSpan startTime, TimeSpan endTime)
    {
      try
      {
        var startDateTime = date.Date.Add(startTime);
        var endDateTime = date.Date.Add(endTime);

        var maintenance = await _craneUsageService.GetAvailableMaintenanceSchedulesAsync(craneId, startDateTime, endDateTime);
        return Json(maintenance);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting related maintenance schedules");
        return Json(new List<SelectListItem>());
      }
    }

    // GET: CraneUsage/Visualization
    public async Task<IActionResult> Visualization(int craneId = 0, DateTime? date = null)
    {
      try
      {
        if (craneId == 0)
        {
          // Default to first crane if none specified
          var firstCrane = await _context.Cranes.OrderBy(c => c.Code).FirstOrDefaultAsync();
          if (firstCrane != null)
          {
            craneId = firstCrane.Id;
          }
        }

        var viewDate = date ?? DateTime.Today;
        var viewModel = await _craneUsageService.GetUsageVisualizationDataAsync(craneId, viewDate);
        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error retrieving crane usage visualization data");
        TempData["ErrorMessage"] = "Terjadi kesalahan saat memuat visualisasi penggunaan crane.";
        return View(new CraneUsageVisualizationViewModel());
      }
    }

    // AJAX: CraneUsage/SearchBookings
    [HttpGet]
    public async Task<IActionResult> SearchBookings(string term, int craneId)
    {
      try
      {
        // Cari semua booking yang status PICApproved atau Done dengan craneId yang sama
        var bookings = await _context.Bookings
            .Where(b => b.CraneId == craneId &&
                       (b.Status == BookingStatus.PICApproved ||
                        b.Status == BookingStatus.Done) &&
                       (b.BookingNumber.Contains(term) ||
                        b.DocumentNumber.Contains(term) ||
                        b.Name.Contains(term)))
            .Select(b => new SelectListItem
            {
              Value = b.Id.ToString(),
              Text = $"{b.BookingNumber} - {b.Name} ({b.StartDate:dd/MM/yyyy HH:mm} - {b.EndDate:dd/MM/yyyy HH:mm})"
            })
            .Take(10)
            .ToListAsync();

        return Json(bookings);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error searching bookings");
        return Json(new List<SelectListItem>());
      }
    }

    // Helper methods
    private async Task<List<SelectListItem>> GetCraneListAsync()
    {
      var cranes = await _context.Cranes.OrderBy(c => c.Code).ToListAsync();
      return cranes.Select(c => new SelectListItem
      {
        Value = c.Id.ToString(),
        Text = $"{c.Code} - {c.Capacity} Ton"
      }).ToList();
    }
  }
}
