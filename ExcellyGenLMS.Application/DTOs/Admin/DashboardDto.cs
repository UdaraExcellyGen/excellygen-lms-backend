using System;
using System.Collections.Generic;

namespace ExcellyGenLMS.Application.DTOs.Admin
{
    public class DashboardStatsDto
    {
        public CategoryStatsDto CourseCategories { get; set; } = new();
        public UserStatsDto Users { get; set; } = new();
        public TechnologyStatsDto Technologies { get; set; } = new();
    }

    public class CategoryStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
    }

    public class UserStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
    }

    public class TechnologyStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
    }

    public class NotificationDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public bool IsNew { get; set; }
    }
}