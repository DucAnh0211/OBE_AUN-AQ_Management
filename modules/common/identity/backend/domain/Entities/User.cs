using System;
using System.Collections.Generic;

namespace ObeAunQa.Modules.Identity.Domain.Entities;

public partial class User
{
    public long Id { get; set; }

    public string Email { get; set; } = null!;

    public string NormalizedEmail { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool MustChangePassword { get; set; }

    public int TokenVersion { get; set; }

    public int FailedLoginCount { get; set; }

    public DateTime? LockedUntil { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<LecturerCourseAssignment> LecturerCourseAssignmentAssignedByNavigations { get; set; } = new List<LecturerCourseAssignment>();

    public virtual ICollection<LecturerCourseAssignment> LecturerCourseAssignmentUsers { get; set; } = new List<LecturerCourseAssignment>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<StudentProgramAssignment> StudentProgramAssignmentAssignedByNavigations { get; set; } = new List<StudentProgramAssignment>();

    public virtual ICollection<StudentProgramAssignment> StudentProgramAssignmentUsers { get; set; } = new List<StudentProgramAssignment>();
}
