using AspnetCoreMvcFull.DTOs;

namespace AspnetCoreMvcFull.Services
{
  public interface IHazardService
  {
    Task<IEnumerable<HazardDto>> GetAllHazardsAsync();
    Task<HazardDto> GetHazardByIdAsync(int id);
    Task<HazardDto> CreateHazardAsync(HazardCreateDto hazardDto);
    Task<HazardDto> UpdateHazardAsync(int id, HazardUpdateDto hazardDto);
    Task DeleteHazardAsync(int id);
    Task<bool> HazardExistsAsync(int id);
  }
}
