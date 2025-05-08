// // Controllers/CraneUsageController.cs

// using Microsoft.AspNetCore.Mvc;
// using AspnetCoreMvcFull.Filters;
// using AspnetCoreMvcFull.Models;
// using AspnetCoreMvcFull.Services.CraneUsage;
// using AspnetCoreMvcFull.ViewModels.CraneUsage;
// using AspnetCoreMvcFull.Services;

// namespace AspnetCoreMvcFull.Controllers
// {
//   [ServiceFilter(typeof(AuthorizationFilter))]
//   public class CraneUsageController : Controller
//   {
//     private readonly ICraneUsageService _usageService;
//     private readonly IBookingService _bookingService;
//     private readonly ICraneService _craneService;
//     private readonly ILogger<CraneUsageController> _logger;

//     public CraneUsageController(
//         ICraneUsageService usageService,
//         IBookingService bookingService,
//         ICraneService craneService,
//         ILogger<CraneUsageController> logger)
//     {
//       _usageService = usageService;
//       _bookingService = bookingService;
//       _craneService = craneService;
//       _logger = logger;
//     }

//     // GET: /CraneUsage/Index?documentNumber=xyz
//     public async Task<IActionResult> Index(string documentNumber)
//     {
//       try
//       {
//         // Logging untuk debugging
//         _logger.LogInformation("Trying to access CraneUsage/Index with documentNumber: {documentNumber}", documentNumber);

//         if (string.IsNullOrEmpty(documentNumber))
//         {
//           _logger.LogWarning("Document number is null or empty");
//           return NotFound("Booking document number is required");
//         }

//         // Dapatkan booking berdasarkan document number
//         var booking = await _bookingService.GetBookingByDocumentNumberAsync(documentNumber);

//         // Ambil ID booking dari detail yang didapat
//         int bookingId = booking.Id;

//         // Get usage summary
//         var summary = await _usageService.GetUsageSummaryByBookingIdAsync(bookingId);

//         // Get available subcategories for all categories for dropdowns
//         var subcategories = new Dictionary<UsageCategory, IEnumerable<UsageSubcategoryViewModel>>();
//         foreach (UsageCategory category in Enum.GetValues(typeof(UsageCategory)))
//         {
//           subcategories[category] = await _usageService.GetSubcategoriesByCategoryAsync(category);
//         }

//         // Prepare view model
//         var viewModel = new CraneUsageIndexViewModel
//         {
//           Booking = booking,
//           UsageSummary = summary,
//           Subcategories = subcategories
//         };

//         return View(viewModel);
//       }
//       catch (KeyNotFoundException ex)
//       {
//         _logger.LogWarning(ex, "Booking dengan document number {documentNumber} tidak ditemukan", documentNumber);
//         return NotFound($"Booking dengan document number {documentNumber} tidak ditemukan");
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "Terjadi kesalahan saat memuat halaman usage untuk booking dengan document number {documentNumber}", documentNumber);
//         TempData["ErrorMessage"] = "Terjadi kesalahan saat memuat data crane usage. Silakan coba lagi.";
//         return View(new CraneUsageIndexViewModel());
//       }
//     }

//     // GET: /CraneUsage/History
//     public async Task<IActionResult> History()
//     {
//       try
//       {
//         // Get all bookings for history page
//         var bookings = await _bookingService.GetAllBookingsAsync();

//         // Get all cranes for filter
//         var cranes = await _craneService.GetAllCranesAsync();

//         // Create view model
//         var viewModel = new CraneUsageHistoryViewModel
//         {
//           Bookings = bookings.ToList(),
//           Cranes = cranes.ToList()
//         };

//         return View(viewModel);
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "Error loading usage history");
//         ModelState.AddModelError("", $"Error loading usage history: {ex.Message}");
//         return View(new CraneUsageHistoryViewModel());
//       }
//     }

//     // GET: /CraneUsage/GetUsageSummary/{bookingId}
//     public async Task<IActionResult> GetUsageSummary(int bookingId)
//     {
//       try
//       {
//         var summary = await _usageService.GetUsageSummaryByBookingIdAsync(bookingId);
//         return Json(summary);
//       }
//       catch (KeyNotFoundException)
//       {
//         return NotFound(new { message = $"Booking with ID {bookingId} not found" });
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "Error getting usage summary for booking {BookingId}", bookingId);
//         return StatusCode(500, new { message = "Error loading usage summary" });
//       }
//     }

