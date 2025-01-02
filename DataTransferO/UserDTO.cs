using System;

namespace DataTransferO
{
    public class UserDTO
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public DateOnly DateOfBirth { get; set; }

        // Change Email from nullable to non-nullable
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid RoleId { get; set; }
        public string? PasswordHash { get; set; }

        // Update type from object to bool
        public bool IsDeleted { get; set; } = false;

        // Constructor to initialize required properties
        public UserDTO(Guid userId, string name, DateOnly dateOfBirth, string email, string phoneNumber, string gender, Guid roleId, string passwordHash)
        {
            UserId = userId;
            Name = name;
            DateOfBirth = dateOfBirth;
            Email = email;
            PhoneNumber = phoneNumber;
            Gender = gender;
            RoleId = roleId;
            PasswordHash = passwordHash;
            IsDeleted = false; // Ensure IsDeleted is initialized in the constructor
        }

        // Parameterless constructor for cases where other properties are set later
        public UserDTO()
        {
            Email = string.Empty; // Ensure Email is initialized in the parameterless constructor
            IsDeleted = false;
        }
    }
}
