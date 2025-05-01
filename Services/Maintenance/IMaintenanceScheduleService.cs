using AspnetCoreMvcFull.DTOs;

namespace AspnetCoreMvcFull.Services
{
  public interface IMaintenanceScheduleService
  {
    Task<IEnumerable<MaintenanceScheduleDto>> GetAllMaintenanceSchedulesAsync();
    Task<MaintenanceScheduleDetailDto> GetMaintenanceScheduleByIdAsync(int id);
    Task<IEnumerable<MaintenanceScheduleDto>> GetMaintenanceSchedulesByCraneIdAsync(int craneId);
    Task<MaintenanceScheduleDetailDto> CreateMaintenanceScheduleAsync(MaintenanceScheduleCreateDto maintenanceDto);
    Task<MaintenanceScheduleDetailDto> UpdateMaintenanceScheduleAsync(int id, MaintenanceScheduleUpdateDto maintenanceDto);
    Task DeleteMaintenanceScheduleAsync(int id);
    Task<bool> IsShiftMaintenanceConflictAsync(int craneId, DateTime date, int shiftDefinitionId, int? excludeMaintenanceId = null);
    Task<bool> MaintenanceScheduleExistsAsync(int id);
  }
}
