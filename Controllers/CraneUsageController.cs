// Controllers/CraneUsageController.cs
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Services.CraneUsage;
using AspnetCoreMvcFull.ViewModels.BookingManagement;
using AspnetCoreMvcFull.ViewModels.CraneUsage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

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

        // Menggunakan ViewBag untuk pesan dari TempData
        ViewBag.SuccessMessage = TempData["CraneUsageSuccessMessage"] as string;
        ViewBag.ErrorMessage = TempData["CraneUsageErrorMessage"] as string;

        // Hapus TempData setelah digunakan
        TempData.Remove("CraneUsageSuccessMessage");
        TempData.Remove("CraneUsageErrorMessage");

        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error retrieving crane usage records");
        TempData["CraneUsageErrorMessage"] = "Terjadi kesalahan saat mengambil data penggunaan crane: " + ex.Message;
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

        // Menggunakan ViewBag untuk pesan dari TempData
        ViewBag.SuccessMessage = TempData["CraneUsageSuccessMessage"] as string;
        ViewBag.ErrorMessage = TempData["CraneUsageErrorMessage"] as string;

        // Hapus TempData setelah digunakan
        TempData.Remove("CraneUsageSuccessMessage");
        TempData.Remove("CraneUsageErrorMessage");

        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading crane usage form");
        TempData["CraneUsageErrorMessage"] = "Terjadi kesalahan saat memuat form penggunaan crane: " + ex.Message;
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
            TempData["CraneUsageSuccessMessage"] = "Data penggunaan crane berhasil disimpan.";
            return RedirectToAction(nameof(Form), new { craneId = viewModel.CraneId, date = viewModel.Date.ToString("yyyy-MM-dd") });
          }
          else
          {
            TempData["CraneUsageErrorMessage"] = "Terdapat konflik waktu pada entri penggunaan. Pastikan waktu tidak tumpang tindih.";
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
        TempData["CraneUsageErrorMessage"] = "Terjadi kesalahan saat menyimpan data penggunaan crane: " + ex.Message;

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

        // Menggunakan ViewBag untuk pesan dari TempData
        ViewBag.SuccessMessage = TempData["CraneUsageSuccessMessage"] as string;
        ViewBag.ErrorMessage = TempData["CraneUsageErrorMessage"] as string;

        // Hapus TempData setelah digunakan
        TempData.Remove("CraneUsageSuccessMessage");
        TempData.Remove("CraneUsageErrorMessage");

        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error retrieving crane usage visualization data");
        TempData["CraneUsageErrorMessage"] = "Terjadi kesalahan saat memuat visualisasi penggunaan crane: " + ex.Message;
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

    // Metode tambahan untuk List.cshtml dan BookingForm.cshtml

    // GET: CraneUsage/List
    [HttpGet]
    public async Task<IActionResult> List()
    {
      try
      {
        // Ambil booking dengan status Cancelled atau Done
        var bookings = await _context.Bookings
            .Include(b => b.Crane)
            .Where(b => b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.Done)
            .OrderByDescending(b => b.SubmitTime)  // Booking terbaru di atas
            .Select(b => new BookingViewModel
            {
              Id = b.Id,
              BookingNumber = b.BookingNumber,
              CraneCode = b.Crane != null ? b.Crane.Code : "N/A",
              StartDate = b.StartDate,
              EndDate = b.EndDate,
              Department = b.Department,
              Location = b.Location ?? "N/A",
              Status = b.Status
            })
            .ToListAsync();

        // Menggunakan ViewBag untuk pesan dari TempData
        ViewBag.SuccessMessage = TempData["CraneUsageSuccessMessage"] as string;
        ViewBag.ErrorMessage = TempData["CraneUsageErrorMessage"] as string;

        // Hapus TempData setelah digunakan
        TempData.Remove("CraneUsageSuccessMessage");
        TempData.Remove("CraneUsageErrorMessage");

        return View(bookings);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading bookings for crane usage input");
        TempData["CraneUsageErrorMessage"] = "Terjadi kesalahan saat memuat daftar booking: " + ex.Message;
        return View(new List<BookingViewModel>());
      }
    }

    // GET: CraneUsage/BookingForm
    [HttpGet]
    public async Task<IActionResult> BookingForm(int bookingId, DateTime? date = null)
    {
      try
      {
        // Ambil detail booking
        var booking = await _context.Bookings
            .Include(b => b.Crane)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
          TempData["CraneUsageErrorMessage"] = "Booking tidak ditemukan.";
          return RedirectToAction(nameof(List));
        }

        // Default tanggal ke hari ini jika tidak dispesifikasikan
        var viewDate = date ?? DateTime.Today;

        // Buat view model
        var viewModel = new BookingUsageFormViewModel
        {
          BookingId = booking.Id,
          BookingNumber = booking.BookingNumber,
          BookingName = booking.Name,
          Department = booking.Department,
          CraneCode = booking.Crane?.Code ?? "N/A",
          CraneId = booking.CraneId,
          StartDate = booking.StartDate,
          EndDate = booking.EndDate,
          Location = booking.Location ?? "N/A",
          Status = booking.Status,
          Date = viewDate,
          OperatorName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty
        };

        // Load entri yang sudah ada untuk tanggal ini
        if (booking.CraneId > 0)
        {
          viewModel.Entries = await _craneUsageService.GetCraneUsageEntriesForDateAsync(booking.CraneId, viewDate);

          // Filter entri untuk menampilkan hanya yang terkait dengan booking ini
          viewModel.Entries = viewModel.Entries
              .Where(e => e.BookingId == booking.Id)
              .ToList();
        }

        // Menggunakan ViewBag untuk pesan dari TempData
        ViewBag.SuccessMessage = TempData["CraneUsageSuccessMessage"] as string;
        ViewBag.ErrorMessage = TempData["CraneUsageErrorMessage"] as string;

        // Hapus TempData setelah digunakan
        TempData.Remove("CraneUsageSuccessMessage");
        TempData.Remove("CraneUsageErrorMessage");

        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading booking form for crane usage with ID: {BookingId}", bookingId);
        TempData["CraneUsageErrorMessage"] = "Terjadi kesalahan saat memuat form penggunaan crane: " + ex.Message;
        return RedirectToAction(nameof(List));
      }
    }

    // POST: CraneUsage/BookingForm
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookingForm(BookingUsageFormViewModel viewModel)
    {
      try
      {
        if (ModelState.IsValid)
        {
          // Ambil info user saat ini dari claims
          var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

          // Pastikan semua entri memiliki BookingId yang diset
          foreach (var entry in viewModel.Entries)
          {
            entry.BookingId = viewModel.BookingId;
          }

          // Coba simpan form menggunakan metode baru
          var success = await _craneUsageService.SaveBookingUsageFormAsync(viewModel, userName);

          if (success)
          {
            TempData["CraneUsageSuccessMessage"] = "Data penggunaan crane berhasil disimpan.";
            return RedirectToAction(nameof(BookingForm), new { bookingId = viewModel.BookingId, date = viewModel.Date.ToString("yyyy-MM-dd") });
          }
          else
          {
            TempData["CraneUsageErrorMessage"] = "Terdapat konflik waktu pada entri penggunaan. Pastikan waktu tidak tumpang tindih.";
          }
        }

        // Jika validasi gagal, kembalikan form dengan data yang dibutuhkan
        // Muat ulang detail booking
        var booking = await _context.Bookings
            .Include(b => b.Crane)
            .FirstOrDefaultAsync(b => b.Id == viewModel.BookingId);

        if (booking == null)
        {
          TempData["CraneUsageErrorMessage"] = "Booking tidak ditemukan.";
          return RedirectToAction(nameof(List));
        }

        // Update viewModel dengan detail booking
        viewModel.BookingNumber = booking.BookingNumber;
        viewModel.BookingName = booking.Name;
        viewModel.Department = booking.Department;
        viewModel.CraneCode = booking.Crane?.Code ?? "N/A";
        viewModel.StartDate = booking.StartDate;
        viewModel.EndDate = booking.EndDate;
        viewModel.Location = booking.Location ?? "N/A";
        viewModel.Status = booking.Status;

        // Muat ulang entri jika validasi gagal
        var entries = await _craneUsageService.GetCraneUsageEntriesForDateAsync(viewModel.CraneId, viewModel.Date);
        viewModel.Entries = entries.Where(e => e.BookingId == viewModel.BookingId).ToList();

        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error saving booking usage form");
        TempData["CraneUsageErrorMessage"] = "Terjadi kesalahan saat menyimpan data penggunaan crane: " + ex.Message;

        // Kembali ke form dengan ID booking dan tanggal
        return RedirectToAction(nameof(BookingForm), new { bookingId = viewModel.BookingId, date = viewModel.Date.ToString("yyyy-MM-dd") });
      }
    }

    // AJAX: CraneUsage/AddBookingEntry
    [HttpPost]
    public async Task<IActionResult> AddBookingEntry(CraneUsageEntryViewModel entry, int craneId, DateTime date, string operatorName, int bookingId)
    {
      try
      {
        // Validasi format waktu
        if (entry.StartTime >= entry.EndTime)
        {
          return Json(new { success = false, message = "Waktu mulai harus lebih awal dari waktu selesai." });
        }

        // Set booking ID
        entry.BookingId = bookingId;

        // Cek konflik dengan entri yang sudah ada
        var existingEntries = await _craneUsageService.GetCraneUsageEntriesForDateAsync(craneId, date);
        if (!_craneUsageService.ValidateNoTimeConflicts(existingEntries, entry))
        {
          return Json(new { success = false, message = "Terdapat konflik waktu dengan entri yang sudah ada." });
        }

        // Tambah entri baru
        var result = await _craneUsageService.AddCraneUsageEntryAsync(craneId, date, entry, operatorName);

        if (result)
        {
          // Ambil entri lengkap dengan ID dan navigation properties
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
        _logger.LogError(ex, "Error adding booking crane usage entry");
        return Json(new { success = false, message = "Terjadi kesalahan saat menambahkan entri penggunaan crane." });
      }
    }

    // AJAX: CraneUsage/UpdateBookingEntry
    [HttpPost]
    public async Task<IActionResult> UpdateBookingEntry([FromBody] CraneUsageEntryViewModel entry)
    {
      try
      {
        // Log detail request yang diterima
        _logger.LogInformation($"Request body: {JsonSerializer.Serialize(entry)}");
        _logger.LogInformation($"ID Entri: {entry.Id}, Type: {entry.Id.GetType().Name}");

        // PERBAIKAN: Cek nilai ID, pastikan integer dan positif
        if (entry == null || entry.Id <= 0)
        {
          _logger.LogWarning($"ID entri tidak valid atau null: {(entry == null ? "null" : entry.Id.ToString())}");
          return Json(new { success = false, message = "ID entri tidak valid." });
        }

        // Buat ID query yang eksplisit sebagai int
        int entryId = entry.Id;
        _logger.LogInformation($"Mencari entri dengan ID: {entryId}");

        // Coba ambil entri dari database
        var existingEntry = await _context.CraneUsageEntries.FirstOrDefaultAsync(e => e.Id == entryId);

        if (existingEntry == null)
        {
          // Log data diagnostik untuk debug
          _logger.LogWarning($"Entri dengan ID {entryId} tidak ditemukan");

          // Cek total entri dan beberapa ID teratas
          var allEntryIds = await _context.CraneUsageEntries
              .Select(e => e.Id)
              .OrderBy(id => id)
              .Take(10)
              .ToListAsync();

          _logger.LogInformation($"Beberapa ID entri yang ada: {string.Join(", ", allEntryIds)}");

          return Json(new { success = false, message = $"Entri dengan ID {entryId} tidak tersedia di database." });
        }

        _logger.LogInformation($"Entri ditemukan dengan ID {existingEntry.Id}");

        // Parse waktu yang diterima
        TimeSpan startTime, endTime;

        // Pastikan waktu diberikan sebagai string HH:MM
        if (!TimeSpan.TryParse(entry.StartTime.ToString(), out startTime) ||
            !TimeSpan.TryParse(entry.EndTime.ToString(), out endTime))
        {
          _logger.LogWarning($"Format waktu tidak valid: {entry.StartTime} - {entry.EndTime}");
          return Json(new { success = false, message = "Format waktu tidak valid." });
        }

        _logger.LogInformation($"Waktu valid: {startTime} - {endTime}");

        // Verifikasi waktu mulai < waktu selesai
        if (startTime >= endTime)
        {
          _logger.LogWarning($"Validasi waktu gagal: {startTime} >= {endTime}");
          return Json(new { success = false, message = "Waktu mulai harus lebih awal dari waktu selesai." });
        }

        // Update entri
        existingEntry.StartTime = startTime;
        existingEntry.EndTime = endTime;
        existingEntry.Category = entry.Category;
        existingEntry.UsageSubcategoryId = entry.UsageSubcategoryId;
        existingEntry.Notes = entry.Notes;

        // Simpan perubahan
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Entri berhasil diupdate di database");

        // Ambil data subcategory
        var subcategory = await _context.UsageSubcategories.FindAsync(entry.UsageSubcategoryId);

        // Buat response
        var updatedEntry = new CraneUsageEntryViewModel
        {
          Id = existingEntry.Id,
          StartTime = existingEntry.StartTime,
          EndTime = existingEntry.EndTime,
          Category = existingEntry.Category,
          CategoryName = existingEntry.Category.ToString(),
          UsageSubcategoryId = existingEntry.UsageSubcategoryId,
          SubcategoryName = subcategory?.Name ?? "",
          BookingId = existingEntry.BookingId,
          Notes = existingEntry.Notes
        };

        // Tambahkan booking info jika ada
        if (existingEntry.BookingId.HasValue)
        {
          var booking = await _context.Bookings.FindAsync(existingEntry.BookingId.Value);
          if (booking != null)
          {
            updatedEntry.BookingNumber = booking.BookingNumber;
          }
        }

        return Json(new { success = true, entry = updatedEntry });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error saat update entri: {Message}", ex.Message);
        return Json(new { success = false, message = $"Terjadi kesalahan: {ex.Message}" });
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
