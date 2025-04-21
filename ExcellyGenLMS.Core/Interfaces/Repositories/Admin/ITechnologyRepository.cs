using ExcellyGenLMS.Core.Entities.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Admin
{
    public interface ITechnologyRepository
    {
        Task<List<Technology>> GetAllTechnologiesAsync();
        Task<Technology?> GetTechnologyByIdAsync(string id);
        Task<Technology> CreateTechnologyAsync(Technology technology);
        Task<Technology> UpdateTechnologyAsync(Technology technology);
        Task DeleteTechnologyAsync(string id);
        Task<Technology> ToggleTechnologyStatusAsync(string id);
    }
}