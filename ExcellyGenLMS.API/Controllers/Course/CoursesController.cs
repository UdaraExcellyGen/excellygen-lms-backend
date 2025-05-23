// ExcellyGenLMS.API/Controllers/Course/CoursesController.cs
using ExcellyGenLMS.Application.DTOs.Course;      // DTOs for Course, Lesson, Document, etc.
using ExcellyGenLMS.Application.Interfaces.Course; // ICourseService interface
using Microsoft.AspNetCore.Authorization;         // For [Authorize] attribute
using Microsoft.AspNetCore.Http;                // For IFormFile and StatusCodes
using Microsoft.AspNetCore.Mvc;                 // For ControllerBase, action results, attributes
using Microsoft.Extensions.Logging;             // For logging
using System;                                   // For Exception, ArgumentException, etc.
using System.Collections.Generic;               // For IEnumerable, List
using System.Security.Claims;                   // For retrieving user ID from JWT claims
using System.Threading.Tasks;                   // For Task, async/await


namespace ExcellyGenLMS.API.Controllers.Course
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "CourseCoordinator")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(ICourseService courseService, ILogger<CoursesController> logger)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // --- GetCurrentUserId Helper ---
        private string GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
            {
                var errorMsg = "User ID claim ('NameIdentifier' or 'sub') not found in token.";
                _logger.LogError(errorMsg);
                throw new UnauthorizedAccessException(errorMsg);
            }
            return userId;
        }


        // ==============================
        //      COURSE ENDPOINTS
        // ==============================

        // --- POST /api/courses ---
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CourseDto>> CreateCourse([FromForm] CreateCourseDto createCourseDto)
        {
            // (Implementation remains the same as before)
            _logger.LogInformation("Received request to create course: {Title}", createCourseDto.Title);
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                string creatorId = GetCurrentUserId();
                _logger.LogInformation("Creating course '{Title}' for user {CreatorId}", createCourseDto.Title, creatorId);
                var createdCourse = await _courseService.CreateCourseAsync(createCourseDto, creatorId);
                _logger.LogInformation("Course {CourseId} created successfully: {Title}", createdCourse.Id, createdCourse.Title);
                return CreatedAtAction(nameof(GetCourseById), new { courseId = createdCourse.Id }, createdCourse);
            }
            // ... (keep existing catch blocks) ...
            catch (ArgumentException ex) { _logger.LogWarning(ex, "Bad request during course creation: {ErrorMessage}", ex.Message); return BadRequest(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { _logger.LogError(ex, "Authorization error during course creation: {ErrorMessage}", ex.Message); return Unauthorized(new { message = "Unable to authorize user from token." }); }
            catch (InvalidOperationException ex) { _logger.LogError(ex, "Operation failed during course creation: {ErrorMessage}", ex.Message); return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An operation failed: {ex.Message}" }); }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error creating course: {Title}", createCourseDto.Title); return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while creating the course. Please try again later." }); }

        }

        // --- PUT /api/courses/{courseId} --- (NEW ENDPOINT)
        [HttpPut("{courseId}")]
        [Consumes("multipart/form-data")] // Required because UpdateCourseCoordinatorDto contains IFormFile
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CourseDto>> UpdateCourse(int courseId, [FromForm] UpdateCourseCoordinatorDto updateDto)
        {
            _logger.LogInformation("Received request to update course: {CourseId}", courseId);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Update course request for {CourseId} failed model validation.", courseId);
                return BadRequest(ModelState);
            }

            try
            {
                string userId = GetCurrentUserId(); // Get user performing the update for logging/auth check in service
                var updatedCourse = await _courseService.UpdateCourseAsync(courseId, updateDto, userId);
                _logger.LogInformation("Course {CourseId} updated successfully.", courseId);
                return Ok(updatedCourse);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Update failed: Course not found: ID {CourseId}", courseId);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex) // Handles invalid CategoryId/TechnologyId from service
            {
                _logger.LogWarning(ex, "Bad request updating course {CourseId}: {ErrorMessage}", courseId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex) // E.g., If service checks ownership
            {
                _logger.LogError(ex, "Authorization error updating course {CourseId}: {ErrorMessage}", courseId, ex.Message);
                return Forbid(); // Return 403 Forbidden if user is authenticated but not allowed
            }
            catch (InvalidOperationException ex) // E.g., file save failure
            {
                _logger.LogError(ex, "Operation failed updating course {CourseId}: {ErrorMessage}", courseId, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Could not complete the update: {ex.Message}" });
            }
            catch (Exception ex) // Catch-all
            {
                _logger.LogError(ex, "Unexpected error updating course {CourseId}", courseId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while updating the course." });
            }
        }

        // --- GET /api/courses/{courseId} ---
        [HttpGet("{courseId}")]
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CourseDto>> GetCourseById(int courseId)
        {
            // (Implementation remains the same)
            _logger.LogInformation("Received request to get course by ID: {CourseId}", courseId);
            try
            {
                var course = await _courseService.GetCourseByIdAsync(courseId);
                if (course == null) { _logger.LogWarning("Course not found: ID {CourseId}", courseId); return NotFound(new { message = $"Course with ID {courseId} not found." }); }
                _logger.LogInformation("Successfully retrieved course {CourseId}: {Title}", courseId, course.Title);
                return Ok(course);
            }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error retrieving course {CourseId}", courseId); return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while retrieving the course." }); }
        }

        // --- GET /api/courses ---
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CourseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetAllCourses()
        {
            // (Implementation remains the same)
            _logger.LogInformation("Received request to get all courses.");
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                _logger.LogInformation("Retrieved {Count} courses.", courses.Count());
                return Ok(courses);
            }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error retrieving all courses."); return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while retrieving courses." }); }
        }


        // --- PATCH /api/courses/{courseId}/publish ---
        [HttpPatch("{courseId}/publish")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PublishCourse(int courseId)
        {
            // (Implementation remains the same)
            _logger.LogInformation("Received request to publish course: {CourseId}", courseId);
            try { await _courseService.PublishCourseAsync(courseId); _logger.LogInformation("Course {CourseId} published successfully.", courseId); return NoContent(); }
            catch (KeyNotFoundException ex) { _logger.LogWarning(ex, "Publish failed: Course not found: ID {CourseId}", courseId); return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { _logger.LogWarning(ex, "Publish failed: Business rule violation for course {CourseId}: {ErrorMessage}", courseId, ex.Message); return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error publishing course {CourseId}", courseId); return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while publishing the course." }); }
        }


        // --- DELETE /api/courses/{courseId} ---
        [HttpDelete("{courseId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            // (Implementation remains the same)
            _logger.LogInformation("Received request to delete course: {CourseId}", courseId);
            try { await _courseService.DeleteCourseAsync(courseId); _logger.LogInformation("Course {CourseId} deleted successfully.", courseId); return NoContent(); }
            catch (KeyNotFoundException ex) { _logger.LogWarning(ex, "Delete failed: Course not found: ID {CourseId}", courseId); return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error deleting course {CourseId}", courseId); return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while deleting the course." }); }
        }


        // ==============================
        //      LESSON ENDPOINTS (Remain the same)
        // ==============================
        [HttpPost("lessons")]
        [ProducesResponseType(typeof(LessonDto), StatusCodes.Status201Created)] /* ... */
        public async Task<ActionResult<LessonDto>> AddLesson([FromBody] CreateLessonDto createLessonDto)
        {
            // (Implementation remains the same)
            _logger.LogInformation("Received request to add lesson '{LessonName}' to course {CourseId}", createLessonDto.LessonName, createLessonDto.CourseId);
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try { var createdLesson = await _courseService.AddLessonAsync(createLessonDto); _logger.LogInformation("Lesson {LessonId} added successfully to course {CourseId}.", createdLesson.Id, createLessonDto.CourseId); return StatusCode(StatusCodes.Status201Created, createdLesson); }
            catch (ArgumentException ex) { _logger.LogWarning(ex, "Bad request adding lesson: {ErrorMessage}", ex.Message); return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { _logger.LogWarning(ex, "Add lesson failed: Business rule violation: {ErrorMessage}", ex.Message); return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error adding lesson to course {CourseId}", createLessonDto.CourseId); return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while adding the lesson." }); }
        }

        [HttpPut("lessons/{lessonId}")]
        [ProducesResponseType(typeof(LessonDto), StatusCodes.Status200OK)] /* ... */
        public async Task<ActionResult<LessonDto>> UpdateLesson(int lessonId, [FromBody] UpdateLessonDto updateLessonDto)
        {
            // (Implementation remains the same)
            _logger.LogInformation("Received request to update lesson: {LessonId}", lessonId);
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try { var updatedLesson = await _courseService.UpdateLessonAsync(lessonId, updateLessonDto); _logger.LogInformation("Lesson {LessonId} updated successfully.", lessonId); return Ok(updatedLesson); }
            catch (KeyNotFoundException ex) { _logger.LogWarning(ex, "Update failed: Lesson not found: ID {LessonId}", lessonId); return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { _logger.LogWarning(ex, "Update lesson failed: Business rule violation for {LessonId}: {ErrorMessage}", lessonId, ex.Message); return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error updating lesson {LessonId}", lessonId); return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while updating the lesson." }); }
        }

        [HttpDelete("lessons/{lessonId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] /* ... */
        public async Task<IActionResult> DeleteLesson(int lessonId)
        {
            // (Implementation remains the same)
            _logger.LogInformation("Received request to delete lesson: {LessonId}", lessonId);
            try { await _courseService.DeleteLessonAsync(lessonId); _logger.LogInformation("Lesson {LessonId} deleted successfully.", lessonId); return NoContent(); }
            catch (KeyNotFoundException ex) { _logger.LogWarning(ex, "Delete failed: Lesson not found: ID {LessonId}", lessonId); return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error deleting lesson {LessonId}", lessonId); return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while deleting the lesson." }); }
        }


        // ==============================
        //      DOCUMENT ENDPOINTS (Remain the same)
        // ==============================
        [HttpPost("lessons/{lessonId}/documents")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CourseDocumentDto), StatusCodes.Status201Created)] /* ... */
        public async Task<ActionResult<CourseDocumentDto>> UploadDocument(int lessonId, IFormFile file)
        {
            // (Implementation remains the same)
            _logger.LogInformation("Received request to upload document for lesson {LessonId}. File: {FileName}", lessonId, file?.FileName ?? "N/A");
            if (file == null || file.Length == 0) { _logger.LogWarning("Upload document request for lesson {LessonId} received no file.", lessonId); return BadRequest(new { message = "No file was uploaded. Please provide a document file." }); }
            try { _logger.LogDebug("Calling service to upload document '{FileName}' for lesson {LessonId}", file.FileName, lessonId); var createdDocument = await _courseService.UploadDocumentAsync(lessonId, file); _logger.LogInformation("Document {DocumentId} uploaded successfully for lesson {LessonId}: {FileName}", createdDocument.Id, lessonId, file.FileName); return StatusCode(StatusCodes.Status201Created, createdDocument); }
            catch (ArgumentException ex) { _logger.LogWarning(ex, "Bad request uploading document for lesson {LessonId}: {ErrorMessage}", lessonId, ex.Message); return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { _logger.LogWarning(ex, "Upload failed: Lesson not found: ID {LessonId}", lessonId); return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { _logger.LogError(ex, "File storage operation failed uploading document for lesson {LessonId}: {ErrorMessage}", lessonId, ex.Message); return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Could not save the document: {ex.Message}" }); }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error uploading document '{FileName}' for lesson {LessonId}", file.FileName, lessonId); return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while uploading the document." }); }
        }

        [HttpDelete("documents/{documentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] /* ... */
        public async Task<IActionResult> DeleteDocument(int documentId)
        {
            // (Implementation remains the same)
            _logger.LogInformation("Received request to delete document: {DocumentId}", documentId);
            try { await _courseService.DeleteDocumentAsync(documentId); _logger.LogInformation("Document {DocumentId} deleted successfully.", documentId); return NoContent(); }
            catch (KeyNotFoundException ex) { _logger.LogWarning(ex, "Delete failed: Document not found: ID {DocumentId}", documentId); return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error deleting document {DocumentId}", documentId); return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while deleting the document." }); }
        }


        // ==============================
        //      LOOKUP ENDPOINTS (Remain the same)
        // ==============================
        [AllowAnonymous]
        [HttpGet("categories")]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)] /* ... */
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            // (Implementation remains the same)
            _logger.LogInformation("Received request to get course categories.");
            try { var categories = await _courseService.GetCourseCategoriesAsync(); return Ok(categories); }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error retrieving course categories."); return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving categories." }); }
        }

        [AllowAnonymous]
        [HttpGet("technologies")]
        [ProducesResponseType(typeof(IEnumerable<TechnologyDto>), StatusCodes.Status200OK)] /* ... */
        public async Task<ActionResult<IEnumerable<TechnologyDto>>> GetTechnologies()
        {
            // (Implementation remains the same)
            _logger.LogInformation("Received request to get technologies.");
            try { var technologies = await _courseService.GetTechnologiesAsync(); return Ok(technologies); }
            catch (Exception ex) { _logger.LogError(ex, "Unexpected error retrieving technologies."); return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving technologies." }); }
        }

    }
}