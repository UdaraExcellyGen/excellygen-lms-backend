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


        private class KpiQueryResult
        {
            public string? Name { get; set; }
            public int Count { get; set; }
        }

        public AnalyticsService(IDbConnection dbConnection, ILogger<AnalyticsService> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

       
        public async Task<KpiSummaryDto> GetKpiSummaryAsync()
        {
            try
            {
                _logger.LogInformation("Getting KPI summary data.");

                string usersSql = "SELECT COUNT(*) FROM Users; SELECT COUNT(*) FROM Users WHERE Status = 'active';";
                string coursesSql = "SELECT COUNT(*) FROM Courses;";
                string enrollmentsSql = "SELECT COUNT(*) FROM Enrollments; SELECT COUNT(*) FROM Enrollments WHERE completion_date IS NOT NULL;";

                using var usersMulti = await _dbConnection.QueryMultipleAsync(usersSql);
                var totalUsers = await usersMulti.ReadFirstAsync<int>();
                var activeUsers = await usersMulti.ReadFirstAsync<int>();

                var totalCourses = await _dbConnection.QuerySingleAsync<int>(coursesSql);

                using var enrollmentsMulti = await _dbConnection.QueryMultipleAsync(enrollmentsSql);
                var totalEnrollments = await enrollmentsMulti.ReadFirstAsync<int>();
                var completedEnrollments = await enrollmentsMulti.ReadFirstAsync<int>();

                return new KpiSummaryDto
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    TotalCourses = totalCourses,
                    TotalEnrollments = totalEnrollments,
                    CompletedEnrollments = completedEnrollments
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting KPI summary data.");
                throw;
            }
        }

        public async Task<EnrollmentKpiDto> GetEnrollmentKpiAsync()
        {
            try
            {
                _logger.LogInformation("Getting enrollment KPI data using robust, isolated queries.");

                string mostEnrolledCategorySql = @"
                    SELECT TOP 1 cc.Title as Name, COUNT(1) as Count
                    FROM Enrollments e
                    INNER JOIN Courses c ON e.course_id = c.Id
                    INNER JOIN CourseCategories cc ON c.CategoryId = cc.Id
                    GROUP BY cc.Title
                    ORDER BY Count DESC;";

                string mostEnrolledCourseSql = @"
                    SELECT TOP 1 c.Title as Name, COUNT(1) as Count
                    FROM Enrollments e
                    INNER JOIN Courses c ON e.course_id = c.Id
                    GROUP BY c.Title
                    ORDER BY Count DESC;";

                string mostCompletedCourseSql = @"
                    SELECT TOP 1 c.Title as Name, COUNT(1) as Count
                    FROM Enrollments e
                    INNER JOIN Courses c ON e.course_id = c.Id
                    WHERE e.completion_date IS NOT NULL
                    GROUP BY c.Title
                    ORDER BY Count DESC;";

                var popularCategory = await _dbConnection.QueryFirstOrDefaultAsync<KpiQueryResult>(mostEnrolledCategorySql);
                var popularCourse = await _dbConnection.QueryFirstOrDefaultAsync<KpiQueryResult>(mostEnrolledCourseSql);
                var completedCourse = await _dbConnection.QueryFirstOrDefaultAsync<KpiQueryResult>(mostCompletedCourseSql);


                return new EnrollmentKpiDto
                {
                    MostPopularCategoryName = popularCategory?.Name,
                    MostPopularCategoryEnrollments = popularCategory?.Count ?? 0,
                    MostPopularCourseName = popularCourse?.Name,
                    MostPopularCourseEnrollments = popularCourse?.Count ?? 0,
                    MostCompletedCourseName = completedCourse?.Name,
                    MostCompletedCourseCount = completedCourse?.Count ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A critical error occurred while getting enrollment KPI data.");
                throw;
            }
        }

        
        public async Task<EnrollmentAnalyticsDto> GetEnrollmentAnalyticsAsync(string? categoryId = null)
        {
            try
            {
                string categoriesSql = @"
                    SELECT 
                        c.Id, c.Title, c.Description, c.Icon, c.Status, COUNT(co.Id) AS TotalCourses
                    FROM CourseCategories c
                    LEFT JOIN Courses co ON c.Id = co.CategoryId
                    WHERE c.Status = 'active'
                    GROUP BY c.Id, c.Title, c.Description, c.Icon, c.Status
                    ORDER BY c.Title";
                
                // Execute the categories query
                var categories = await _dbConnection.QueryAsync<CourseCategoryDto>(categoriesSql);

                
                IEnumerable<EnrollmentChartItemDto> enrollmentData;

                if (string.IsNullOrEmpty(categoryId))
                {
                    _logger.LogInformation("Getting category-level enrollment analytics for drill-down chart.");
                    string enrollmentSql = @"
                        WITH EnrollmentCounts AS (
                            SELECT
                                c.CategoryId,
                                COUNT(CASE WHEN e.completion_date IS NOT NULL THEN 1 END) AS Completed,
                                COUNT(CASE WHEN e.completion_date IS NULL THEN 1 END) AS InProgress
                            FROM Enrollments e
                            JOIN Courses c ON e.course_id = c.Id
                            GROUP BY c.CategoryId
                        )
                        SELECT
                            cc.Id,
                            cc.Title AS Name,
                            COALESCE(ec.Completed, 0) AS Completed,
                            COALESCE(ec.InProgress, 0) AS InProgress
                        FROM CourseCategories cc
                        LEFT JOIN EnrollmentCounts ec ON cc.Id = ec.CategoryId
                        WHERE cc.Status = 'active'
                        ORDER BY (COALESCE(ec.Completed, 0) + COALESCE(ec.InProgress, 0)) DESC, cc.Title;";
                    
                    // Execute the query with no parameters and assign the result
                    enrollmentData = await _dbConnection.QueryAsync<EnrollmentChartItemDto>(enrollmentSql);
                }
                else
                {
                     _logger.LogInformation("Getting course-level enrollment analytics for category: {CategoryId}", categoryId);
                    string enrollmentSql = @"
                        SELECT 
                            c.Id,
                            c.Title AS Name, 
                            SUM(CASE WHEN e.completion_date IS NULL THEN 1 ELSE 0 END) AS InProgress,
                            SUM(CASE WHEN e.completion_date IS NOT NULL THEN 1 ELSE 0 END) AS Completed
                        FROM Enrollments e
                        JOIN Courses c ON e.course_id = c.Id
                        WHERE c.CategoryId = @CategoryId
                        GROUP BY c.Id, c.Title
                        ORDER BY (SUM(CASE WHEN e.completion_date IS NULL THEN 1 ELSE 0 END) + SUM(CASE WHEN e.completion_date IS NOT NULL THEN 1 ELSE 0 END)) DESC";
                    
                    // Execute the query with parameters and assign the result
                    enrollmentData = await _dbConnection.QueryAsync<EnrollmentChartItemDto>(enrollmentSql, new { CategoryId = categoryId });
                }
                
                // Now, create the final DTO to return
                return new EnrollmentAnalyticsDto
                {
                    EnrollmentData = enrollmentData.ToList(),
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
                string sql = @"
                    SELECT cc.Title AS Name, COUNT(c.Id) AS Value 
                    FROM CourseCategories cc
                    LEFT JOIN Courses c ON cc.Id = c.CategoryId
                    WHERE cc.Status = 'active'
                    GROUP BY cc.Title
                    ORDER BY Value DESC";

                var availabilityData = await _dbConnection.QueryAsync<ChartDataDto>(sql);

                return new CourseAvailabilityDto
                {
                    AvailabilityData = availabilityData.ToList()
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
                string sql = @"
                    SELECT 
                        r.value AS Role,
                        COUNT(CASE WHEN u.Status = 'active' THEN 1 END) AS Active,
                        COUNT(CASE WHEN u.Status = 'inactive' THEN 1 END) AS Inactive
                    FROM 
                        Users u
                    CROSS APPLY 
                        OPENJSON(u.Roles) r
                    GROUP BY 
                        r.value
                    ORDER BY 
                        (COUNT(CASE WHEN u.Status = 'active' THEN 1 END) + COUNT(CASE WHEN u.Status = 'inactive' THEN 1 END)) DESC";

                var distributionData = await _dbConnection.QueryAsync<UserDistributionItemDto>(sql);

                return new UserDistributionDto
                {
                    DistributionData = distributionData.ToList()
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