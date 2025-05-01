namespace AspnetCoreMvcFull.Services
{
  public interface IScheduleConflictService
  {
    Task<bool> IsBookingConflictAsync(int craneId, DateTime date, int shiftDefinitionId, int? excludeBookingId = null);
    Task<bool> IsMaintenanceConflictAsync(int craneId, DateTime date, int shiftDefinitionId, int? excludeMaintenanceId = null);
  }
}
