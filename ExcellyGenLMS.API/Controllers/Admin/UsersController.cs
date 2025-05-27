using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.Interfaces.Admin;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]  // Changed from "admin" to "Admin" to match database
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserManagementService userManagementService,
            ILogger<UsersController> logger)
        {
            _userManagementService = userManagementService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<AdminUserDto>>> GetAllUsers()
        {
            try
            {
                _logger.LogInformation("Getting all users");
                var users = await _userManagementService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, new { message = "An error occurred while retrieving users", details = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<AdminUserDto>>> SearchUsers([FromQuery] AdminUserSearchParams searchParams)
        {
            try
            {
                _logger.LogInformation($"Searching users with params");
                var users = await _userManagementService.SearchUsersAsync(searchParams);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                return StatusCode(500, new { message = "An error occurred while searching users", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AdminUserDto>> GetUserById(string id)
        {
            try
            {
                _logger.LogInformation($"Getting user with ID: {id}");
                var user = await _userManagementService.GetUserByIdAsync(id);

                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user with ID: {id}");
                return StatusCode(500, new { message = "An error occurred while retrieving the user", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<AdminUserDto>> CreateUser([FromBody] AdminCreateUserDto createUserDto)
        {
            try
            {
                _logger.LogInformation($"Creating new user with email: {createUserDto.Email}");
                var createdUser = await _userManagementService.CreateUserAsync(createUserDto);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { message = "An error occurred while creating the user", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AdminUserDto>> UpdateUser(string id, [FromBody] AdminUpdateUserDto updateUserDto)
        {
            try
            {
                _logger.LogInformation($"Updating user with ID: {id}");
                var updatedUser = await _userManagementService.UpdateUserAsync(id, updateUserDto);

                if (updatedUser == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found" });
                }

                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with ID: {id}");
                return StatusCode(500, new { message = "An error occurred while updating the user", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            try
            {
                _logger.LogInformation($"Deleting user with ID: {id}");
                var result = await _userManagementService.DeleteUserAsync(id);

                if (!result)
                {
                    return NotFound(new { message = $"User with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID: {id}");
                return StatusCode(500, new { message = "An error occurred while deleting the user", details = ex.Message });
            }
        }

        [HttpPost("{id}/toggle-status")]
        public async Task<ActionResult<AdminUserDto>> ToggleUserStatus(string id)
        {
            try
            {
                _logger.LogInformation($"Toggling status for user with ID: {id}");
                var user = await _userManagementService.ToggleUserStatusAsync(id);

                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling status for user with ID: {id}");
                return StatusCode(500, new { message = "An error occurred while toggling user status", details = ex.Message });
            }
        }
    }
}