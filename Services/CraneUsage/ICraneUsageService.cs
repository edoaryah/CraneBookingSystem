// Services/CraneUsage/ICraneUsageService.cs
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.ViewModels.CraneUsage;

namespace AspnetCoreMvcFull.Services.CraneUsage
{
  public interface ICraneUsageService
  {
    Task<List<CraneUsageRecordViewModel>> GetAllUsageRecordsAsync(CraneUsageFilterViewModel filter);
    Task<CraneUsageRecordViewModel> GetUsageRecordByIdAsync(int id);
    Task<CraneUsageRecordViewModel> CreateUsageRecordAsync(CraneUsageRecordViewModel model, string createdBy);
    Task<CraneUsageRecordViewModel> UpdateUsageRecordAsync(CraneUsageRecordViewModel model, string updatedBy);
    Task<bool> DeleteUsageRecordAsync(int id);
    Task<CraneUsageListViewModel> GetFilteredUsageRecordsAsync(CraneUsageFilterViewModel filter);
    Task<CraneUsageVisualizationViewModel> GetUsageVisualizationDataAsync(int craneId, DateTime date);
    Task<List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>> GetSubcategoriesByCategoryAsync(UsageCategory category);
    Task<List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>> GetAvailableBookingsAsync(int craneId, DateTime startTime, DateTime endTime, int? currentRecordId = null);
    Task<List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>> GetAvailableMaintenanceSchedulesAsync(int craneId, DateTime startTime, DateTime endTime, int? currentRecordId = null);
  }
}
