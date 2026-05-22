using System;

namespace Ssomero.Api.Entities;

public class ClassElectionVote
{
    public Guid Id { get; set; }
    public Guid ElectionId { get; set; }
    public ClassElection Election { get; set; } = null!;

    public Guid VoterStudentId { get; set; }
    public Student VoterStudent { get; set; } = null!;

    public Guid CandidateStudentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