//     // GET: /CraneUsage/GetUsageRecords/{bookingId}
//     public async Task<IActionResult> GetUsageRecords(int bookingId)
//     {
//       try
//       {
//         var records = await _usageService.GetUsageRecordsByBookingIdAsync(bookingId);
//         return Json(records);
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "Error getting usage records for booking {BookingId}", bookingId);
//         return StatusCode(500, new { message = "Error loading usage records" });
//       }
//     }

//     // GET: /CraneUsage/GetSubcategories/{category}
//     public async Task<IActionResult> GetSubcategories(UsageCategory category)
//     {
//       try
//       {
//         var subcategories = await _usageService.GetSubcategoriesByCategoryAsync(category);
//         return Json(subcategories);
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "Error getting subcategories for category {Category}", category);
//         return StatusCode(500, new { message = "Error loading subcategories" });
//       }
//     }

//     // POST: /CraneUsage/CreateRecord
//     [HttpPost]
//     [ValidateAntiForgeryToken]
//     public async Task<IActionResult> CreateRecord(CraneUsageRecordCreateViewModel viewModel)
//     {
//       try
//       {
//         if (ModelState.IsValid)
//         {
//           string createdBy = GetCurrentUsername();
//           var result = await _usageService.CreateUsageRecordAsync(viewModel, createdBy);

//           // Dapatkan document number untuk redirect
//           var booking = await _bookingService.GetBookingByIdAsync(viewModel.BookingId);
//           string documentNumber = booking.DocumentNumber;

//           // Jika di-call dengan AJAX, return JSON
//           if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//           {
//             return Json(new { success = true, record = result, documentNumber = documentNumber });
//           }

//           // Jika form submit biasa, redirect ke Index dengan document number
//           return RedirectToAction(nameof(Index), new { documentNumber });
//         }

//         // Jika invalid dan AJAX
//         if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//         {
//           return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
//         }

//         // Jika invalid dan form submit biasa, perlu load ulang data
//         // Dapatkan document number untuk reload halaman
//         var bookingDetail = await _bookingService.GetBookingByIdAsync(viewModel.BookingId);
//         var summary = await _usageService.GetUsageSummaryByBookingIdAsync(viewModel.BookingId);

//         var subcategories = new Dictionary<UsageCategory, IEnumerable<UsageSubcategoryViewModel>>();
//         foreach (UsageCategory category in Enum.GetValues(typeof(UsageCategory)))
//         {
//           subcategories[category] = await _usageService.GetSubcategoriesByCategoryAsync(category);
//         }

//         return View("Index", new CraneUsageIndexViewModel
//         {
//           Booking = bookingDetail,
//           UsageSummary = summary,
//           Subcategories = subcategories
//         });
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "Error creating usage record");

//         if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//         {
//           return StatusCode(500, new { success = false, message = ex.Message });
//         }

//         ModelState.AddModelError("", $"Error creating record: {ex.Message}");

//         // Load data again for the view
//         var booking = await _bookingService.GetBookingByIdAsync(viewModel.BookingId);
//         var summary = await _usageService.GetUsageSummaryByBookingIdAsync(viewModel.BookingId);

//         var subcategories = new Dictionary<UsageCategory, IEnumerable<UsageSubcategoryViewModel>>();
//         foreach (UsageCategory category in Enum.GetValues(typeof(UsageCategory)))
//         {
//           subcategories[category] = await _usageService.GetSubcategoriesByCategoryAsync(category);
//         }

//         return View("Index", new CraneUsageIndexViewModel
//         {
//           Booking = booking,
//           UsageSummary = summary,
//           Subcategories = subcategories
//         });
//       }
//     }

//     // POST: /CraneUsage/UpdateRecord/{id}
//     [HttpPost]
//     [ValidateAntiForgeryToken]
//     public async Task<IActionResult> UpdateRecord(int id, CraneUsageRecordUpdateViewModel viewModel)
//     {
//       try
//       {
//         if (ModelState.IsValid)
//         {
//           string updatedBy = GetCurrentUsername();
//           var result = await _usageService.UpdateUsageRecordAsync(id, viewModel, updatedBy);

//           // Get booking document number for redirect
//           var booking = await _bookingService.GetBookingByIdAsync(result.BookingId);
//           string documentNumber = booking.DocumentNumber;

//           if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//           {
//             return Json(new { success = true, record = result, documentNumber = documentNumber });
//           }

//           return RedirectToAction(nameof(Index), new { documentNumber });
//         }

//         if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//         {
//           return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
//         }

