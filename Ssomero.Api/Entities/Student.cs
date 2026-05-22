using System;
using System.ComponentModel.DataAnnotations;

namespace Ssomero.Api.Entities;

public class Student
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string SecondName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? OtherNames { get; set; }

    public DateOnly Dob { get; set; }

    [Required, MaxLength(20)]
    public string Gender { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Photo { get; set; }

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsVerified { get; set; }

    public Guid? UniversityId { get; set; }
    public University? University { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Optimistic-concurrency token — EF Core uses this to detect conflicting updates.
    public byte[] RowVersion { get; set; } = [];

    // Navigation
    public AcademicProfile? AcademicProfile { get; set; }
    public ICollection<StudentClass> StudentClasses { get; set; } = [];
}
