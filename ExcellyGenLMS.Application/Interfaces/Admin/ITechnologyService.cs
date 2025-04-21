using ExcellyGenLMS.Application.DTOs.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Admin
{
    public interface ITechnologyService
    {
        Task<List<TechnologyDto>> GetAllTechnologiesAsync();
        Task<TechnologyDto> GetTechnologyByIdAsync(string id);
        Task<TechnologyDto> CreateTechnologyAsync(CreateTechnologyDto createTechnologyDto);
        Task<TechnologyDto> UpdateTechnologyAsync(string id, UpdateTechnologyDto updateTechnologyDto);
        Task DeleteTechnologyAsync(string id);
        Task<TechnologyDto> ToggleTechnologyStatusAsync(string id);
    }
}