//         // Untuk non-AJAX, kita perlu tahu document number untuk redirect atau re-render view
//         var record = await _usageService.GetUsageRecordByIdAsync(id);
//         var bookingForRedirect = await _bookingService.GetBookingByIdAsync(record.BookingId);
//         return RedirectToAction(nameof(Index), new { documentNumber = bookingForRedirect.DocumentNumber });
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "Error updating usage record");

//         if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//         {
//           return StatusCode(500, new { success = false, message = ex.Message });
//         }

//         ModelState.AddModelError("", $"Error updating record: {ex.Message}");

//         // Get booking document number for redirect
//         var record = await _usageService.GetUsageRecordByIdAsync(id);
//         var booking = await _bookingService.GetBookingByIdAsync(record.BookingId);
//         return RedirectToAction(nameof(Index), new { documentNumber = booking.DocumentNumber });
//       }
//     }

//     // POST: /CraneUsage/DeleteRecord/{id}
//     [HttpPost]
//     [ValidateAntiForgeryToken]
//     public async Task<IActionResult> DeleteRecord(int id)
//     {
//       try
//       {
//         // Get booking document number before deleting for redirect
//         var record = await _usageService.GetUsageRecordByIdAsync(id);
//         var booking = await _bookingService.GetBookingByIdAsync(record.BookingId);
//         string documentNumber = booking.DocumentNumber;

//         await _usageService.DeleteUsageRecordAsync(id);

//         if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//         {
//           return Json(new { success = true, documentNumber = documentNumber });
//         }

//         return RedirectToAction(nameof(Index), new { documentNumber });
//       }
//       catch (Exception ex)
//       {
//         _logger.LogError(ex, "Error deleting usage record");

//         if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//         {
//           return StatusCode(500, new { success = false, message = ex.Message });
//         }

//         return NotFound();
//       }
//     }

//     // Helpers
//     private string GetCurrentUsername()
//     {
//       return User.FindFirst("ldapuser")?.Value ?? "system";
//     }
//   }

//   // ViewModel classes

//   // ViewModel for Index page
//   public class CraneUsageIndexViewModel
//   {
//     public ViewModels.BookingManagement.BookingDetailViewModel? Booking { get; set; }
//     public UsageSummaryViewModel? UsageSummary { get; set; }
//     public Dictionary<UsageCategory, IEnumerable<UsageSubcategoryViewModel>> Subcategories { get; set; } =
//         new Dictionary<UsageCategory, IEnumerable<UsageSubcategoryViewModel>>();
//   }

//   // ViewModel for History page
//   public class CraneUsageHistoryViewModel
//   {
//     public List<ViewModels.BookingManagement.BookingViewModel> Bookings { get; set; } =
//         new List<ViewModels.BookingManagement.BookingViewModel>();
//     public List<ViewModels.CraneManagement.CraneViewModel> Cranes { get; set; } =
//         new List<ViewModels.CraneManagement.CraneViewModel>();
//     public string? ErrorMessage { get; set; }
//     public string? SuccessMessage { get; set; }
//   }
// }
// -------------
// using AspnetCoreMvcFull.Data;
// using AspnetCoreMvcFull.Models;
// using AspnetCoreMvcFull.Services;
// using AspnetCoreMvcFull.ViewModels;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Rendering;
// using Microsoft.EntityFrameworkCore;
// using System;
// using System.Linq;
// using System.Threading.Tasks;

// namespace AspnetCoreMvcFull.Controllers
// {
//   // [Authorize(Roles = "Admin,Operator,PIC")] // Sesuaikan dengan kebutuhan role
//   public class CraneUsageController : Controller
//   {
//     private readonly AppDbContext _context;
//     private readonly ICraneUsageService _usageService;

//     public CraneUsageController(AppDbContext context, ICraneUsageService usageService)
//     {
//       _context = context;
//       _usageService = usageService;
//     }

//     // GET: CraneUsage
//     public async Task<IActionResult> Index(int? craneId, DateTime? date)
//     {
//       var today = date ?? DateTime.Today;

//       // Persiapkan ViewModel
//       var viewModel = new CraneUsageIndexViewModel
//       {
//         Date = today,
//         CraneList = await _context.Cranes
//               // Hanya tampilkan crane dengan status Available
//               .Where(c => c.Status == CraneStatus.Available)
//               .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Code })
//               .ToListAsync()
//       };

//       // Jika tidak ada crane yang dipilih, gunakan crane pertama yang tersedia
//       if (!craneId.HasValue && viewModel.CraneList.Any())
//       {
//         craneId = int.Parse(viewModel.CraneList.First().Value);
//       }

