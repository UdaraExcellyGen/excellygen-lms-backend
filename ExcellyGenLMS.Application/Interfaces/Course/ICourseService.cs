// ExcellyGenLMS.Application/Interfaces/Course/ICourseService.cs
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace ExcellyGenLMS.Application.Interfaces.Course
{
    public interface ICourseService
    {
        // --- Course Management ---
        Task<CourseDto?> GetCourseByIdAsync(int courseId);
        Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
        Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto, string creatorId);
        // ADDED: Method for coordinator updates
        Task<CourseDto> UpdateCourseAsync(int courseId, UpdateCourseCoordinatorDto dto, string userId); // Pass userId for potential auth checks
        Task DeleteCourseAsync(int courseId);
        Task PublishCourseAsync(int courseId);

        //Methods for soft deletion
        Task DeactivateCourseAsync(int courseId);
        Task ReactivateCourseAsync(int courseId);

        // --- Lesson (Subtopic) Management ---
        Task<LessonDto?> GetLessonByIdAsync(int lessonId);
        Task<LessonDto> AddLessonAsync(CreateLessonDto createLessonDto);
        Task<LessonDto> UpdateLessonAsync(int lessonId, UpdateLessonDto updateLessonDto);
        Task DeleteLessonAsync(int lessonId);

        // --- Document Management ---
        Task<CourseDocumentDto?> GetDocumentByIdAsync(int documentId);
        Task<CourseDocumentDto> UploadDocumentAsync(int lessonId, IFormFile file);
        Task DeleteDocumentAsync(int documentId);

        // --- Lookups ---
        Task<IEnumerable<CategoryDto>> GetCourseCategoriesAsync();
        Task<IEnumerable<TechnologyDto>> GetTechnologiesAsync();
    }
}