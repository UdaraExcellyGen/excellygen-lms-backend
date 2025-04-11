using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Auth
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User> CreateUserAsync(User user)
        {
            user.Id = Guid.NewGuid().ToString();
            user.JoinedDate = DateTime.UtcNow;
            user.Status = "active";

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id)
                ?? throw new KeyNotFoundException($"User with ID {user.Id} not found");

            // Update properties
            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Phone = user.Phone;
            existingUser.Roles = user.Roles;
            existingUser.Department = user.Department;
            existingUser.Status = user.Status;

            // Optional fields
            if (!string.IsNullOrEmpty(user.JobRole))
                existingUser.JobRole = user.JobRole;

            if (!string.IsNullOrEmpty(user.About))
                existingUser.About = user.About;

            if (user.Avatar != null)
                existingUser.Avatar = user.Avatar;

            await _context.SaveChangesAsync();

            return existingUser;
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await _context.Users.FindAsync(id)
                ?? throw new KeyNotFoundException($"User with ID {id} not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<User>> SearchUsersAsync(string? searchTerm, List<string>? roles, string? status)
        {
            var query = _context.Users.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u =>
                    u.Name.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.Id.ToLower().Contains(searchTerm));
            }

            // Apply role filter
            if (roles != null && roles.Any())
            {
                // This requires a custom approach due to the JSON serialized roles
                // We'll need to fetch all and filter in memory for exact matching
                var users = await query.ToListAsync();
                users = users.Where(u =>
                    u.Roles.Any(role => roles.Contains(role))).ToList();

                // Apply status filter to the in-memory list
                if (!string.IsNullOrWhiteSpace(status) && status != "all")
                {
                    users = users.Where(u => u.Status == status).ToList();
                }

                return users;
            }

            // Apply status filter at the database level if roles filter is not used
            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                query = query.Where(u => u.Status == status);
            }

            return await query.ToListAsync();
        }
    }
}