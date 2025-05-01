using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models; // Tambahkan ini untuk mengakses BookingStatus

namespace AspnetCoreMvcFull.Services
{
  public interface IBookingService
  {
    Task<IEnumerable<BookingDto>> GetAllBookingsAsync();
    Task<BookingDetailDto> GetBookingByIdAsync(int id);
    Task<IEnumerable<BookingDto>> GetBookingsByCraneIdAsync(int craneId);
    Task<CalendarResponseDto> GetCalendarViewAsync(DateTime startDate, DateTime endDate);
    Task<BookingDetailDto> CreateBookingAsync(BookingCreateDto bookingDto);
    Task<BookingDetailDto> UpdateBookingAsync(int id, BookingUpdateDto bookingDto);
    Task DeleteBookingAsync(int id);
    Task<bool> BookingExistsAsync(int id);

    // Method for shift conflict checking
    Task<bool> IsShiftBookingConflictAsync(int craneId, DateTime date, int shiftDefinitionId, int? excludeBookingId = null);

    Task<int> RelocateAffectedBookingsAsync(int craneId, DateTime maintenanceStartTime, DateTime maintenanceEndTime);

    // Metode baru untuk mendapatkan booking berdasarkan status
    Task<IEnumerable<BookingDetailDto>> GetBookingsByStatusAsync(BookingStatus status);
  }
}
