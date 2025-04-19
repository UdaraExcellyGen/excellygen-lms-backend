using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Auth;
using ExcellyGenLMS.Application.Interfaces.Auth;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;

namespace ExcellyGenLMS.Application.Services.Auth
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public UserService(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return users.Select(MapToDto).ToList();
        }

        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");

            return MapToDto(user);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Check if email is already in use
            var existingUser = await _userRepository.GetUserByEmailAsync(createUserDto.Email);
            if (existingUser != null)
                throw new InvalidOperationException($"Email {createUserDto.Email} is already in use");

            // In a real app, you would hash the password here
            var hashedPassword = createUserDto.Password; // Replace with a proper hashing function

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = createUserDto.Name,
                Email = createUserDto.Email,
                Phone = createUserDto.Phone,
                Roles = createUserDto.Roles,
                Department = createUserDto.Department,
                Status = "active",  // Set default status to active
                JoinedDate = DateTime.UtcNow  // Set joined date to current time
                // Firebase UID would be set after authentication
            };

            var createdUser = await _userRepository.CreateUserAsync(user);
            return MapToDto(createdUser);
        }

        public async Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateUserDto)
        {
            var existingUser = await _userRepository.GetUserByIdAsync(id);
            if (existingUser == null)
                throw new KeyNotFoundException($"User with ID {id} not found");

            // Check if email is changing and already in use by another user
            if (existingUser.Email != updateUserDto.Email)
            {
                var userWithEmail = await _userRepository.GetUserByEmailAsync(updateUserDto.Email);
                if (userWithEmail != null && userWithEmail.Id != id)
                    throw new InvalidOperationException($"Email {updateUserDto.Email} is already in use");
            }

            // Update user
            existingUser.Name = updateUserDto.Name;
            existingUser.Email = updateUserDto.Email;
            existingUser.Phone = updateUserDto.Phone;
            existingUser.Roles = updateUserDto.Roles;
            existingUser.Department = updateUserDto.Department;
            existingUser.Status = updateUserDto.Status;

            // If password is provided, update it (in a real app, you would hash it)
            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                // Update password logic here (usually involves hashing)
                // existingUser.PasswordHash = HashPassword(updateUserDto.Password);
            }

            var updatedUser = await _userRepository.UpdateUserAsync(existingUser);
            return MapToDto(updatedUser);
        }

        public async Task DeleteUserAsync(string id)
        {
            await _userRepository.DeleteUserAsync(id);
        }

        public async Task<UserDto> ToggleUserStatusAsync(string id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");

            // Toggle status
            user.Status = user.Status == "active" ? "inactive" : "active";

            var updatedUser = await _userRepository.UpdateUserAsync(user);
            return MapToDto(updatedUser);
        }

        public async Task<List<UserDto>> SearchUsersAsync(UserSearchParams searchParams)
        {
            var users = await _userRepository.SearchUsersAsync(
                searchParams.SearchTerm,
                searchParams.Roles,
                searchParams.Status);

            return users.Select(MapToDto).ToList();
        }

        // Helper method to map User entity to UserDto
        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Roles = user.Roles,
                Department = user.Department,
                Status = user.Status,
                JoinedDate = user.JoinedDate,
                JobRole = user.JobRole,
                About = user.About,
                FirebaseUid = user.FirebaseUid,
                Avatar = user.Avatar
            };
        }

        public string GetUserIdFromToken(string token)
        {
            return _tokenService.GetUserIdFromToken(token);
        }

        public string GetCurrentRoleFromToken(string token)
        {
            return _tokenService.GetCurrentRoleFromToken(token);
        }
    }
}