using ExcellyGenLMS.Core.Entities.Auth;
using System.Linq;

namespace ExcellyGenLMS.Application.Services.Auth
{
	public static class SuperAdminExtensions
	{
		// Constants
		private const string SUPER_ADMIN_ROLE = "SuperAdmin";
		private const string ADMIN_ROLE = "Admin";

		// Check if a user is a SuperAdmin
		public static bool IsSuperAdmin(this User user)
		{
			return user.Roles != null && user.Roles.Any(r => r == SUPER_ADMIN_ROLE);
		}

		// Check if a user is a regular Admin (but not SuperAdmin)
		public static bool IsRegularAdmin(this User user)
		{
			return user.Roles != null &&
				   user.Roles.Any(r => r == ADMIN_ROLE) &&
				   !user.Roles.Any(r => r == SUPER_ADMIN_ROLE);
		}

		// Check if a user has any admin privileges (either Admin or SuperAdmin)
		public static bool HasAdminPrivileges(this User user)
		{
			return user.Roles != null &&
				   (user.Roles.Any(r => r == ADMIN_ROLE) ||
					user.Roles.Any(r => r == SUPER_ADMIN_ROLE));
		}

		// Check if current user can delete target user
		public static bool CanDeleteUser(this User currentUser, User targetUser)
		{
			// SuperAdmin can delete anyone except themselves
			if (currentUser.IsSuperAdmin())
			{
				return currentUser.Id != targetUser.Id;
			}

			// Regular Admin can only delete non-admin users
			if (currentUser.IsRegularAdmin())
			{
				return !targetUser.HasAdminPrivileges();
			}

			// Non-admin users cannot delete anyone
			return false;
		}

		// Check if current user can edit target user
		public static bool CanEditUser(this User currentUser, User targetUser)
		{
			// SuperAdmin can edit anyone (including themselves)
			if (currentUser.IsSuperAdmin())
			{
				return true;
			}

			// Regular Admin can only edit non-admin users
			if (currentUser.IsRegularAdmin())
			{
				return !targetUser.HasAdminPrivileges();
			}

			// Non-admin users cannot edit anyone
			return false;
		}

		// Check if user can create users with specific role
		public static bool CanCreateUserWithRole(this User currentUser, string role)
		{
			// Only SuperAdmin can create another SuperAdmin
			if (role == SUPER_ADMIN_ROLE)
			{
				return currentUser.IsSuperAdmin();
			}

			// Both Admin and SuperAdmin can create users with other roles
			return currentUser.HasAdminPrivileges();
		}
	}
}