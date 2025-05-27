// ExcellyGenLMS.Application/Services/Admin/AnalyticsService.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.Interfaces.Admin;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Application.Services.Admin
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(IDbConnection dbConnection, ILogger<AnalyticsService> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync(string? categoryId = null)
        {
            var enrollmentAnalytics = await GetEnrollmentAnalyticsAsync(categoryId);
            var courseAvailability = await GetCourseAvailabilityAnalyticsAsync();
            var userDistribution = await GetUserDistributionAnalyticsAsync();

            return new DashboardAnalyticsDto
            {
                EnrollmentAnalytics = enrollmentAnalytics,
                CourseAvailability = courseAvailability,
                UserDistribution = userDistribution
            };
        }

        public async Task<EnrollmentAnalyticsDto> GetEnrollmentAnalyticsAsync(string? categoryId = null)
        {
            try
            {
                _logger.LogInformation("Getting enrollment analytics data for category: {CategoryId}", categoryId ?? "all");

                // Get all categories for dropdown with course counts
                string categoriesSql = @"
                    SELECT 
                        c.Id, 
                        c.Title, 
                        c.Description, 
                        c.Icon, 
                        c.Status,
                        COUNT(co.Id) AS TotalCourses
                    FROM 
                        CourseCategories c
                    LEFT JOIN 
                        Courses co ON c.Id = co.CategoryId
                    WHERE 
                        c.Status = 'active'
                    GROUP BY 
                        c.Id, c.Title, c.Description, c.Icon, c.Status";

                var categories = await _dbConnection.QueryAsync<CourseCategoryDto>(categoriesSql);

                // If no category provided, return empty enrollment data but with categories
                if (string.IsNullOrEmpty(categoryId))
                {
                    return new EnrollmentAnalyticsDto
                    {
                        EnrollmentData = new List<ChartDataDto>(),
                        Categories = categories.ToList()
                    };
                }

                // Get enrollment data for the selected category
                // Using courseName and enrollmentCount properties to match frontend expectations
                string enrollmentSql = @"
                    SELECT c.Title AS CourseName, COUNT(e.enrollment_id) AS EnrollmentCount 
                    FROM Enrollments e
                    JOIN Courses c ON e.course_id = c.Id
                    WHERE c.CategoryId = @CategoryId
                    GROUP BY c.Title
                    ORDER BY EnrollmentCount DESC";

                // This query will return records with CourseName and EnrollmentCount properties
                var enrollmentData = await _dbConnection.QueryAsync(enrollmentSql, new { CategoryId = categoryId });

                // Convert to ChartDataDto which uses Name and Value properties
                var chartData = enrollmentData.Select(item => new ChartDataDto
                {
                    Name = item.CourseName,
                    Value = item.EnrollmentCount
                }).ToList();

                return new EnrollmentAnalyticsDto
                {
                    EnrollmentData = chartData,
                    Categories = categories.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enrollment analytics data");
                throw;
            }
        }

        public async Task<CourseAvailabilityDto> GetCourseAvailabilityAnalyticsAsync()
        {
            try
            {
                _logger.LogInformation("Getting course availability analytics data");

                // Get course counts by category
                string sql = @"
                    SELECT cc.Title AS Name, COUNT(c.Id) AS Value 
                    FROM CourseCategories cc
                    LEFT JOIN Courses c ON cc.Id = c.CategoryId
                    WHERE cc.Status = 'active'
                    GROUP BY cc.Title
                    ORDER BY Value DESC";

                var availabilityData = await _dbConnection.QueryAsync<ChartDataDto>(sql);

                // Assign colors to make chart visually appealing
                string[] colors = { "#8884d8", "#83a6ed", "#8dd1e1", "#82ca9d", "#a4de6c", "#d0ed57" };
                int colorIndex = 0;

                var availabilityWithColors = availabilityData.Select(item => {
                    item.Color = colors[colorIndex++ % colors.Length];
                    return item;
                }).ToList();

                return new CourseAvailabilityDto
                {
                    AvailabilityData = availabilityWithColors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course availability analytics data");
                throw;
            }
        }

        public async Task<UserDistributionDto> GetUserDistributionAnalyticsAsync()
        {
            try
            {
                _logger.LogInformation("Getting user distribution analytics data");

                // Count unique users by role - SQL Server specific JSON handling
                string sql = @"
                    SELECT 
                        r.value AS Name,
                        COUNT(DISTINCT u.Id) AS Value
                    FROM 
                        Users u
                    CROSS APPLY 
                        OPENJSON(u.Roles) r
                    GROUP BY 
                        r.value
                    ORDER BY 
                        Value DESC";

                var distributionData = await _dbConnection.QueryAsync<ChartDataDto>(sql);

                // Assign colors for the pie chart (match your frontend theme)
                string[] roleColors = {
                    "#52007C", // Admin - Purple
                    "#BF4BF6", // CourseCoordinator - Light Purple
                    "#D68BF9", // Learner - Lighter Purple
                    "#E6E6FA"  // ProjectManager - Lightest Purple
                };

                var rolesWithColors = distributionData.Select((item, index) => {
                    item.Color = roleColors[index % roleColors.Length];
                    return item;
                }).ToList();

                return new UserDistributionDto
                {
                    DistributionData = rolesWithColors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user distribution analytics data");
                throw;
            }
        }
    }
}