namespace Ssomero.Models;

// ── Response models (mirror backend DTOs) ─────────────────────────────────────

public class ElectionCandidateModel
{
    public Guid   StudentId    { get; set; }
    public string StudentName  { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public int    VoteCount    { get; set; }

    public string VoteCountLabel => VoteCount == 1 ? "1 vote" : $"{VoteCount} votes";
    public string Initials       => BuildInitials(StudentName);

    private static string BuildInitials(string name) =>
        string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                          .Where(p => p.Length > 0)
                          .Take(2)
                          .Select(p => char.ToUpper(p[0]).ToString()));
}

public class ClassElectionModel
{
    public Guid   Id               { get; set; }
    public Guid   ClassId          { get; set; }
    public string ClassName        { get; set; } = string.Empty;
    public string Status           { get; set; } = string.Empty; // "Active" | "Completed"
    public int    SecondsRemaining { get; set; }
    public bool   CanVote          { get; set; }
    public bool   HasVoted         { get; set; }
    public string? WinnerName      { get; set; }
    public Guid?   WinnerStudentId { get; set; }
    public List<ElectionCandidateModel> Candidates { get; set; } = [];

    // ── Computed ──────────────────────────────────────────────────────────────
    public bool   IsActive    => Status?.Equals("Active",    StringComparison.OrdinalIgnoreCase) == true;
    public bool   IsCompleted => Status?.Equals("Completed", StringComparison.OrdinalIgnoreCase) == true;
    public string CountdownLabel => SecondsRemaining > 0
        ? $"{SecondsRemaining}s remaining"
        : "Voting closed";
    public string WinnerDisplay => string.IsNullOrWhiteSpace(WinnerName)
        ? "No winner yet"
        : $"🏆 {WinnerName}";
    public int TotalVotes => Candidates.Sum(c => c.VoteCount);
    public string TotalVotesLabel => TotalVotes == 1 ? "1 vote cast" : $"{TotalVotes} votes cast";
}

// ── Request models ─────────────────────────────────────────────────────────────

public class StartElectionRequest
{
    public Guid ClassId { get; set; }
}

public class VoteRequest
{
    public Guid CandidateStudentId { get; set; }
}
