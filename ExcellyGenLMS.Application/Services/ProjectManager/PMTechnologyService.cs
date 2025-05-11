// Path: ExcellyGenLMS.Application/Services/ProjectManager/PMTechnologyService.cs

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.ProjectManager;
using ExcellyGenLMS.Application.Interfaces.ProjectManager;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;

namespace ExcellyGenLMS.Application.Services.ProjectManager
{
    public class PMTechnologyService : IPMTechnologyService
    {
        private readonly ITechnologyRepository _technologyRepository;

        public PMTechnologyService(ITechnologyRepository technologyRepository)
        {
            _technologyRepository = technologyRepository;
        }

        public async Task<IEnumerable<TechnologyDto>> GetTechnologiesAsync()
        {
            var technologies = await _technologyRepository.GetAllTechnologiesAsync();
            
            return technologies.Select(t => new TechnologyDto
            {
                Id = t.Id,
                Name = t.Name,
                Status = t.Status,
                CreatorType = t.CreatorType ?? "admin", // Default to admin if null
                CreatorId = t.CreatorId ?? "system"     // Default to system if null
            }).ToList();
        }
    }
}