//       if (craneId.HasValue)
//       {
//         viewModel.CraneId = craneId.Value;

//         // Dapatkan nama user yang login
//         var userName = User.Identity.Name;

//         // Ambil data untuk ViewModel
//         viewModel.DailySummary = await _usageService.GetDailySummaryAsync(craneId.Value, today, userName);
//         viewModel.UsageList = await _usageService.GetDailyUsagesAsync(craneId.Value, today);
//         viewModel.ActiveBookings = await _usageService.GetActiveBookingsAsync(craneId.Value, today);
//         viewModel.ActiveMaintenanceSchedules = await _usageService.GetActiveMaintenanceSchedulesAsync(craneId.Value, today);
//       }

//       return View(viewModel);
//     }

//     // GET: CraneUsage/Details/5
//     public async Task<IActionResult> Details(int? id)
//     {
//       if (id == null)
//       {
//         return NotFound();
//       }

//       var usage = await _context.CraneUsageRecords
//           .Include(u => u.Crane)
//           .Include(u => u.Booking)
//           .Include(u => u.MaintenanceSchedule)
//           .Include(u => u.UsageSubcategory)
//           .FirstOrDefaultAsync(u => u.Id == id);

//       if (usage == null)
//       {
//         return NotFound();
//       }

//       var viewModel = new CraneUsageDetailsViewModel
//       {
//         Usage = usage
//       };

//       return View(viewModel);
//     }

//     // GET: CraneUsage/Create
//     public async Task<IActionResult> Create(int craneId, DateTime? date, int? bookingId, int? maintenanceId, UsageCategory? category)
//     {
//       var viewDate = date ?? DateTime.Today;

//       var viewModel = new CraneUsageFormViewModel
//       {
//         CraneId = craneId,
//         BookingId = bookingId,
//         MaintenanceScheduleId = maintenanceId,
//         Category = category ?? UsageCategory.Operating,
//         StartTime = viewDate.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute),
//         EndTime = viewDate.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddHours(1)
//       };

//       await PopulateFormDropdowns(viewModel, craneId, viewDate);

//       return View(viewModel);
//     }

//     // POST: CraneUsage/Create
//     [HttpPost]
//     [ValidateAntiForgeryToken]
//     public async Task<IActionResult> Create(CraneUsageFormViewModel viewModel)
//     {
//       if (ModelState.IsValid)
//       {
//         // Cek apakah hari tersebut sudah finalisasi
//         var summary = await _usageService.GetDailySummaryAsync(viewModel.CraneId, viewModel.StartTime.Date, User.Identity.Name);
//         if (summary.IsFinal)
//         {
//           ModelState.AddModelError(string.Empty, "Penggunaan crane untuk tanggal ini sudah difinalisasi dan tidak dapat diubah.");
//           await PopulateFormDropdowns(viewModel, viewModel.CraneId, viewModel.StartTime.Date);
//           return View(viewModel);
//         }

//         // Dapatkan nama user yang login
//         var userName = User.Identity.Name;

//         // Buat entitas CraneUsageRecord dari ViewModel
//         var usage = new CraneUsageRecord
//         {
//           CraneId = viewModel.CraneId,
//           BookingId = viewModel.BookingId,
//           MaintenanceScheduleId = viewModel.MaintenanceScheduleId,
//           StartTime = viewModel.StartTime,
//           EndTime = viewModel.EndTime,
//           Category = viewModel.Category,
//           UsageSubcategoryId = viewModel.UsageSubcategoryId,
//           Notes = viewModel.Notes,
//           CreatedBy = userName // Menambahkan required field
//         };

//         // Tambahkan penggunaan
//         var result = await _usageService.AddUsageAsync(usage, userName);

//         if (result)
//         {
//           return RedirectToAction(nameof(Index), new { craneId = viewModel.CraneId, date = viewModel.StartTime.Date.ToString("yyyy-MM-dd") });
//         }
//         else
//         {
//           ModelState.AddModelError(string.Empty, "Terjadi tumpang tindih waktu dengan penggunaan crane yang sudah ada.");
//         }
//       }

//       // Jika ada kesalahan, kembalikan ke form dengan dropdown yang sudah diisi
//       await PopulateFormDropdowns(viewModel, viewModel.CraneId, viewModel.StartTime.Date);
//       return View(viewModel);
//     }

