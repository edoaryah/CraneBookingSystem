using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AspnetCoreMvcFull.Models; // Untuk mengakses BookingStatus
using Microsoft.Extensions.Logging; // Tambahkan ini untuk ILogger

namespace AspnetCoreMvcFull.Controllers.Api
{
  [Route("api/[controller]")]
  [ApiController]
  // [Authorize]
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class BookingsController : ControllerBase
  {
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingsController> _logger; // Tambahkan field ini

    public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
    {
      _bookingService = bookingService;
      _logger = logger; // Inisialisasi logger
    }

    // GET: api/Bookings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookings()
    {
      var bookings = await _bookingService.GetAllBookingsAsync();
      return Ok(bookings);
    }

    // GET: api/Bookings/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BookingDetailDto>> GetBooking(int id)
    {
      var booking = await _bookingService.GetBookingByIdAsync(id);
      return Ok(booking);
    }

    // GET: api/Bookings/Crane/5
    [HttpGet("Crane/{craneId}")]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookingsByCrane(int craneId)
    {
      var bookings = await _bookingService.GetBookingsByCraneIdAsync(craneId);
      return Ok(bookings);
    }

    [HttpGet("CalendarView")]
    public async Task<ActionResult<CalendarResponseDto>> GetCalendarView(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
      // Jika startDate tidak disediakan, gunakan hari ini
      DateTime start = startDate?.Date ?? DateTime.Now.Date;

      // Jika endDate tidak disediakan, gunakan 6 hari setelah startDate (total 7 hari)
      DateTime end = endDate?.Date ?? start.AddDays(6);

      var calendarData = await _bookingService.GetCalendarViewAsync(start, end);
      return Ok(calendarData);
    }

    // Controllers/Api/BookingsController.cs - tambahkan endpoint untuk mendapatkan approved bookings
    // GET: api/Bookings/Approved
    [HttpGet("Approved")]
    public async Task<ActionResult<IEnumerable<BookingDetailDto>>> GetApprovedBookings()
    {
      try
      {
        // Mendapatkan semua booking dengan status Approved menggunakan BookingService
        var bookings = await _bookingService.GetBookingsByStatusAsync(BookingStatus.PICApproved);
        return Ok(bookings);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error fetching approved bookings");
        return StatusCode(500, new { message = "Terjadi kesalahan saat mengambil data booking yang disetujui." });
      }
    }

    // POST: api/Bookings
    [HttpPost]
    public async Task<ActionResult<BookingDetailDto>> CreateBooking(BookingCreateDto bookingDto)
    {
      var result = await _bookingService.CreateBookingAsync(bookingDto);
      return CreatedAtAction(nameof(GetBooking), new { id = result.Id }, result);
    }

    // PUT: api/Bookings/5
    [HttpPut("{id}")]
    public async Task<ActionResult<BookingDetailDto>> UpdateBooking(int id, BookingUpdateDto bookingDto)
    {
      var result = await _bookingService.UpdateBookingAsync(id, bookingDto);
      return Ok(result);
    }

    // DELETE: api/Bookings/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBooking(int id)
    {
      await _bookingService.DeleteBookingAsync(id);
      return NoContent();
    }

    // Endpoint baru untuk pengecekan konflik shift
    // GET: api/Bookings/CheckShiftConflict?craneId=1&date=2025-04-01&shiftDefinitionId=2
    [HttpGet("CheckShiftConflict")]
    public async Task<ActionResult<bool>> CheckShiftConflict(
        [FromQuery] int craneId,
        [FromQuery] DateTime date,
        [FromQuery] int shiftDefinitionId,
        [FromQuery] int? excludeBookingId = null)
    {
      var hasConflict = await _bookingService.IsShiftBookingConflictAsync(
          craneId, date, shiftDefinitionId, excludeBookingId);

      return Ok(hasConflict);
    }
  }
}
