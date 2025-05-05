// Controllers/CraneUsageController.cs

using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Services.CraneUsage;
using AspnetCoreMvcFull.ViewModels.CraneUsage;
using AspnetCoreMvcFull.Services;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class CraneUsageController : Controller
  {
    private readonly ICraneUsageService _usageService;
    private readonly IBookingService _bookingService;
    private readonly ICraneService _craneService;
    private readonly ILogger<CraneUsageController> _logger;

    public CraneUsageController(
        ICraneUsageService usageService,
        IBookingService bookingService,
        ICraneService craneService,
        ILogger<CraneUsageController> logger)
    {
      _usageService = usageService;
      _bookingService = bookingService;
      _craneService = craneService;
      _logger = logger;
    }

    // GET: /CraneUsage/Index?documentNumber=xyz
    public async Task<IActionResult> Index(string documentNumber)
    {
      try
      {
        // Logging untuk debugging
        _logger.LogInformation("Trying to access CraneUsage/Index with documentNumber: {documentNumber}", documentNumber);

        if (string.IsNullOrEmpty(documentNumber))
        {
          _logger.LogWarning("Document number is null or empty");
          return NotFound("Booking document number is required");
        }

        // Dapatkan booking berdasarkan document number
        var booking = await _bookingService.GetBookingByDocumentNumberAsync(documentNumber);

        // Ambil ID booking dari detail yang didapat
        int bookingId = booking.Id;

        // Get usage summary
        var summary = await _usageService.GetUsageSummaryByBookingIdAsync(bookingId);

        // Get available subcategories for all categories for dropdowns
        var subcategories = new Dictionary<UsageCategory, IEnumerable<UsageSubcategoryViewModel>>();
        foreach (UsageCategory category in Enum.GetValues(typeof(UsageCategory)))
        {
          subcategories[category] = await _usageService.GetSubcategoriesByCategoryAsync(category);
        }

        // Prepare view model
        var viewModel = new CraneUsageIndexViewModel
        {
          Booking = booking,
          UsageSummary = summary,
          Subcategories = subcategories
        };

        return View(viewModel);
      }
      catch (KeyNotFoundException ex)
      {
        _logger.LogWarning(ex, "Booking dengan document number {documentNumber} tidak ditemukan", documentNumber);
        return NotFound($"Booking dengan document number {documentNumber} tidak ditemukan");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Terjadi kesalahan saat memuat halaman usage untuk booking dengan document number {documentNumber}", documentNumber);
        TempData["ErrorMessage"] = "Terjadi kesalahan saat memuat data crane usage. Silakan coba lagi.";
        return View(new CraneUsageIndexViewModel());
      }
    }

    // GET: /CraneUsage/History
    public async Task<IActionResult> History()
    {
      try
      {
        // Get all bookings for history page
        var bookings = await _bookingService.GetAllBookingsAsync();

        // Get all cranes for filter
        var cranes = await _craneService.GetAllCranesAsync();

        // Create view model
        var viewModel = new CraneUsageHistoryViewModel
        {
          Bookings = bookings.ToList(),
          Cranes = cranes.ToList()
        };

        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading usage history");
        ModelState.AddModelError("", $"Error loading usage history: {ex.Message}");
        return View(new CraneUsageHistoryViewModel());
      }
    }

    // GET: /CraneUsage/GetUsageSummary/{bookingId}
    public async Task<IActionResult> GetUsageSummary(int bookingId)
    {
      try
      {
        var summary = await _usageService.GetUsageSummaryByBookingIdAsync(bookingId);
        return Json(summary);
      }
      catch (KeyNotFoundException)
      {
        return NotFound(new { message = $"Booking with ID {bookingId} not found" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting usage summary for booking {BookingId}", bookingId);
        return StatusCode(500, new { message = "Error loading usage summary" });
      }
    }

    // GET: /CraneUsage/GetUsageRecords/{bookingId}
    public async Task<IActionResult> GetUsageRecords(int bookingId)
    {
      try
      {
        var records = await _usageService.GetUsageRecordsByBookingIdAsync(bookingId);
        return Json(records);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting usage records for booking {BookingId}", bookingId);
        return StatusCode(500, new { message = "Error loading usage records" });
      }
    }

    // GET: /CraneUsage/GetSubcategories/{category}
    public async Task<IActionResult> GetSubcategories(UsageCategory category)
    {
      try
      {
        var subcategories = await _usageService.GetSubcategoriesByCategoryAsync(category);
        return Json(subcategories);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting subcategories for category {Category}", category);
        return StatusCode(500, new { message = "Error loading subcategories" });
      }
    }

    // POST: /CraneUsage/CreateRecord
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRecord(CraneUsageRecordCreateViewModel viewModel)
    {
      try
      {
        if (ModelState.IsValid)
        {
          string createdBy = GetCurrentUsername();
          var result = await _usageService.CreateUsageRecordAsync(viewModel, createdBy);

          // Dapatkan document number untuk redirect
          var booking = await _bookingService.GetBookingByIdAsync(viewModel.BookingId);
          string documentNumber = booking.DocumentNumber;

          // Jika di-call dengan AJAX, return JSON
          if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
          {
            return Json(new { success = true, record = result, documentNumber = documentNumber });
          }

          // Jika form submit biasa, redirect ke Index dengan document number
          return RedirectToAction(nameof(Index), new { documentNumber });
        }

        // Jika invalid dan AJAX
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
          return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // Jika invalid dan form submit biasa, perlu load ulang data
        // Dapatkan document number untuk reload halaman
        var bookingDetail = await _bookingService.GetBookingByIdAsync(viewModel.BookingId);
        var summary = await _usageService.GetUsageSummaryByBookingIdAsync(viewModel.BookingId);

        var subcategories = new Dictionary<UsageCategory, IEnumerable<UsageSubcategoryViewModel>>();
        foreach (UsageCategory category in Enum.GetValues(typeof(UsageCategory)))
        {
          subcategories[category] = await _usageService.GetSubcategoriesByCategoryAsync(category);
        }

        return View("Index", new CraneUsageIndexViewModel
        {
          Booking = bookingDetail,
          UsageSummary = summary,
          Subcategories = subcategories
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error creating usage record");

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
          return StatusCode(500, new { success = false, message = ex.Message });
        }

        ModelState.AddModelError("", $"Error creating record: {ex.Message}");

        // Load data again for the view
        var booking = await _bookingService.GetBookingByIdAsync(viewModel.BookingId);
        var summary = await _usageService.GetUsageSummaryByBookingIdAsync(viewModel.BookingId);

        var subcategories = new Dictionary<UsageCategory, IEnumerable<UsageSubcategoryViewModel>>();
        foreach (UsageCategory category in Enum.GetValues(typeof(UsageCategory)))
        {
          subcategories[category] = await _usageService.GetSubcategoriesByCategoryAsync(category);
        }

        return View("Index", new CraneUsageIndexViewModel
        {
          Booking = booking,
          UsageSummary = summary,
          Subcategories = subcategories
        });
      }
    }

    // POST: /CraneUsage/UpdateRecord/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRecord(int id, CraneUsageRecordUpdateViewModel viewModel)
    {
      try
      {
        if (ModelState.IsValid)
        {
          string updatedBy = GetCurrentUsername();
          var result = await _usageService.UpdateUsageRecordAsync(id, viewModel, updatedBy);

          // Get booking document number for redirect
          var booking = await _bookingService.GetBookingByIdAsync(result.BookingId);
          string documentNumber = booking.DocumentNumber;

          if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
          {
            return Json(new { success = true, record = result, documentNumber = documentNumber });
          }

          return RedirectToAction(nameof(Index), new { documentNumber });
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
          return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // Untuk non-AJAX, kita perlu tahu document number untuk redirect atau re-render view
        var record = await _usageService.GetUsageRecordByIdAsync(id);
        var bookingForRedirect = await _bookingService.GetBookingByIdAsync(record.BookingId);
        return RedirectToAction(nameof(Index), new { documentNumber = bookingForRedirect.DocumentNumber });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error updating usage record");

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
          return StatusCode(500, new { success = false, message = ex.Message });
        }

        ModelState.AddModelError("", $"Error updating record: {ex.Message}");

        // Get booking document number for redirect
        var record = await _usageService.GetUsageRecordByIdAsync(id);
        var booking = await _bookingService.GetBookingByIdAsync(record.BookingId);
        return RedirectToAction(nameof(Index), new { documentNumber = booking.DocumentNumber });
      }
    }

    // POST: /CraneUsage/DeleteRecord/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRecord(int id)
    {
      try
      {
        // Get booking document number before deleting for redirect
        var record = await _usageService.GetUsageRecordByIdAsync(id);
        var booking = await _bookingService.GetBookingByIdAsync(record.BookingId);
        string documentNumber = booking.DocumentNumber;

        await _usageService.DeleteUsageRecordAsync(id);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
          return Json(new { success = true, documentNumber = documentNumber });
        }

        return RedirectToAction(nameof(Index), new { documentNumber });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error deleting usage record");

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
          return StatusCode(500, new { success = false, message = ex.Message });
        }

        return NotFound();
      }
    }

    // Helpers
    private string GetCurrentUsername()
    {
      return User.FindFirst("ldapuser")?.Value ?? "system";
    }
  }

  // ViewModel classes

  // ViewModel for Index page
  public class CraneUsageIndexViewModel
  {
    public ViewModels.BookingManagement.BookingDetailViewModel? Booking { get; set; }
    public UsageSummaryViewModel? UsageSummary { get; set; }
    public Dictionary<UsageCategory, IEnumerable<UsageSubcategoryViewModel>> Subcategories { get; set; } =
        new Dictionary<UsageCategory, IEnumerable<UsageSubcategoryViewModel>>();
  }

  // ViewModel for History page
  public class CraneUsageHistoryViewModel
  {
    public List<ViewModels.BookingManagement.BookingViewModel> Bookings { get; set; } =
        new List<ViewModels.BookingManagement.BookingViewModel>();
    public List<ViewModels.CraneManagement.CraneViewModel> Cranes { get; set; } =
        new List<ViewModels.CraneManagement.CraneViewModel>();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
  }
}