//     // GET: CraneUsage/Edit/5
//     public async Task<IActionResult> Edit(int? id)
//     {
//       if (id == null)
//       {
//         return NotFound();
//       }

//       var usage = await _context.CraneUsageRecords
//           .Include(u => u.Crane)
//           .FirstOrDefaultAsync(u => u.Id == id);

//       if (usage == null)
//       {
//         return NotFound();
//       }

//       // Cek apakah sudah finalisasi
//       var summary = await _usageService.GetDailySummaryAsync(usage.CraneId, usage.StartTime.Date, User.Identity.Name);
//       if (summary.IsFinal)
//       {
//         TempData["ErrorMessage"] = "Penggunaan crane untuk tanggal ini sudah difinalisasi dan tidak dapat diubah.";
//         return RedirectToAction(nameof(Index), new { craneId = usage.CraneId, date = usage.StartTime.Date.ToString("yyyy-MM-dd") });
//       }

//       var viewModel = new CraneUsageFormViewModel
//       {
//         Id = usage.Id,
//         CraneId = usage.CraneId,
//         BookingId = usage.BookingId,
//         MaintenanceScheduleId = usage.MaintenanceScheduleId,
//         StartTime = usage.StartTime,
//         EndTime = usage.EndTime,
//         Category = usage.Category,
//         UsageSubcategoryId = usage.UsageSubcategoryId,
//         Notes = usage.Notes
//       };

//       await PopulateFormDropdowns(viewModel, usage.CraneId, usage.StartTime.Date);

//       return View(viewModel);
//     }

//     // POST: CraneUsage/Edit/5
//     [HttpPost]
//     [ValidateAntiForgeryToken]
//     public async Task<IActionResult> Edit(int id, CraneUsageFormViewModel viewModel)
//     {
//       if (id != viewModel.Id)
//       {
//         return NotFound();
//       }

//       if (ModelState.IsValid)
//       {
//         // Cek apakah hari tersebut sudah finalisasi
//         var summary = await _usageService.GetDailySummaryAsync(viewModel.CraneId, viewModel.StartTime.Date, User.Identity.Name);
//         if (summary.IsFinal)
//         {
//           ModelState.AddModelError(string.Empty, "Penggunaan crane untuk tanggal ini sudah difinalisasi dan tidak dapat diubah.");
//           await PopulateFormDropdowns(viewModel, viewModel.CraneId, viewModel.StartTime.Date);
//           return View(viewModel);
//         }

//         // Dapatkan entitas yang sudah ada
//         var usage = await _context.CraneUsageRecords.FindAsync(id);
//         if (usage == null)
//         {
//           return NotFound();
//         }

//         // Update properti
//         usage.CraneId = viewModel.CraneId;
//         usage.BookingId = viewModel.BookingId;
//         usage.MaintenanceScheduleId = viewModel.MaintenanceScheduleId;
//         usage.StartTime = viewModel.StartTime;
//         usage.EndTime = viewModel.EndTime;
//         usage.Category = viewModel.Category;
//         usage.UsageSubcategoryId = viewModel.UsageSubcategoryId;
//         usage.Notes = viewModel.Notes;

//         // Dapatkan nama user yang login
//         var userName = User.Identity.Name;

//         // Update entitas
//         var result = await _usageService.UpdateUsageAsync(usage, userName);

//         if (result)
//         {
//           return RedirectToAction(nameof(Index), new { craneId = viewModel.CraneId, date = viewModel.StartTime.Date.ToString("yyyy-MM-dd") });
//         }
//         else
//         {
//           ModelState.AddModelError(string.Empty, "Terjadi tumpang tindih waktu dengan penggunaan crane yang sudah ada.");
//         }
//       }

//       // Jika ada kesalahan, kembalikan ke form dengan dropdown yang sudah diisi
//       await PopulateFormDropdowns(viewModel, viewModel.CraneId, viewModel.StartTime.Date);
//       return View(viewModel);
//     }

//     // GET: CraneUsage/Delete/5
//     public async Task<IActionResult> Delete(int? id)
//     {
//       if (id == null)
//       {
//         return NotFound();
//       }

//       var usage = await _context.CraneUsageRecords
//           .Include(u => u.Crane)
//           .Include(u => u.Booking)
//           .Include(u => u.MaintenanceSchedule)
//           .Include(u => u.UsageSubcategory)
//           .FirstOrDefaultAsync(u => u.Id == id);

//       if (usage == null)
//       {
//         return NotFound();
//       }

