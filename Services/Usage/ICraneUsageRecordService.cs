using AspnetCoreMvcFull.DTOs.Usage;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services.Usage
{
  public interface ICraneUsageRecordService
  {
    Task<IEnumerable<CraneUsageRecordDto>> GetAllUsageRecordsAsync();
    Task<IEnumerable<CraneUsageRecordDto>> GetUsageRecordsByBookingIdAsync(int bookingId);
    Task<CraneUsageRecordDto> GetUsageRecordByIdAsync(int id);
    Task<CraneUsageRecordDto> CreateUsageRecordAsync(CraneUsageRecordCreateDto recordDto, string createdBy);
    Task<CraneUsageRecordDto> UpdateUsageRecordAsync(int id, CraneUsageRecordUpdateDto recordDto, string updatedBy);
    Task DeleteUsageRecordAsync(int id);

    // Get usage summary for a booking
    Task<UsageSummaryDto> GetUsageSummaryByBookingIdAsync(int bookingId);

    // Get subcategories for a category
    Task<IEnumerable<UsageSubcategoryDto>> GetSubcategoriesByCategoryAsync(UsageCategory category);
  }
}
