using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.Interfaces.Admin;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Admin
{
    public class TechnologyService : ITechnologyService
    {
        private readonly ITechnologyRepository _technologyRepository;

        public TechnologyService(ITechnologyRepository technologyRepository)
        {
            _technologyRepository = technologyRepository;
        }

        public async Task<List<TechnologyDto>> GetAllTechnologiesAsync()
        {
            var technologies = await _technologyRepository.GetAllTechnologiesAsync();
            return technologies.Select(MapToDto).ToList();
        }

        public async Task<TechnologyDto> GetTechnologyByIdAsync(string id)
        {
            var technology = await _technologyRepository.GetTechnologyByIdAsync(id);
            if (technology == null)
                throw new KeyNotFoundException($"Technology with ID {id} not found");

            return MapToDto(technology);
        }

        public async Task<TechnologyDto> CreateTechnologyAsync(CreateTechnologyDto createTechnologyDto)
        {
            var technology = new Technology
            {
                Id = Guid.NewGuid().ToString(),
                Name = createTechnologyDto.Name,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            var createdTechnology = await _technologyRepository.CreateTechnologyAsync(technology);
            return MapToDto(createdTechnology);
        }

        public async Task<TechnologyDto> UpdateTechnologyAsync(string id, UpdateTechnologyDto updateTechnologyDto)
        {
            var technology = await _technologyRepository.GetTechnologyByIdAsync(id);
            if (technology == null)
                throw new KeyNotFoundException($"Technology with ID {id} not found");

            technology.Name = updateTechnologyDto.Name;
            technology.UpdatedAt = DateTime.UtcNow;

            var updatedTechnology = await _technologyRepository.UpdateTechnologyAsync(technology);
            return MapToDto(updatedTechnology);
        }

        public async Task DeleteTechnologyAsync(string id)
        {
            await _technologyRepository.DeleteTechnologyAsync(id);
        }

        public async Task<TechnologyDto> ToggleTechnologyStatusAsync(string id)
        {
            var technology = await _technologyRepository.ToggleTechnologyStatusAsync(id);
            return MapToDto(technology);
        }

        // Helper method to map Technology entity to TechnologyDto
        private static TechnologyDto MapToDto(Technology technology)
        {
            return new TechnologyDto
            {
                Id = technology.Id,
                Name = technology.Name,
                Status = technology.Status,
                CreatedAt = technology.CreatedAt,
                UpdatedAt = technology.UpdatedAt
            };
        }
    }
}