//       // Cek apakah sudah finalisasi
//       var summary = await _usageService.GetDailySummaryAsync(usage.CraneId, usage.StartTime.Date, User.Identity.Name);
//       if (summary.IsFinal)
//       {
//         TempData["ErrorMessage"] = "Penggunaan crane untuk tanggal ini sudah difinalisasi dan tidak dapat dihapus.";
//         return RedirectToAction(nameof(Index), new { craneId = usage.CraneId, date = usage.StartTime.Date.ToString("yyyy-MM-dd") });
//       }

//       var viewModel = new CraneUsageDetailsViewModel
//       {
//         Usage = usage
//       };

//       return View(viewModel);
//     }

//     // POST: CraneUsage/Delete/5
//     [HttpPost, ActionName("Delete")]
//     [ValidateAntiForgeryToken]
//     public async Task<IActionResult> DeleteConfirmed(int id)
//     {
//       var usage = await _context.CraneUsageRecords.FindAsync(id);
//       if (usage == null)
//       {
//         return NotFound();
//       }

//       // Cek apakah sudah finalisasi
//       var summary = await _usageService.GetDailySummaryAsync(usage.CraneId, usage.StartTime.Date, User.Identity.Name);
//       if (summary.IsFinal)
//       {
//         TempData["ErrorMessage"] = "Penggunaan crane untuk tanggal ini sudah difinalisasi dan tidak dapat dihapus.";
//         return RedirectToAction(nameof(Index), new { craneId = usage.CraneId, date = usage.StartTime.Date.ToString("yyyy-MM-dd") });
//       }

//       var craneId = usage.CraneId;
//       var date = usage.StartTime.Date;

//       var result = await _usageService.DeleteUsageAsync(id);

//       return RedirectToAction(nameof(Index), new { craneId, date = date.ToString("yyyy-MM-dd") });
//     }

//     // GET: CraneUsage/Finalize
//     public async Task<IActionResult> Finalize(int craneId, DateTime date)
//     {
//       var crane = await _context.Cranes.FindAsync(craneId);
//       if (crane == null)
//       {
//         return NotFound();
//       }

//       var summary = await _usageService.GetDailySummaryAsync(craneId, date, User.Identity.Name);

//       var viewModel = new FinalizeDailyUsageViewModel
//       {
//         CraneId = craneId,
//         CraneName = crane.Code, // Menggunakan Code sebagai pengganti Name
//         Date = date,
//         Summary = summary
//       };

//       return View(viewModel);
//     }

//     // POST: CraneUsage/Finalize
//     [HttpPost]
//     [ValidateAntiForgeryToken]
//     public async Task<IActionResult> Finalize(FinalizeDailyUsageViewModel viewModel)
//     {
//       if (ModelState.IsValid)
//       {
//         if (viewModel.Confirmation == "FINALIZE")
//         {
//           // Dapatkan nama user yang login
//           var userName = User.Identity.Name;

//           var result = await _usageService.FinalizeDailyUsageAsync(viewModel.CraneId, viewModel.Date, userName);

//           if (result)
//           {
//             TempData["SuccessMessage"] = "Penggunaan crane untuk tanggal ini berhasil difinalisasi.";
//           }
//           else
//           {
//             TempData["ErrorMessage"] = "Gagal memfinalisasi penggunaan crane.";
//           }
//         }
//         else
//         {
//           ModelState.AddModelError("Confirmation", "Konfirmasi dengan mengetik 'FINALIZE'");
//           return View(viewModel);
//         }
//       }

//       return RedirectToAction(nameof(Index), new { craneId = viewModel.CraneId, date = viewModel.Date.ToString("yyyy-MM-dd") });
//     }

//     // AJAX: Mendapatkan subkategori berdasarkan kategori yang dipilih
//     [HttpGet]
//     public async Task<IActionResult> GetSubcategories(UsageCategory category)
//     {
//       var subcategories = await _context.UsageSubcategories
//           .Where(s => s.Category == category && s.IsActive)
//           .Select(s => new { id = s.Id, name = s.Name })
//           .ToListAsync();

//       return Json(subcategories);
//     }

//     // Helper untuk mengisi dropdown pada form
//     private async Task PopulateFormDropdowns(CraneUsageFormViewModel viewModel, int craneId, DateTime date)
//     {
//       // Daftar crane
//       viewModel.CraneList = await _context.Cranes
//           // Hanya tampilkan crane dengan status Available
//           .Where(c => c.Status == CraneStatus.Available)
//           .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Code, Selected = c.Id == craneId })
//           .ToListAsync();

