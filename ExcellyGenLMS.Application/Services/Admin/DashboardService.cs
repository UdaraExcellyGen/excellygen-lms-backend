using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.Interfaces.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Admin
{
    public class DashboardService : IDashboardService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICourseCategoryRepository _categoryRepository;
        private readonly ITechnologyRepository _technologyRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IUserRepository userRepository,
            ICourseCategoryRepository categoryRepository,
            ITechnologyRepository technologyRepository,
            ILogger<DashboardService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _technologyRepository = technologyRepository ?? throw new ArgumentNullException(nameof(technologyRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching dashboard statistics");

                // Get all users and count active ones
                var users = await _userRepository.GetAllUsersAsync();
                var activeUsers = users.Count(u => u.Status == "active");

                // Get all categories and count active ones
                var categories = await _categoryRepository.GetAllCategoriesAsync();
                var activeCategories = categories.Count(c => c.Status == "active");

                // Get all technologies and count active ones
                var technologies = await _technologyRepository.GetAllTechnologiesAsync();
                var activeTechnologies = technologies.Count(t => t.Status == "active");

                return new DashboardStatsDto
                {
                    CourseCategories = new CategoryStatsDto
                    {
                        Total = categories.Count,
                        Active = activeCategories
                    },
                    Users = new UserStatsDto
                    {
                        Total = users.Count,
                        Active = activeUsers
                    },
                    Technologies = new TechnologyStatsDto
                    {
                        Total = technologies.Count,
                        Active = activeTechnologies
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                throw;
            }
        }

        public Task<List<NotificationDto>> GetDashboardNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching dashboard notifications");

                // In a real application, this would come from a notification repository
                // For now, return some sample notifications wrapped in a completed task
                // This eliminates the async/await warning
                var notifications = new List<NotificationDto>
                {
                    new NotificationDto
                    {
                        Id = 1,
                        Text = "New user registered: Sarah Johnson",
                        Time = "2 hours ago",
                        IsNew = true
                    },
                    new NotificationDto
                    {
                        Id = 2,
                        Text = "Course category 'Web Development' has been updated",
                        Time = "5 hours ago",
                        IsNew = true
                    },
                    new NotificationDto
                    {
                        Id = 3,
                        Text = "System maintenance scheduled for tomorrow",
                        Time = "1 day ago",
                        IsNew = false
                    }
                };

                // Return the list wrapped in a completed task
                return Task.FromResult(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard notifications");
                throw;
            }
        }
    }
}