// C:\Users\ASUS\Desktop\quizz\excellygen-lms-backend\ExcellyGenLMS.Application\Services\Course\CourseService.cs
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Course;
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Enums;
using ExcellyGenLMS.Core.Interfaces.Infrastructure;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;

// Namespace: ExcellyGenLMS.Application.Services.Course
namespace ExcellyGenLMS.Application.Services.Course
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseDocumentRepository _documentRepository;
        private readonly ICourseCategoryRepository _categoryRepository;
        private readonly ITechnologyRepository _technologyRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<CourseService> _logger;

        public CourseService(
            ICourseRepository courseRepository,
            ILessonRepository lessonRepository,
            ICourseDocumentRepository documentRepository,
            ICourseCategoryRepository categoryRepository,
            ITechnologyRepository technologyRepository,
            IFileStorageService fileStorageService,
            ILogger<CourseService> logger)
        {
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _lessonRepository = lessonRepository ?? throw new ArgumentNullException(nameof(lessonRepository));
            _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _technologyRepository = technologyRepository ?? throw new ArgumentNullException(nameof(technologyRepository));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int courseId)
        {
            _logger.LogInformation("Attempting to retrieve course with ID: {CourseId}", courseId);
            var course = await _courseRepository.GetByIdWithDetailsAsync(courseId);
            if (course == null)
            {
                _logger.LogWarning("Course with ID {CourseId} not found.", courseId);
                return null;
            }
            _logger.LogInformation("Successfully retrieved course {CourseId}: {Title}", courseId, course.Title);
            return MapCourseToDto(course);
        }

        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
        {
            _logger.LogInformation("Retrieving all courses.");
            var courses = await _courseRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} courses.", courses.Count());
            return courses.Select(MapCourseToDto).ToList();
        }

        public async Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto, string creatorId)
        {
            _logger.LogInformation("Attempting course creation for title '{Title}' by user {CreatorId}", createCourseDto.Title, creatorId);

            var category = await _categoryRepository.GetCategoryByIdAsync(createCourseDto.CategoryId);
            if (category == null || category.Status != "active")
            {
                var errorMsg = $"Invalid or inactive CategoryId provided: {createCourseDto.CategoryId}";
                _logger.LogError(errorMsg);
                throw new ArgumentException(errorMsg);
            }

            var validTechnologies = new List<Technology>();
            if (createCourseDto.TechnologyIds == null || !createCourseDto.TechnologyIds.Any())
            {
                throw new ArgumentException("At least one TechnologyId must be provided.");
            }
            foreach (var techId in createCourseDto.TechnologyIds.Distinct())
            {
                var tech = await _technologyRepository.GetTechnologyByIdAsync(techId);
                if (tech != null && tech.Status == "active")
                {
                    validTechnologies.Add(tech);
                }
                else
                {
                    _logger.LogWarning("Invalid or inactive TechnologyId '{TechId}' provided during course creation, skipping.", techId);
                }
            }
            if (!validTechnologies.Any())
            {
                var errorMsg = "No valid/active TechnologyIds were provided for the course.";
                _logger.LogError(errorMsg);
                throw new ArgumentException(errorMsg);
            }
            _logger.LogDebug("Validated Category {CategoryId} and {TechCount} active Technologies for course '{Title}'", category.Id, validTechnologies.Count, createCourseDto.Title);

            string? thumbnailPath = null;
            if (createCourseDto.ThumbnailImage != null && createCourseDto.ThumbnailImage.Length > 0)
            {
                _logger.LogInformation("Processing thumbnail upload for course '{Title}'.", createCourseDto.Title);
                string containerName = "thumbnails";
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                double maxFileSizeMB = 2.0;

                try
                {
                    using (var stream = createCourseDto.ThumbnailImage.OpenReadStream())
                    {
                        thumbnailPath = await _fileStorageService.SaveFileAsync(
                            stream,
                            createCourseDto.ThumbnailImage.FileName,
                            createCourseDto.ThumbnailImage.ContentType,
                            containerName,
                            allowedExtensions,
                            maxFileSizeMB
                        );
                    }
                }
                catch (IOException ioEx)
                {
                    _logger.LogError(ioEx, "IO error saving thumbnail file '{FileName}' for course '{Title}'", createCourseDto.ThumbnailImage.FileName, createCourseDto.Title);
                    throw new InvalidOperationException("Error saving thumbnail file.", ioEx);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error saving thumbnail file '{FileName}' for course '{Title}'", createCourseDto.ThumbnailImage.FileName, createCourseDto.Title);
                    throw;
                }

                if (thumbnailPath == null)
                {
                    _logger.LogWarning("Thumbnail upload failed (SaveFileAsync returned null) for course '{Title}'. Continuing without thumbnail.", createCourseDto.Title);
                }
                else
                {
                    _logger.LogInformation("Thumbnail saved successfully for course '{Title}' at path: {Path}", createCourseDto.Title, thumbnailPath);
                }
            }
            else
            {
                _logger.LogInformation("No thumbnail image provided for course '{Title}'.", createCourseDto.Title);
            }

            var course = new Core.Entities.Course.Course
            {
                Title = createCourseDto.Title,
                Description = createCourseDto.Description,
                EstimatedTime = createCourseDto.EstimatedTime,
                CategoryId = createCourseDto.CategoryId,
                CreatorId = creatorId,
                ThumbnailImagePath = thumbnailPath,
                Status = CourseStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                CoursePoints = null
            };

            var createdCourse = await _courseRepository.AddAsync(course);
            _logger.LogInformation("Course entity created in DB: ID {CourseId}, Title '{Title}'.", createdCourse.Id, createdCourse.Title);

            foreach (var tech in validTechnologies) { try { await _courseRepository.AddTechnologyAsync(createdCourse.Id, tech.Id); } catch (Exception ex) { _logger.LogError(ex, "Failed link Tech {TechId} to Course {CourseId}", tech.Id, createdCourse.Id); } }
            _logger.LogInformation("Attempted linking of {Count} active technologies to Course {CourseId}.", validTechnologies.Count, createdCourse.Id);

            var resultCourse = await _courseRepository.GetByIdWithDetailsAsync(createdCourse.Id);
            if (resultCourse == null) { _logger.LogCritical("CRITICAL: Failed retrieve course {CourseId} after create.", createdCourse.Id); throw new InvalidOperationException($"Critical error: Could not retrieve course ID {createdCourse.Id} after creation."); }

            _logger.LogInformation("Course creation complete for ID {CourseId}.", createdCourse.Id);
            return MapCourseToDto(resultCourse);
        }

        public async Task<CourseDto> UpdateCourseAsync(int courseId, UpdateCourseCoordinatorDto dto, string userId)
        {
            _logger.LogInformation("Attempting to update course {CourseId} by user {UserId}", courseId, userId);

            var course = await _courseRepository.GetByIdWithDetailsAsync(courseId);
            if (course == null)
            {
                _logger.LogWarning("Update failed: Course {CourseId} not found.", courseId);
                throw new KeyNotFoundException($"Course with ID {courseId} not found.");
            }

            course.Title = dto.Title;
            course.Description = dto.Description;
            course.EstimatedTime = dto.EstimatedTime;

            if (course.CategoryId != dto.CategoryId)
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(dto.CategoryId);
                if (category == null || category.Status != "active")
                {
                    _logger.LogError("Update failed: Invalid or inactive CategoryId {CategoryId} provided for course {CourseId}.", dto.CategoryId, courseId);
                    throw new ArgumentException($"Invalid or inactive CategoryId: {dto.CategoryId}");
                }
                course.CategoryId = dto.CategoryId;
            }

            await UpdateCourseTechnologies(course, dto.TechnologyIds);

            if (dto.ThumbnailImage != null && dto.ThumbnailImage.Length > 0)
            {
                _logger.LogInformation("Processing updated thumbnail upload for course {CourseId}.", courseId);
                if (!string.IsNullOrWhiteSpace(course.ThumbnailImagePath))
                {
                    await TryDeleteFileAsync(course.ThumbnailImagePath, $"old thumb C:{courseId}");
                    course.ThumbnailImagePath = null;
                }

                string containerName = "thumbnails";
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                double maxFileSizeMB = 2.0;
                string? newThumbnailPath = null;

                try
                {
                    using (var stream = dto.ThumbnailImage.OpenReadStream())
                    {
                        newThumbnailPath = await _fileStorageService.SaveFileAsync(
                            stream,
                            dto.ThumbnailImage.FileName,
                            dto.ThumbnailImage.ContentType,
                            containerName,
                            allowedExtensions,
                            maxFileSizeMB
                        );
                    }
                }
                catch (IOException ioEx) { _logger.LogError(ioEx, "IO error saving updated thumbnail file for course {CourseId}", courseId); throw new InvalidOperationException("Error saving updated thumbnail file.", ioEx); }
                catch (Exception ex) { _logger.LogError(ex, "Unexpected error saving updated thumbnail file for course {CourseId}", courseId); throw; }

                if (newThumbnailPath == null)
                {
                    _logger.LogWarning("Failed to save updated thumbnail for course {CourseId}. Update proceeding without new thumbnail.", courseId);
                }
                else
                {
                    course.ThumbnailImagePath = newThumbnailPath;
                    _logger.LogInformation("Updated thumbnail saved successfully for course {CourseId} at path: {Path}", courseId, newThumbnailPath);
                }
            }
            else
            {
                _logger.LogInformation("No new thumbnail provided during update for course {CourseId}", courseId);
            }

            course.LastUpdatedDate = DateTime.UtcNow;
            await _courseRepository.UpdateAsync(course);
            _logger.LogInformation("Course {CourseId} updated successfully.", courseId);

            var updatedCourse = await _courseRepository.GetByIdWithDetailsAsync(courseId);
            if (updatedCourse == null) throw new InvalidOperationException($"Course {courseId} not found after update.");
            return MapCourseToDto(updatedCourse);
        }

        public async Task PublishCourseAsync(int courseId)
        {
            _logger.LogInformation("Attempting to publish course {CourseId}", courseId);
            var course = await _courseRepository.GetByIdWithDetailsAsync(courseId);

            if (course == null)
            {
                throw new KeyNotFoundException($"Course {courseId} not found.");
            }

            if (course.Status == CourseStatus.Published)
            {
                _logger.LogInformation("Course {CourseId} already published.", courseId);
                return;
            }

            if (course.Status != CourseStatus.Draft)
            {
                throw new InvalidOperationException($"Cannot publish course from status '{course.Status}'.");
            }

            if (course.Lessons == null || !course.Lessons.Any())
            {
                throw new InvalidOperationException("Cannot publish course with no lessons.");
            }

            int totalPoints = course.Lessons.Sum(l => l.LessonPoints);
            int lessonCount = course.Lessons.Count;
            int averagePoints = lessonCount > 0 ? (int)Math.Round((double)totalPoints / lessonCount) : 0;

            _logger.LogInformation("Calculating course points for {CourseId}: Total={TotalPoints}, Count={LessonCount}, Average={AveragePoints}",
                courseId, totalPoints, lessonCount, averagePoints);

            course.CoursePoints = averagePoints;
            course.Status = CourseStatus.Published;
            course.LastUpdatedDate = DateTime.UtcNow;

            await _courseRepository.UpdateAsync(course);
            _logger.LogInformation("Course {CourseId} published with {Points} points.", courseId, averagePoints);
        }

        //public async Task DeactivateCourseAsync(int courseId)
        //{
        //    _logger.LogInformation("Attempting to deactivate course {CourseId}", courseId);
        //    var course = await _courseRepository.GetByIdAsync(courseId);
        //    if (course == null)
        //    {
        //        throw new KeyNotFoundException($"Course {courseId} not found.");
        //    }
        //    if (course.Status != CourseStatus.Published)
        //    {
        //        throw new InvalidOperationException("Only published courses can be deactivated.");
        //    }

        //    course.IsInactive = true;
        //    course.LastUpdatedDate = DateTime.UtcNow;

        //    await _courseRepository.UpdateAsync(course);
        //    _logger.LogInformation("Course {CourseId} deactivated successfully.", courseId);
        //}
        public async Task DeactivateCourseAsync(int courseId)
        {
            _logger.LogInformation("Attempting to deactivate course {CourseId}", courseId);
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException($"Course {courseId} not found.");
            }

            // ▼▼▼ THIS IS THE CRITICAL CHECK ▼▼▼
            if (course.Status != CourseStatus.Published)
            {
                // The course status is 'Draft', so this condition is TRUE.
                // An exception is thrown immediately.
                throw new InvalidOperationException("Only published courses can be deactivated.");
            }
            // ▲▲▲ The code never reaches this point for a Draft course. ▲▲▲

            course.IsInactive = true;
            course.LastUpdatedDate = DateTime.UtcNow;

            await _courseRepository.UpdateAsync(course);
            _logger.LogInformation("Course {CourseId} deactivated successfully.", courseId);
        }

        public async Task ReactivateCourseAsync(int courseId)
        {
            _logger.LogInformation("Attempting to reactivate course {CourseId}", courseId);
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException($"Course {courseId} not found.");
            }

            course.IsInactive = false;
            course.LastUpdatedDate = DateTime.UtcNow;

            await _courseRepository.UpdateAsync(course);
            _logger.LogInformation("Course {CourseId} reactivated successfully.", courseId);
        }

        public async Task DeleteCourseAsync(int courseId)
        {
            _logger.LogInformation("Attempting to delete course {CourseId}", courseId);
            var course = await _courseRepository.GetByIdWithDetailsAsync(courseId);
            if (course == null) { throw new KeyNotFoundException($"Course {courseId} not found."); }

            if (!string.IsNullOrWhiteSpace(course.ThumbnailImagePath)) { await TryDeleteFileAsync(course.ThumbnailImagePath, $"thumb C:{courseId}"); }
            if (course.Lessons != null)
            {
                foreach (var lesson in course.Lessons.ToList())
                {
                    if (lesson.Documents != null)
                    {
                        foreach (var doc in lesson.Documents.ToList())
                        {
                            if (!string.IsNullOrWhiteSpace(doc.FilePath)) { await TryDeleteFileAsync(doc.FilePath, $"doc {doc.Id} L:{lesson.Id}"); }
                        }
                    }
                }
            }
            await _courseRepository.DeleteAsync(courseId);
            _logger.LogInformation("Course {CourseId} deleted.", courseId);
        }

        public async Task<LessonDto?> GetLessonByIdAsync(int lessonId)
        {
            _logger.LogInformation("Retrieving lesson {LessonId}", lessonId);
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            return lesson == null ? null : MapLessonToDto(lesson);
        }

        public async Task<LessonDto> AddLessonAsync(CreateLessonDto createLessonDto)
        {
            _logger.LogInformation("Adding lesson '{LessonName}' to course {CourseId}", createLessonDto.LessonName, createLessonDto.CourseId);
            var course = await _courseRepository.GetByIdAsync(createLessonDto.CourseId);
            if (course == null) { throw new ArgumentException($"Cannot add lesson: Course {createLessonDto.CourseId} not found."); }
            var lesson = new Lesson { CourseId = createLessonDto.CourseId, LessonName = createLessonDto.LessonName, LessonPoints = createLessonDto.LessonPoints, LastUpdatedDate = DateTime.UtcNow, Documents = new List<CourseDocument>() };
            var createdLesson = await _lessonRepository.AddAsync(lesson);
            course.LastUpdatedDate = DateTime.UtcNow; await _courseRepository.UpdateAsync(course);
            return MapLessonToDto(createdLesson);
        }

        public async Task<LessonDto> UpdateLessonAsync(int lessonId, UpdateLessonDto updateLessonDto)
        {
            _logger.LogInformation("Updating lesson {LessonId}", lessonId);
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null) { throw new KeyNotFoundException($"Lesson {lessonId} not found."); }
            lesson.LessonName = updateLessonDto.LessonName; lesson.LessonPoints = updateLessonDto.LessonPoints; lesson.LastUpdatedDate = DateTime.UtcNow;
            await _lessonRepository.UpdateAsync(lesson);
            var course = await _courseRepository.GetByIdAsync(lesson.CourseId);
            if (course != null) { course.LastUpdatedDate = DateTime.UtcNow; await _courseRepository.UpdateAsync(course); }
            var updatedLesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (updatedLesson == null) throw new InvalidOperationException($"Lesson {lessonId} missing after update.");
            return MapLessonToDto(updatedLesson);
        }

        public async Task DeleteLessonAsync(int lessonId)
        {
            _logger.LogInformation("Attempting to delete lesson {LessonId}", lessonId);
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null) { throw new KeyNotFoundException($"Lesson {lessonId} not found."); }
            int courseId = lesson.CourseId;
            if (lesson.Documents != null) { foreach (var doc in lesson.Documents.ToList()) { if (!string.IsNullOrWhiteSpace(doc.FilePath)) { await TryDeleteFileAsync(doc.FilePath, $"doc {doc.Id}"); } } }
            await _lessonRepository.DeleteAsync(lessonId);
            _logger.LogInformation("Lesson {LessonId} deleted.", lessonId);
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course != null) { course.LastUpdatedDate = DateTime.UtcNow; await _courseRepository.UpdateAsync(course); }
        }

        public async Task<CourseDocumentDto?> GetDocumentByIdAsync(int documentId)
        {
            _logger.LogInformation("Retrieving document {DocumentId}", documentId);
            var document = await _documentRepository.GetByIdAsync(documentId);
            return document == null ? null : MapDocumentToDto(document);
        }

        public async Task<CourseDocumentDto> UploadDocumentAsync(int lessonId, IFormFile file)
        {
            _logger.LogInformation("Attempting upload document '{FileName}' for lesson {LessonId}", file?.FileName, lessonId);
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null) { throw new ArgumentException($"Cannot upload: Lesson {lessonId} not found."); }
            if (file == null || file.Length == 0) { throw new ArgumentException("File required for upload."); }

            var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            DocumentType docType; string[] allowedExtensions; const double maxFileSizeMB = 5.0;
            switch (fileExtension)
            {
                case ".pdf": docType = DocumentType.PDF; allowedExtensions = new[] { ".pdf" }; break;
                case ".doc": case ".docx": docType = DocumentType.Word; allowedExtensions = new[] { ".doc", ".docx" }; break;
                default: throw new ArgumentException($"Unsupported type '{fileExtension}'. Allowed: PDF, DOC, DOCX.");
            }

            string container = $"courses/{lesson.CourseId}/lessons/{lesson.Id}/documents";
            string? filePath = null;

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    filePath = await _fileStorageService.SaveFileAsync(
                        stream,
                        file.FileName,
                        file.ContentType,
                        container,
                        allowedExtensions,
                        maxFileSizeMB
                    );
                }
            }
            catch (IOException ioEx) { _logger.LogError(ioEx, "IO error saving document file '{FileName}' for lesson {LessonId}", file.FileName, lessonId); throw new InvalidOperationException("Error saving document file.", ioEx); }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error saving document file '{FileName}' for lesson {LessonId}", file.FileName, lessonId); throw; }

            if (filePath == null)
            {
                throw new InvalidOperationException($"File storage service failed to save '{file.FileName}'.");
            }
            _logger.LogInformation("Document '{FileName}' saved at '{FilePath}'", file.FileName, filePath);

            var document = new CourseDocument
            {
                LessonId = lessonId,
                Name = Path.GetFileName(file.FileName),
                DocumentType = docType,
                FilePath = filePath,
                FileSize = file.Length,
                LastUpdatedDate = DateTime.UtcNow
            };

            var createdDocument = await _documentRepository.AddAsync(document);
            _logger.LogInformation("Document record {DocumentId} saved to DB.", createdDocument.Id);

            lesson.LastUpdatedDate = DateTime.UtcNow; await _lessonRepository.UpdateAsync(lesson);
            var course = await _courseRepository.GetByIdAsync(lesson.CourseId);
            if (course != null) { course.LastUpdatedDate = DateTime.UtcNow; await _courseRepository.UpdateAsync(course); }

            return MapDocumentToDto(createdDocument);
        }

        public async Task DeleteDocumentAsync(int documentId)
        {
            _logger.LogInformation("Attempting to delete document {DocumentId}", documentId);
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null) { throw new KeyNotFoundException($"Doc {documentId} not found."); }
            int lessonId = document.LessonId;
            if (!string.IsNullOrWhiteSpace(document.FilePath)) { await TryDeleteFileAsync(document.FilePath, $"doc {document.Id}"); }
            await _documentRepository.DeleteAsync(documentId);
            _logger.LogInformation("Doc record {DocumentId} deleted.", documentId);
            // FIX: Changed __lessonRepository to _lessonRepository
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson != null) { lesson.LastUpdatedDate = DateTime.UtcNow; await _lessonRepository.UpdateAsync(lesson); var course = await _courseRepository.GetByIdAsync(lesson.CourseId); if (course != null) { course.LastUpdatedDate = DateTime.UtcNow; await _courseRepository.UpdateAsync(course); } }
        }

        public async Task<IEnumerable<CategoryDto>> GetCourseCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            return categories
                .Where(c => c.Status == "active")
                .Select(c => new CategoryDto { Id = c.Id, Title = c.Title });
        }

        public async Task<IEnumerable<TechnologyDto>> GetTechnologiesAsync()
        {
            var technologies = await _technologyRepository.GetAllTechnologiesAsync();
            return technologies
                .Where(t => t.Status == "active")
                 .Select(t => new TechnologyDto { Id = t.Id, Name = t.Name });
        }

        private async Task UpdateCourseTechnologies(Core.Entities.Course.Course course, List<string> desiredTechIds)
        {
            _logger.LogDebug("Updating technologies for course {CourseId}. Desired count: {Count}", course.Id, desiredTechIds?.Count ?? 0);
            var validDesiredTechIds = new List<string>();

            if (desiredTechIds != null && desiredTechIds.Any())
            {
                foreach (var techId in desiredTechIds.Distinct())
                {
                    var tech = await _technologyRepository.GetTechnologyByIdAsync(techId);
                    if (tech != null && tech.Status == "active")
                    {
                        validDesiredTechIds.Add(techId);
                    }
                    else { _logger.LogWarning("Invalid/inactive TechId '{TechId}' during update for course {CourseId}, skipping.", techId, course.Id); }
                }
                if (!validDesiredTechIds.Any())
                {
                    throw new ArgumentException("No valid/active TechnologyIds were provided for the course update.");
                }
            }
            else
            {
                _logger.LogWarning("No TechnologyIds provided for update on course {CourseId}. Clearing all existing.", course.Id);
            }

            await _courseRepository.ClearTechnologiesAsync(course.Id);
            _logger.LogDebug("Cleared technology links for course {CourseId}.", course.Id);

            foreach (var techIdToAdd in validDesiredTechIds)
            {
                try { await _courseRepository.AddTechnologyAsync(course.Id, techIdToAdd); }
                catch (Exception ex) { _logger.LogError(ex, "Failed to add technology link ({TechId}) during update for course {CourseId}", techIdToAdd, course.Id); }
            }
            _logger.LogInformation("Set {Count} technology links for course {CourseId}.", validDesiredTechIds.Count, course.Id);
        }

        private async Task TryDeleteFileAsync(string filePath, string fileDescription)
        {
            _logger.LogDebug("Attempting deletion file ({Description}): '{Path}'", fileDescription, filePath);
            if (string.IsNullOrWhiteSpace(filePath)) { _logger.LogWarning("Skipped deletion ({Description}): Path null/empty.", fileDescription); return; }
            try { bool deleted = await _fileStorageService.DeleteFileAsync(filePath); if (!deleted) _logger.LogWarning("Storage reported fail delete file ({Desc}) '{Path}'. May not exist.", fileDescription, filePath); else _logger.LogInformation("Deleted file ({Desc}) '{Path}'.", fileDescription, filePath); }
            catch (Exception ex) { _logger.LogError(ex, "Error deleting file ({Desc}) '{Path}'.", fileDescription, filePath); }
        }

        private CourseDto MapCourseToDto(Core.Entities.Course.Course course)
        {
            if (course == null) throw new ArgumentNullException(nameof(course));
            var categoryDto = course.Category != null ? new CategoryDto { Id = course.Category.Id, Title = course.Category.Title } : new CategoryDto { Id = "", Title = "N/A" };
            var creatorDto = course.Creator != null ? new UserBasicDto { Id = course.Creator.Id, Name = course.Creator.Name } : new UserBasicDto { Id = "", Name = "N/A" };
            var techDtos = course.CourseTechnologies?.Where(ct => ct.Technology != null).Select(ct => new TechnologyDto { Id = ct.Technology!.Id, Name = ct.Technology!.Name }).OrderBy(t => t.Name).ToList() ?? new List<TechnologyDto>();
            var lessonDtos = course.Lessons?.Select(MapLessonToDto).OrderBy(l => l.Id).ToList() ?? new List<LessonDto>();
            int calculatedPoints = course.CoursePoints ?? (course.Lessons?.Sum(l => l.LessonPoints) ?? 0);
            return new CourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                CalculatedCoursePoints = calculatedPoints,
                EstimatedTime = course.EstimatedTime,
                CreatedAt = course.CreatedAt,
                LastUpdatedDate = course.LastUpdatedDate,
                Status = course.Status,
                IsInactive = course.IsInactive,
                ThumbnailUrl = !string.IsNullOrWhiteSpace(course.ThumbnailImagePath) ? _fileStorageService.GetFileUrl(course.ThumbnailImagePath) : null,
                Category = categoryDto,
                Creator = creatorDto,
                Technologies = techDtos,
                Lessons = lessonDtos
            };
        }

        private LessonDto MapLessonToDto(Lesson lesson)
        {
            if (lesson == null) throw new ArgumentNullException(nameof(lesson));
            var docDtos = lesson.Documents?.Select(MapDocumentToDto).OrderBy(d => d.Name).ToList() ?? new List<CourseDocumentDto>();
            return new LessonDto
            {
                Id = lesson.Id,
                LessonName = lesson.LessonName,
                LessonPoints = lesson.LessonPoints,
                LastUpdatedDate = lesson.LastUpdatedDate,
                CourseId = lesson.CourseId,
                Documents = docDtos
            };
        }

        private CourseDocumentDto MapDocumentToDto(CourseDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            return new CourseDocumentDto
            {
                Id = document.Id,
                Name = document.Name,
                DocumentType = document.DocumentType,
                FileSize = document.FileSize,
                FileUrl = !string.IsNullOrWhiteSpace(document.FilePath) ? _fileStorageService.GetFileUrl(document.FilePath) : "",
                LastUpdatedDate = document.LastUpdatedDate,
                LessonId = document.LessonId
            };
        }
    }
}