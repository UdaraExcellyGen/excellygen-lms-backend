using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Application.Services.Learner
{
    public class UserTechnologyService : IUserTechnologyService
    {
        private readonly IUserTechnologyRepository _userTechnologyRepository;
        private readonly ILogger<UserTechnologyService> _logger;

        public UserTechnologyService(
            IUserTechnologyRepository userTechnologyRepository,
            ILogger<UserTechnologyService> logger)
        {
            _userTechnologyRepository = userTechnologyRepository;
            _logger = logger;
        }

        public async Task<List<UserTechnologyDto>> GetUserTechnologiesAsync(string userId)
        {
            var userTechnologies = await _userTechnologyRepository.GetUserTechnologiesAsync(userId);

            return userTechnologies.Select(ut => new UserTechnologyDto
            {
                Id = ut.Technology.Id,
                Name = ut.Technology.Name,
                AddedDate = ut.AddedDate
            }).ToList();
        }

        public async Task<List<TechnologyDto>> GetAvailableTechnologiesAsync(string userId)
        {
            var availableTechnologies = await _userTechnologyRepository.GetAvailableTechnologiesAsync(userId);

            return availableTechnologies.Select(t => new TechnologyDto
            {
                Id = t.Id,
                Name = t.Name,
                Status = t.Status
            }).ToList();
        }

        public async Task<UserTechnologyDto> AddUserTechnologyAsync(string userId, string technologyId)
        {
            _logger.LogInformation("Adding technology {TechnologyId} to user {UserId}", technologyId, userId);

            var userTechnology = await _userTechnologyRepository.AddUserTechnologyAsync(userId, technologyId);

            return new UserTechnologyDto
            {
                Id = userTechnology.Technology.Id,
                Name = userTechnology.Technology.Name,
                AddedDate = userTechnology.AddedDate
            };
        }

        public async Task RemoveUserTechnologyAsync(string userId, string technologyId)
        {
            _logger.LogInformation("Removing technology {TechnologyId} from user {UserId}", technologyId, userId);

            await _userTechnologyRepository.RemoveUserTechnologyAsync(userId, technologyId);
        }
    }
}