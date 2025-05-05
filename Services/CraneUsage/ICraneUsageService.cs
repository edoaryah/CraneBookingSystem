// Tambahkan file Services/CraneUsage/ICraneUsageService.cs

using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.ViewModels.CraneUsage;

namespace AspnetCoreMvcFull.Services.CraneUsage
{
  public interface ICraneUsageService
  {
    Task<IEnumerable<CraneUsageRecordViewModel>> GetAllUsageRecordsAsync();
    Task<IEnumerable<CraneUsageRecordViewModel>> GetUsageRecordsByBookingIdAsync(int bookingId);
    Task<CraneUsageRecordViewModel> GetUsageRecordByIdAsync(int id);
    Task<CraneUsageRecordViewModel> CreateUsageRecordAsync(CraneUsageRecordCreateViewModel viewModel, string createdBy);
    Task<CraneUsageRecordViewModel> UpdateUsageRecordAsync(int id, CraneUsageRecordUpdateViewModel viewModel, string updatedBy);
    Task DeleteUsageRecordAsync(int id);

    // Get usage summary for a booking
    Task<UsageSummaryViewModel> GetUsageSummaryByBookingIdAsync(int bookingId);

    // Get subcategories for a category
    Task<IEnumerable<UsageSubcategoryViewModel>> GetSubcategoriesByCategoryAsync(UsageCategory category);
  }
}
