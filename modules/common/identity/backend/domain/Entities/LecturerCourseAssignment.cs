using System;
using System.Collections.Generic;

namespace ObeAunQa.Modules.Identity.Domain.Entities;

public partial class LecturerCourseAssignment
{
    public long UserId { get; set; }

    public long ProgramCourseId { get; set; }

    public long? AssignedBy { get; set; }

    public DateTime AssignedAt { get; set; }

    public virtual User? AssignedByNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
