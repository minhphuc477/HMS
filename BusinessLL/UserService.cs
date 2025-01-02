using AutoMapper;
using DataAL.Models;
using DataTransferO;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BusinessLL
{
    public class UserService
    {
        private readonly HmsAContext _context;
        private readonly Dictionary<string, Guid> _roleDictionary;
        private readonly IMapper _mapper;
        private readonly AuditLogService _auditLogService;

        // Add a field to store the current user ID
        private Guid _currentUserId;

        public UserService(HmsAContext context, IMapper mapper, AuditLogService auditLogService)
        {
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;

            // Define the roles with their corresponding GUIDs.
            _roleDictionary = new Dictionary<string, Guid>
                {
                    { "Admin", new Guid("4697688e-b27b-411e-8ff2-e6a0126c68d6") },
                    { "Doctor", new Guid("9d8eb0a2-2954-49c4-a933-612d766d3fa5") },
                    { "Patient", new Guid("803dcef7-b173-4799-9ac5-48da9be43f2e") }
                };

            _currentUserId = Guid.Empty; // Initialize to an empty GUID
        }

        #region Role Management
        public string? GetRoleNameById(Guid roleId)
        {
            if (_roleDictionary.TryGetValue(_roleDictionary.FirstOrDefault(r => r.Value == roleId).Key, out Guid value))
            {
                return _roleDictionary.FirstOrDefault(r => r.Value == roleId).Key;
            }
            return null;
        }

        public Guid GetRoleIdByName(string roleName)
        {
            if (_roleDictionary.TryGetValue(roleName, out Guid roleId))
            {
                return roleId;
            }
            throw new Exception("Role not found");
        }
        #endregion

        #region User Management
        public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            return user != null ? _mapper.Map<UserDTO>(user) : null;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10)
        {
            var users = await _context.Users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<UserDTO?> GetCurrentUserAsync(Guid userId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            return user != null ? _mapper.Map<UserDTO>(user) : null;
        }

        public async Task CreateUserAsync(UserDTO userDto)
        {
            ValidateUser(userDto);

            if (userDto.PasswordHash == null || !IsPasswordHashed(userDto.PasswordHash))
            {
                userDto.PasswordHash = HashPassword(userDto.PasswordHash ?? string.Empty);
            }

            var user = _mapper.Map<User>(userDto);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAuditAsync("Users", user.UserId, "Create", null, JsonConvert.SerializeObject(user), "System");
        }

        public async Task UpdateUserAsync(UserDTO userDto)
        {
            var existingUser = await _context.Users.FindAsync(userDto.UserId);
            if (existingUser == null)
            {
                throw new Exception("User not found");
            }

            var oldValue = JsonConvert.SerializeObject(existingUser);

            if (existingUser.PasswordHash != userDto.PasswordHash)
            {
                existingUser.PasswordHash = HashPassword(userDto.PasswordHash ?? string.Empty);
            }

            ValidateUser(userDto);

            _mapper.Map(userDto, existingUser);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAuditAsync("Users", existingUser.UserId, "Update", oldValue, JsonConvert.SerializeObject(existingUser), "System");
        }

        public async Task UpdateUserInforAsync(UserDTO userDto)
        {
            var user = await _context.Users.FindAsync(userDto.UserId);
            if (user == null)
                throw new Exception("User not found.");

            // Map updated fields
            user.Name = userDto.Name ?? string.Empty;
            user.Email = userDto.Email ?? string.Empty;
            user.PhoneNumber = userDto.PhoneNumber ?? string.Empty;
            user.Gender = userDto.Gender ?? string.Empty;
            user.DateOfBirth = DateOnly.FromDateTime(userDto.DateOfBirth.ToDateTime(TimeOnly.MinValue));

            // Update other fields as necessary

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Log the update
            await _auditLogService.LogAuditAsync(
                tableName: "Users",
                recordId: user.UserId,
                changeType: "Update",
                oldValue: JsonConvert.SerializeObject(user),
                newValue: JsonConvert.SerializeObject(userDto),
                changedBy: userDto.UserId.ToString()
            );
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var oldValue = JsonConvert.SerializeObject(user);

            user.IsDeleted = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAuditAsync("Users", user.UserId, "Delete", oldValue, JsonConvert.SerializeObject(user), "System");
        }

        public async Task<UserDTO?> GetUserByEmailOrUsernameAsync(string emailOrUsername)
        {
            var lowerEmailOrUsername = emailOrUsername.ToLower();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == lowerEmailOrUsername || u.Name.ToLower() == lowerEmailOrUsername);
            return user != null ? _mapper.Map<UserDTO>(user) : null;
        }
        #endregion

        #region Helper Methods
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool IsPasswordHashed(string password)
        {
            return password.StartsWith("$2a$") || password.StartsWith("$2b$") || password.StartsWith("$2y$");
        }

        private void ValidateUser(UserDTO userDto)
        {
            if (string.IsNullOrEmpty(userDto.Email) || !IsValidEmail(userDto.Email))
            {
                throw new ArgumentException("Invalid email format.");
            }
        }

        public bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }
        #endregion

        public async Task<User?> FindUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ResetTokenDTO> GenerateResetTokenAsync(Guid userId)
        {
            var token = new ResetToken
            {
                TokenId = Guid.NewGuid(),
                UserId = userId,
                Token = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                Expiration = DateTime.UtcNow.AddHours(1)
            };

            _context.ResetTokens.Add(token);
            await _context.SaveChangesAsync();

            return _mapper.Map<ResetTokenDTO>(token);
        }

        public async Task<ResetTokenDTO?> VerifyResetTokenAsync(string token)
        {
            var resetToken = await _context.ResetTokens.FirstOrDefaultAsync(t => t.Token == token && t.Expiration > DateTime.UtcNow);
            return resetToken != null ? _mapper.Map<ResetTokenDTO>(resetToken) : null;
        }

        public UserDTO Login(string emailOrUsername, string password)
        {
            try
            {
                var user = GetUserByEmailOrUsernameAsync(emailOrUsername).Result;

                if (user == null)
                {
                    throw new Exception("User not found.");
                }

                if (user.IsDeleted is bool isDeleted && isDeleted)
                {
                    throw new Exception("User account is deleted.");
                }

                if (user.IsActive == false)
                {
                    throw new Exception("User account is inactive.");
                }

                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    throw new Exception("Password hash is missing or invalid.");
                }

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

                if (!isPasswordValid)
                {
                    throw new Exception("Invalid password.");
                }

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                throw;
            }
        }

        public async Task<UserDTO> LoginAsync(string emailOrUsername, string password)
        {
            try
            {
                var user = await GetUserByEmailOrUsernameAsync(emailOrUsername);

                if (user == null)
                {
                    throw new Exception("User not found.");
                }

                if (user.IsDeleted is bool isDeleted && isDeleted)
                {
                    throw new Exception("User account is deleted.");
                }

                if (user.IsActive == false)
                {
                    throw new Exception("User account is inactive.");
                }

                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    throw new Exception("Password hash is missing or invalid.");
                }

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

                if (!isPasswordValid)
                {
                    throw new Exception("Invalid password.");
                }

                // Set the current user ID upon successful login
                SetCurrentUserId(user.UserId);

                // Update UserSession
                UserSession.CurrentUser = user;

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<UserDTO>> GetUsersByRoleAsync(string roleName)
        {
            var roleId = _context.Roles.FirstOrDefault(r => r.RoleName == roleName)?.RoleId;
            if (roleId == null)
            {
                throw new Exception("Role not found");
            }

            var users = await _context.Users
                .Where(u => u.RoleId == roleId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        // Add methods to manage the current user ID
        public Guid GetCurrentUserId()
        {
            if (_currentUserId == Guid.Empty)
            {
                throw new InvalidOperationException("Current user ID is not set. Please log in.");
            }
            return _currentUserId;
        }

        public void SetCurrentUserId(Guid userId)
        {
            _currentUserId = userId;
        }

        public async Task ChangeUserPasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                throw new Exception("Current password is incorrect.");

            user.PasswordHash = HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Log the password change
            await _auditLogService.LogAuditAsync(
                tableName: "Users",
                recordId: user.UserId,
                changeType: "PasswordChange",
                oldValue: null,
                newValue: null,
                changedBy: userId.ToString()
            );
        }


        public async Task DeactivateUserAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            user.IsActive = false;
            user.IsDeleted = true;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Log the deactivation
            await _auditLogService.LogAuditAsync(
                tableName: "Users",
                recordId: user.UserId,
                changeType: "Deactivate",
                oldValue: null,
                newValue: JsonConvert.SerializeObject(user),
                changedBy: userId.ToString()
            );
        }


    }
}
