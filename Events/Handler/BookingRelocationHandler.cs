using AspnetCoreMvcFull.Services;

namespace AspnetCoreMvcFull.Events.Handlers
{
  public class BookingRelocationHandler : IEventHandler<CraneMaintenanceEvent>
  {
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingRelocationHandler> _logger;
    private readonly IScheduleConflictService _scheduleConflictService;

    public BookingRelocationHandler(
        IBookingService bookingService,
        ILogger<BookingRelocationHandler> logger,
        IScheduleConflictService scheduleConflictService)
    {
      _bookingService = bookingService;
      _logger = logger;
      _scheduleConflictService = scheduleConflictService;
    }

    public async Task HandleAsync(CraneMaintenanceEvent @event)
    {
      try
      {
        _logger.LogInformation("Processing booking relocation for crane {CraneId} maintenance", @event.CraneId);

        await _bookingService.RelocateAffectedBookingsAsync(
            @event.CraneId,
            @event.MaintenanceStartTime,
            @event.MaintenanceEndTime);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error relocating bookings for crane {CraneId}", @event.CraneId);
        throw;
      }
    }
  }
}
