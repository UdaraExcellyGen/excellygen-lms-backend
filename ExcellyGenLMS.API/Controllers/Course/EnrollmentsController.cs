// ExcellyGenLMS.API/Controllers/Course/EnrollmentsController.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.API.Controllers.Course
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ILogger<EnrollmentsController> _logger;

        public EnrollmentsController(
            IEnrollmentService enrollmentService,
            ILogger<EnrollmentsController> logger)
        {
            _enrollmentService = enrollmentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<EnrollmentDto>>> GetEnrollments()
        {
            try
            {
                _logger.LogInformation("Getting all enrollments");
                var enrollments = await _enrollmentService.GetAllEnrollmentsAsync();
                return Ok(enrollments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enrollments");
                return StatusCode(500, new { message = "An error occurred while retrieving enrollments." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EnrollmentDto>> GetEnrollmentById(int id)
        {
            try
            {
                _logger.LogInformation("Getting enrollment with ID: {EnrollmentId}", id);
                var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
                return Ok(enrollment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Enrollment not found: {EnrollmentId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enrollment: {EnrollmentId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the enrollment." });
            }
        }

        [HttpPost]
        public async Task<ActionResult<EnrollmentDto>> CreateEnrollment([FromBody] CreateEnrollmentDto createEnrollmentDto)
        {
            try
            {
                _logger.LogInformation("Creating new enrollment");
                var enrollment = await _enrollmentService.CreateEnrollmentAsync(createEnrollmentDto);
                return CreatedAtAction(nameof(GetEnrollmentById), new { id = enrollment.Id }, enrollment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating enrollment");
                return StatusCode(500, new { message = "An error occurred while creating the enrollment." });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EnrollmentDto>> UpdateEnrollment(int id, [FromBody] UpdateEnrollmentDto updateEnrollmentDto)
        {
            try
            {
                _logger.LogInformation("Updating enrollment with ID: {EnrollmentId}", id);
                var enrollment = await _enrollmentService.UpdateEnrollmentAsync(id, updateEnrollmentDto);
                return Ok(enrollment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Enrollment not found: {EnrollmentId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating enrollment: {EnrollmentId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the enrollment." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEnrollment(int id)
        {
            try
            {
                _logger.LogInformation("Deleting enrollment with ID: {EnrollmentId}", id);
                await _enrollmentService.DeleteEnrollmentAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Enrollment not found: {EnrollmentId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting enrollment: {EnrollmentId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the enrollment." });
            }
        }
    }
}