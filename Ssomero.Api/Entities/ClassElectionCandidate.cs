using System;

namespace Ssomero.Api.Entities;

public class ClassElectionCandidate
{
    public Guid Id { get; set; }
    public Guid ElectionId { get; set; }
    public ClassElection Election { get; set; } = null!;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
