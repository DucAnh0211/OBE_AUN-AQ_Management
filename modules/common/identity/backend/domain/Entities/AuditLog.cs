using System;
using System.Collections.Generic;

namespace ObeAunQa.Modules.Identity.Domain.Entities;

public partial class AuditLog
{
    public long Id { get; set; }

    public long? ActorUserId { get; set; }

    public string Action { get; set; } = null!;

    public string TargetType { get; set; } = null!;

    public string? TargetId { get; set; }

    public string Details { get; set; } = null!;

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? ActorUser { get; set; }
}