//       // Daftar booking aktif
//       viewModel.BookingList = await _context.Bookings
//           .Where(b => b.CraneId == craneId &&
//                  b.StartDate.Date <= date.Date &&
//                  b.EndDate.Date >= date.Date &&
//                  (b.Status == BookingStatus.PICApproved || b.Status == BookingStatus.Done))
//           .Select(b => new SelectListItem
//           {
//             Value = b.Id.ToString(),
//             Text = $"{b.BookingNumber} - {b.Name} ({b.Department})",
//             Selected = b.Id == viewModel.BookingId
//           })
//           .ToListAsync();

//       // Tambahkan opsi kosong untuk booking
//       viewModel.BookingList.Insert(0, new SelectListItem { Value = "", Text = "-- Tidak terkait booking --" });

//       // Daftar maintenance aktif
//       viewModel.MaintenanceList = await _context.MaintenanceSchedules
//           .Where(m => m.CraneId == craneId &&
//                  m.StartDate.Date <= date.Date &&
//                  m.EndDate.Date >= date.Date)
//           .Select(m => new SelectListItem
//           {
//             Value = m.Id.ToString(),
//             Text = m.Title,
//             Selected = m.Id == viewModel.MaintenanceScheduleId
//           })
//           .ToListAsync();

//       // Tambahkan opsi kosong untuk maintenance
//       viewModel.MaintenanceList.Insert(0, new SelectListItem { Value = "", Text = "-- Tidak terkait maintenance --" });

//       // Daftar kategori
//       viewModel.CategoryList = Enum.GetValues(typeof(UsageCategory))
//           .Cast<UsageCategory>()
//           .Select(c => new SelectListItem
//           {
//             Value = c.ToString(),
//             Text = c.ToString(),
//             Selected = c == viewModel.Category
//           })
//           .ToList();

//       // Daftar subkategori berdasarkan kategori yang dipilih
//       viewModel.SubcategoryList = await _context.UsageSubcategories
//           .Where(s => s.Category == viewModel.Category && s.IsActive)
//           .Select(s => new SelectListItem
//           {
//             Value = s.Id.ToString(),
//             Text = s.Name,
//             Selected = s.Id == viewModel.UsageSubcategoryId
//           })
//           .ToListAsync();

//       // Tambahkan opsi kosong untuk subkategori
//       viewModel.SubcategoryList.Insert(0, new SelectListItem { Value = "", Text = "-- Pilih Subkategori --" });
//     }
//   }
// }

