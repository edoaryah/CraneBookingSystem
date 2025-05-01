using AspnetCoreMvcFull.DTOs;

namespace AspnetCoreMvcFull.Services
{
  public interface IShiftDefinitionService
  {
    Task<IEnumerable<ShiftDefinitionDto>> GetAllShiftDefinitionsAsync();
    Task<ShiftDefinitionDto> GetShiftDefinitionByIdAsync(int id);
    Task<ShiftDefinitionDto> CreateShiftDefinitionAsync(ShiftDefinitionCreateDto shiftDto);
    Task<ShiftDefinitionDto> UpdateShiftDefinitionAsync(int id, ShiftDefinitionUpdateDto shiftDto);
    Task DeleteShiftDefinitionAsync(int id);
    Task<bool> ShiftDefinitionExistsAsync(int id);
  }
}
