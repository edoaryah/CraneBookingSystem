// Services/CraneUsage/ICraneUsageService.cs (Updated)
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.ViewModels.CraneUsage;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AspnetCoreMvcFull.Services.CraneUsage
{
  public interface ICraneUsageService
  {
    // Metode untuk form penggunaan crane
    Task<List<CraneUsageEntryViewModel>> GetCraneUsageEntriesForDateAsync(int craneId, DateTime date);
    Task<bool> SaveCraneUsageFormAsync(CraneUsageFormViewModel viewModel, string userName);
    Task<bool> AddCraneUsageEntryAsync(int craneId, DateTime date, CraneUsageEntryViewModel entry, string userName);
    Task<bool> UpdateCraneUsageEntryAsync(CraneUsageEntryViewModel entry);
    Task<bool> DeleteCraneUsageEntryAsync(int entryId);
    Task<CraneUsageEntryViewModel> GetCraneUsageEntryByIdAsync(int id);
    Task<CraneUsageEntryViewModel> GetCraneUsageEntryByTimeAsync(int craneId, DateTime date, TimeSpan startTime, TimeSpan endTime);
    bool ValidateNoTimeConflicts(List<CraneUsageEntryViewModel> existingEntries, CraneUsageEntryViewModel newEntry);

    // Metode untuk dropdown
    Task<List<SelectListItem>> GetSubcategoriesByCategoryAsync(UsageCategory category);
    Task<List<SelectListItem>> GetAvailableBookingsAsync(int craneId, DateTime startTime, DateTime endTime, int? currentEntryId = null);
    Task<List<SelectListItem>> GetAvailableMaintenanceSchedulesAsync(int craneId, DateTime startTime, DateTime endTime, int? currentEntryId = null);

    // Metode untuk list dan filter
    Task<CraneUsageListViewModel> GetFilteredUsageRecordsAsync(CraneUsageFilterViewModel filter);

    // Untuk booking
    Task<bool> SaveBookingUsageFormAsync(BookingUsageFormViewModel viewModel, string userName);

    Task<CraneUsageMinuteVisualizationViewModel> GetMinuteVisualizationDataAsync(int craneId, DateTime date);
  }
}
