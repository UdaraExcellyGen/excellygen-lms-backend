// Path: ExcellyGenLMS.Application/Interfaces/Admin/ITechnologyService.cs

using ExcellyGenLMS.Application.DTOs.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Admin
{
    public interface ITechnologyService
    {
        Task<List<TechnologyDto>> GetAllTechnologiesAsync();
        Task<TechnologyDto> GetTechnologyByIdAsync(string id);
        Task<TechnologyDto> CreateTechnologyAsync(CreateTechnologyDto createTechnologyDto, string creatorId = "system", string creatorType = "admin");
        Task<TechnologyDto> UpdateTechnologyAsync(string id, UpdateTechnologyDto updateTechnologyDto, bool isAdmin = false);
        Task DeleteTechnologyAsync(string id, bool isAdmin = false);
        Task<TechnologyDto> ToggleTechnologyStatusAsync(string id);
    }
}