// Controllers/CraneUsageController.cs
using AspnetCoreMvcFull.Services.CraneUsage;
using AspnetCoreMvcFull.ViewModels.CraneUsage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Models;
using System.Security.Claims;
using AspnetCoreMvcFull.Data;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Controllers
{
  [Authorize]
  public class CraneUsageController : Controller
  {
    private readonly ICraneUsageService _craneUsageService;
    private readonly ILogger<CraneUsageController> _logger;
    private readonly AppDbContext _context;

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

    // GET: CraneUsage/Details/5
    public async Task<IActionResult> Details(int id)
    {
      try
      {
        var viewModel = await _craneUsageService.GetUsageRecordByIdAsync(id);
        if (viewModel.Id == 0)
        {
          return NotFound();
        }
        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error retrieving crane usage record with ID {RecordId}", id);
        TempData["ErrorMessage"] = "Terjadi kesalahan saat mengambil detail penggunaan crane.";
        return RedirectToAction(nameof(Index));
      }
    }

    // GET: CraneUsage/Create
    public async Task<IActionResult> Create()
    {
      try
      {
        var viewModel = new CraneUsageRecordViewModel
        {
          StartTime = DateTime.Now,
          EndTime = DateTime.Now.AddHours(1)
        };

        // Initialize dropdowns
        viewModel.CraneList = await _craneUsageService.GetSubcategoriesByCategoryAsync(UsageCategory.Operating);
        viewModel.CategoryList = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>(
            Enum.GetValues(typeof(UsageCategory))
                .Cast<UsageCategory>()
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                  Value = ((int)c).ToString(),
                  Text = c.ToString()
                }));

        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error initializing create crane usage form");
        TempData["ErrorMessage"] = "Terjadi kesalahan saat menyiapkan form tambah penggunaan crane.";
        return RedirectToAction(nameof(Index));
      }
    }

    // POST: CraneUsage/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CraneUsageRecordViewModel viewModel)
    {
      try
      {
        if (ModelState.IsValid)
        {
          // Get current user info from claims
          var createdBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

          await _craneUsageService.CreateUsageRecordAsync(viewModel, createdBy);
          TempData["SuccessMessage"] = "Data penggunaan crane berhasil ditambahkan.";
          return RedirectToAction(nameof(Index));
        }

        // If we got this far, something failed, redisplay form
        viewModel.CraneList = await _craneUsageService.GetSubcategoriesByCategoryAsync(viewModel.Category);
        viewModel.CategoryList = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>(
            Enum.GetValues(typeof(UsageCategory))
                .Cast<UsageCategory>()
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                  Value = ((int)c).ToString(),
                  Text = c.ToString()
                }));
        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error creating crane usage record");
        TempData["ErrorMessage"] = "Terjadi kesalahan saat menyimpan data penggunaan crane.";
        return RedirectToAction(nameof(Index));
      }
    }

    // GET: CraneUsage/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
      try
      {
        var viewModel = await _craneUsageService.GetUsageRecordByIdAsync(id);
        if (viewModel.Id == 0)
        {
          return NotFound();
        }
        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error retrieving crane usage record for edit with ID {RecordId}", id);
        TempData["ErrorMessage"] = "Terjadi kesalahan saat mengambil data penggunaan crane untuk diedit.";
        return RedirectToAction(nameof(Index));
      }
    }

    // POST: CraneUsage/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CraneUsageRecordViewModel viewModel)
    {
      if (id != viewModel.Id)
      {
        return NotFound();
      }

      try
      {
        if (ModelState.IsValid)
        {
          // Get current user info from claims
          var updatedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

          await _craneUsageService.UpdateUsageRecordAsync(viewModel, updatedBy);
          TempData["SuccessMessage"] = "Data penggunaan crane berhasil diperbarui.";
          return RedirectToAction(nameof(Index));
        }

        // If we got this far, something failed, redisplay form
        viewModel.CraneList = await _craneUsageService.GetSubcategoriesByCategoryAsync(viewModel.Category);
        viewModel.CategoryList = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>(
            Enum.GetValues(typeof(UsageCategory))
                .Cast<UsageCategory>()
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                  Value = ((int)c).ToString(),
                  Text = c.ToString()
                }));
        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error updating crane usage record with ID {RecordId}", id);
        TempData["ErrorMessage"] = "Terjadi kesalahan saat memperbarui data penggunaan crane.";
        return RedirectToAction(nameof(Index));
      }
    }

    // GET: CraneUsage/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
      try
      {
        var viewModel = await _craneUsageService.GetUsageRecordByIdAsync(id);
        if (viewModel.Id == 0)
        {
          return NotFound();
        }
        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error retrieving crane usage record for delete with ID {RecordId}", id);
        TempData["ErrorMessage"] = "Terjadi kesalahan saat mengambil data penggunaan crane untuk dihapus.";
        return RedirectToAction(nameof(Index));
      }
    }

    // POST: CraneUsage/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      try
      {
        await _craneUsageService.DeleteUsageRecordAsync(id);
        TempData["SuccessMessage"] = "Data penggunaan crane berhasil dihapus.";
        return RedirectToAction(nameof(Index));
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error deleting crane usage record with ID {RecordId}", id);
        TempData["ErrorMessage"] = "Terjadi kesalahan saat menghapus data penggunaan crane.";
        return RedirectToAction(nameof(Index));
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

    // AJAX endpoints for dependent dropdowns
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
        return Json(new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>());
      }
    }

    [HttpGet]
    public async Task<IActionResult> GetRelatedBookings(int craneId, string startTime, string endTime)
    {
      try
      {
        DateTime parsedStartTime = DateTime.Parse(startTime);
        DateTime parsedEndTime = DateTime.Parse(endTime);

        var bookings = await _craneUsageService.GetAvailableBookingsAsync(craneId, parsedStartTime, parsedEndTime);
        return Json(bookings);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting related bookings");
        return Json(new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>());
      }
    }

    [HttpGet]
    public async Task<IActionResult> GetRelatedMaintenance(int craneId, string startTime, string endTime)
    {
      try
      {
        DateTime parsedStartTime = DateTime.Parse(startTime);
        DateTime parsedEndTime = DateTime.Parse(endTime);

        var maintenance = await _craneUsageService.GetAvailableMaintenanceSchedulesAsync(craneId, parsedStartTime, parsedEndTime);
        return Json(maintenance);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting related maintenance schedules");
        return Json(new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>());
      }
    }
  }
}
