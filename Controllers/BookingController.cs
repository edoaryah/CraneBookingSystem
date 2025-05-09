using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Services.Role;
using AspnetCoreMvcFull.ViewModels.BookingManagement;
using AspnetCoreMvcFull.Models;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class BookingController : Controller
  {
    private readonly ICraneService _craneService;
    private readonly IShiftDefinitionService _shiftService;
    private readonly IHazardService _hazardService;
    private readonly IBookingService _bookingService;
    private readonly IRoleService _roleService;
    private readonly ILogger<BookingController> _logger;

    public BookingController(
        ICraneService craneService,
        IShiftDefinitionService shiftService,
        IHazardService hazardService,
        IBookingService bookingService,
        IRoleService roleService,
        ILogger<BookingController> logger)
    {
      _craneService = craneService;
      _shiftService = shiftService;
      _hazardService = hazardService;
      _bookingService = bookingService;
      _roleService = roleService;
      _logger = logger;
    }

    // GET: /Booking
    public async Task<IActionResult> Index()
    {
      try
      {
        var viewModel = new BookingFormViewModel
        {
          AvailableCranes = await _craneService.GetAllCranesAsync(),
          ShiftDefinitions = await _shiftService.GetAllShiftDefinitionsAsync(),
          AvailableHazards = await _hazardService.GetAllHazardsAsync()
        };
        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading booking form");
        TempData["ErrorMessage"] = "Error loading booking form: " + ex.Message;
        return View("Error");
      }
    }

    // GET: /Booking/List
    public async Task<IActionResult> List()
    {
      try
      {
        var bookings = await _bookingService.GetAllBookingsAsync();

        var viewModel = new BookingListViewModel
        {
          Bookings = bookings,
          SuccessMessage = TempData["SuccessMessage"] as string,
          ErrorMessage = TempData["ErrorMessage"] as string
        };

        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading bookings");
        TempData["ErrorMessage"] = "Error loading bookings: " + ex.Message;
        return View(new BookingListViewModel { ErrorMessage = ex.Message });
      }
    }

    public async Task<IActionResult> Details(string documentNumber)
    {
      try
      {
        // Get the current user's information from claims
        var ldapUser = User.FindFirst("ldapuser")?.Value;
        var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(ldapUser))
        {
          _logger.LogWarning("User LDAP username not found in claims");
          return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Details", "Booking", new { documentNumber }) });
        }

        // Get booking details
        var booking = await _bookingService.GetBookingByDocumentNumberAsync(documentNumber);

        // Cek otorisasi: Apakah pengguna memiliki akses ke booking ini
        if (booking == null)
        {
          _logger.LogWarning("Booking dengan document number {documentNumber} tidak ditemukan", documentNumber);
          return NotFound();
        }

        // Cek otorisasi: Apakah pengguna adalah PIC, atau pembuat booking, atau punya akses admin
        bool isPic = await _roleService.UserHasRoleAsync(ldapUser, "pic");
        bool isAdmin = await _roleService.UserHasRoleAsync(ldapUser, "admin");
        bool isBookingCreator = booking.Name == userName;

        // Jika bukan PIC, admin, atau pembuat booking, tolak akses
        if (!isPic && !isAdmin && !isBookingCreator)
        {
          _logger.LogWarning("User {ldapUser} mencoba mengakses booking {documentNumber} tanpa otorisasi", ldapUser, documentNumber);
          return RedirectToAction("AccessDenied", "Auth");
        }

        // Pass role information to the view
        ViewData["IsPicRole"] = isPic;
        ViewData["IsAdminRole"] = isAdmin;
        ViewData["IsBookingCreator"] = isBookingCreator;

        // Pass the booking to the view
        return View(booking);
      }
      catch (KeyNotFoundException ex)
      {
        _logger.LogWarning(ex, "Booking dengan document number {documentNumber} tidak ditemukan", documentNumber);
        return NotFound();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Terjadi kesalahan saat memuat detail booking dengan document number {documentNumber}", documentNumber);
        TempData["ErrorMessage"] = "Terjadi kesalahan saat memuat detail booking. Silakan coba lagi.";
        return RedirectToAction(nameof(List));
      }
    }

    // POST: /Booking/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateViewModel viewModel)
    {
      try
      {
        if (ModelState.IsValid)
        {
          var createdBooking = await _bookingService.CreateBookingAsync(viewModel);

          TempData["SuccessMessage"] = "Booking created successfully";
          return RedirectToAction(nameof(Details), new { documentNumber = createdBooking.DocumentNumber });
        }

        // Jika validasi gagal, kembali ke form dengan data yang dibutuhkan
        var formViewModel = new BookingFormViewModel
        {
          AvailableCranes = await _craneService.GetAllCranesAsync(),
          ShiftDefinitions = await _shiftService.GetAllShiftDefinitionsAsync(),
          AvailableHazards = await _hazardService.GetAllHazardsAsync()
        };

        // Tambahkan pesan error
        ModelState.AddModelError("", "Silakan perbaiki error dan coba lagi.");

        return View("Index", formViewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error creating booking");

        // Redirect ke Index dengan pesan error
        TempData["ErrorMessage"] = "Error membuat booking: " + ex.Message;
        return RedirectToAction("Index");
      }
    }

    // GET: /Booking/Edit/{documentNumber}
    public async Task<IActionResult> Edit(string documentNumber)
    {
      try
      {
        var booking = await _bookingService.GetBookingByDocumentNumberAsync(documentNumber);

        // Convert to update view model
        var viewModel = new BookingUpdateViewModel
        {
          Name = booking.Name,
          Department = booking.Department,
          CraneId = booking.CraneId,
          StartDate = booking.StartDate,
          EndDate = booking.EndDate,
          Location = booking.Location,
          ProjectSupervisor = booking.ProjectSupervisor,
          CostCode = booking.CostCode,
          PhoneNumber = booking.PhoneNumber,
          Description = booking.Description,
          CustomHazard = booking.CustomHazard,
          ShiftSelections = ConvertShiftsToSelections(booking),
          Items = booking.Items.Select(i => new BookingItemCreateViewModel
          {
            ItemName = i.ItemName,
            Weight = i.Weight,
            Height = i.Height,
            Quantity = i.Quantity
          }).ToList(),
          HazardIds = booking.SelectedHazards.Select(h => h.Id).ToList()
        };

        ViewBag.Cranes = await _craneService.GetAllCranesAsync();
        ViewBag.ShiftDefinitions = await _shiftService.GetAllShiftDefinitionsAsync();
        ViewBag.Hazards = await _hazardService.GetAllHazardsAsync();
        ViewBag.DocumentNumber = booking.DocumentNumber;
        ViewBag.BookingId = booking.Id;

        return View(viewModel);
      }
      catch (KeyNotFoundException)
      {
        return NotFound();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading booking for edit with document number: {DocumentNumber}", documentNumber);
        TempData["ErrorMessage"] = "Error loading booking: " + ex.Message;
        return RedirectToAction(nameof(List));
      }
    }

    // POST: /Booking/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookingUpdateViewModel viewModel)
    {
      try
      {
        if (ModelState.IsValid)
        {
          var updatedBooking = await _bookingService.UpdateBookingAsync(id, viewModel);

          TempData["SuccessMessage"] = "Booking updated successfully";
          return RedirectToAction(nameof(Details), new { documentNumber = updatedBooking.DocumentNumber });
        }

        // If we got this far, something failed, redisplay form
        ViewBag.Cranes = await _craneService.GetAllCranesAsync();
        ViewBag.ShiftDefinitions = await _shiftService.GetAllShiftDefinitionsAsync();
        ViewBag.Hazards = await _hazardService.GetAllHazardsAsync();

        // Try to get the document number
        var booking = await _bookingService.GetBookingByIdAsync(id);
        ViewBag.DocumentNumber = booking.DocumentNumber;
        ViewBag.BookingId = id;

        return View(viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error updating booking with ID: {Id}", id);
        ModelState.AddModelError("", "Error updating booking: " + ex.Message);

        ViewBag.Cranes = await _craneService.GetAllCranesAsync();
        ViewBag.ShiftDefinitions = await _shiftService.GetAllShiftDefinitionsAsync();
        ViewBag.Hazards = await _hazardService.GetAllHazardsAsync();

        // Try to get the document number
        try
        {
          var booking = await _bookingService.GetBookingByIdAsync(id);
          ViewBag.DocumentNumber = booking.DocumentNumber;
        }
        catch
        {
          // Ignore if we can't get the document number
        }

        ViewBag.BookingId = id;
        return View(viewModel);
      }
    }

    // GET: /Booking/Delete/{documentNumber}
    public async Task<IActionResult> Delete(string documentNumber)
    {
      try
      {
        var booking = await _bookingService.GetBookingByDocumentNumberAsync(documentNumber);
        return View(booking);
      }
      catch (KeyNotFoundException)
      {
        return NotFound();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading booking for deletion with document number: {DocumentNumber}", documentNumber);
        TempData["ErrorMessage"] = "Error loading booking: " + ex.Message;
        return RedirectToAction(nameof(List));
      }
    }

    // GET: /Booking/History
    public async Task<IActionResult> History()
    {
      try
      {
        var bookings = await _bookingService.GetAllBookingsAsync();

        var viewModel = new BookingListViewModel
        {
          Bookings = bookings,
          Title = "Booking History",
          SuccessMessage = TempData["SuccessMessage"] as string,
          ErrorMessage = TempData["ErrorMessage"] as string
        };

        return View("List", viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading booking history");
        TempData["ErrorMessage"] = "Error loading booking history: " + ex.Message;
        return View("List", new BookingListViewModel
        {
          Title = "Booking History",
          ErrorMessage = ex.Message
        });
      }
    }

    // POST: /Booking/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      try
      {
        await _bookingService.DeleteBookingAsync(id);

        TempData["SuccessMessage"] = "Booking deleted successfully";
        return RedirectToAction(nameof(List));
      }
      catch (KeyNotFoundException)
      {
        return NotFound();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error deleting booking with ID: {Id}", id);
        TempData["ErrorMessage"] = "Error deleting booking: " + ex.Message;
        return RedirectToAction(nameof(List));
      }
    }

    // GET: /Booking/Calendar atau /Calendar
    [Route("Booking/Calendar")]
    [Route("Calendar")] // Menambahkan route tambahan
    public async Task<IActionResult> Calendar(DateTime? startDate = null, DateTime? endDate = null)
    {
      try
      {
        // Jika startDate tidak disediakan, gunakan hari ini
        DateTime start = startDate?.Date ?? DateTime.Now.Date;

        // Jika endDate tidak disediakan, gunakan 6 hari setelah startDate (total 7 hari)
        DateTime end = endDate?.Date ?? start.AddDays(6);

        // Ambil data kalender
        var calendarData = await _bookingService.GetCalendarViewAsync(start, end);

        // Ambil data shift definitions dan kirim ke view via ViewBag
        ViewBag.ShiftDefinitions = await _shiftService.GetAllShiftDefinitionsAsync();

        return View(calendarData);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading calendar view");
        TempData["ErrorMessage"] = "Error loading calendar: " + ex.Message;
        return View("Error");
      }
    }

    // GET: /Booking/Approved
    public async Task<IActionResult> Approved()
    {
      try
      {
        var approvedBookings = await _bookingService.GetBookingsByStatusAsync(BookingStatus.PICApproved);

        var viewModel = new BookingListViewModel
        {
          Bookings = approvedBookings,
          Title = "Approved Bookings",
          SuccessMessage = TempData["SuccessMessage"] as string,
          ErrorMessage = TempData["ErrorMessage"] as string
        };

        return View("List", viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error loading approved bookings");
        TempData["ErrorMessage"] = "Error loading approved bookings: " + ex.Message;
        return View(new BookingListViewModel
        {
          Title = "Approved Bookings",
          ErrorMessage = ex.Message
        });
      }
    }

    // GET: /Booking/GetBookedShifts
    [HttpGet]
    public async Task<IActionResult> GetBookedShifts(int craneId, DateTime startDate, DateTime endDate)
    {
      try
      {
        var bookedShifts = await _bookingService.GetBookedShiftsByCraneAndDateRangeAsync(
            craneId, startDate, endDate);
        return Json(bookedShifts);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting booked shifts");
        return StatusCode(500, new { error = ex.Message });
      }
    }

    // GET: /Booking/CheckShiftConflict
    [HttpGet]
    public async Task<IActionResult> CheckShiftConflict(
        int craneId,
        DateTime date,
        int shiftDefinitionId,
        int? excludeBookingId = null)
    {
      try
      {
        var hasConflict = await _bookingService.IsShiftBookingConflictAsync(
            craneId, date, shiftDefinitionId, excludeBookingId);

        return Json(new { hasConflict });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error checking shift conflict");
        return StatusCode(500, new { error = ex.Message });
      }
    }

    // Helper methods
    private List<DailyShiftSelectionViewModel> ConvertShiftsToSelections(BookingDetailViewModel booking)
    {
      // Group shifts by date
      var groupedShifts = booking.Shifts.GroupBy(s => s.Date.Date)
                                     .Select(g => new
                                     {
                                       Date = g.Key,
                                       ShiftIds = g.Select(s => s.ShiftDefinitionId).ToList()
                                     })
                                     .ToList();

      // Convert to selection view models
      return groupedShifts.Select(g => new DailyShiftSelectionViewModel
      {
        Date = g.Date,
        SelectedShiftIds = g.ShiftIds
      }).ToList();
    }
  }
}
