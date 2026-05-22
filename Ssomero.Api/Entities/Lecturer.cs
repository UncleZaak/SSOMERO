using System;
using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class Lecturer
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Photo { get; set; }

    [MaxLength(50)]
    public string? StaffId { get; set; }

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsVerified { get; set; }
    public bool IsApproved { get; set; }

    public Guid? UniversityId { get; set; }
    public University? University { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<LecturerClass> LecturerClasses { get; set; } = [];
}
