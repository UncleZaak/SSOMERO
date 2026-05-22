using System;

namespace Ssomero.Api.Entities;

public class ClassElection
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public Guid StartedByStudentId { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime EndsAt { get; set; }

    /// <summary>Active, Completed, Cancelled</summary>
    public string Status { get; set; } = "Active";

    public Guid? WinnerStudentId { get; set; }
    public DateTime? CompletedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public ICollection<ClassElectionCandidate> Candidates { get; set; } = [];
    public ICollection<ClassElectionVote> Votes { get; set; } = [];
}
