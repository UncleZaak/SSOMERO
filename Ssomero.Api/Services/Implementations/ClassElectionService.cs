using Microsoft.EntityFrameworkCore;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;
using Ssomero.Api.Services.Interfaces;

namespace Ssomero.Api.Services.Implementations;

public sealed class ClassElectionService : IClassElectionService
{
    private readonly SsomeroDbContext _db;
    private readonly ILogger<ClassElectionService> _logger;

    public ClassElectionService(SsomeroDbContext db, ILogger<ClassElectionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── StartElectionAsync ───────────────────────────────────────────────────

    public async Task<ClassElectionDto> StartElectionAsync(Guid userId, StartElectionRequestDto dto, CancellationToken ct = default)
    {
        var cls = await _db.Classes
            .Where(c => c.Id == dto.ClassId && c.ParentClassId == null)
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("Class not found or is not a main class.");

        var enrolled = await _db.StudentClasses
            .AnyAsync(sc => sc.StudentId == userId && sc.ClassId == dto.ClassId && sc.Status == "active", ct);

        if (!enrolled)
            throw new InvalidOperationException("You are not enrolled in this class.");

        // Return existing active election if one exists
        var existing = await _db.ClassElections
            .Where(e => e.ClassId == dto.ClassId && e.Status == "Active" && !e.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (existing != null)
            return await BuildDtoAsync(existing, userId, ct);

        var now = DateTime.UtcNow;
        var election = new ClassElection
        {
            Id = Guid.NewGuid(),
            ClassId = dto.ClassId,
            StartedByStudentId = userId,
            StartedAt = now,
            EndsAt = now.AddMinutes(1),
            Status = "Active",
        };

        _db.ClassElections.Add(election);

        _db.ClassElectionCandidates.Add(new ClassElectionCandidate
        {
            Id = Guid.NewGuid(),
            ElectionId = election.Id,
            StudentId = userId,
            CreatedAt = now,
        });

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Election {ElectionId} started for class {ClassId} by student {StudentId}.",
            election.Id, dto.ClassId, userId);

        return await BuildDtoAsync(election, userId, ct);
    }

    // ── GetActiveElectionAsync ───────────────────────────────────────────────

    public async Task<ClassElectionDto?> GetActiveElectionAsync(Guid userId, Guid classId, CancellationToken ct = default)
    {
        var election = await _db.ClassElections
            .Where(e => e.ClassId == classId && e.Status == "Active" && !e.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (election == null) return null;

        if (DateTime.UtcNow >= election.EndsAt)
        {
            var finalized = await FinalizeElectionAsync(election.Id, ct);
            return finalized;
        }

        return await BuildDtoAsync(election, userId, ct);
    }

    // ── VoteAsync ────────────────────────────────────────────────────────────

    public async Task<ClassElectionDto> VoteAsync(Guid userId, Guid electionId, VoteRequestDto dto, CancellationToken ct = default)
    {
        var election = await _db.ClassElections
            .Where(e => e.Id == electionId && !e.IsDeleted)
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("Election not found.");

        if (election.Status != "Active")
            throw new InvalidOperationException("Election is not active.");

        if (DateTime.UtcNow >= election.EndsAt)
            throw new InvalidOperationException("Election has expired.");

        var enrolled = await _db.StudentClasses
            .AnyAsync(sc => sc.StudentId == userId && sc.ClassId == election.ClassId && sc.Status == "active", ct);

        if (!enrolled)
            throw new InvalidOperationException("You are not enrolled in this class.");

        var alreadyVoted = await _db.ClassElectionVotes
            .AnyAsync(v => v.ElectionId == electionId && v.VoterStudentId == userId, ct);

        if (alreadyVoted)
            throw new InvalidOperationException("You have already voted in this election.");

        var candidateExists = await _db.ClassElectionCandidates
            .AnyAsync(c => c.ElectionId == electionId && c.StudentId == dto.CandidateStudentId, ct);

        if (!candidateExists)
            throw new InvalidOperationException("Candidate not found in this election.");

        _db.ClassElectionVotes.Add(new ClassElectionVote
        {
            Id = Guid.NewGuid(),
            ElectionId = electionId,
            VoterStudentId = userId,
            CandidateStudentId = dto.CandidateStudentId,
            CreatedAt = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync(ct);
        return await BuildDtoAsync(election, userId, ct);
    }

    // ── FinalizeElectionAsync ────────────────────────────────────────────────

    public async Task<ClassElectionDto?> FinalizeElectionAsync(Guid electionId, CancellationToken ct = default)
    {
        var election = await _db.ClassElections
            .Where(e => e.Id == electionId && !e.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (election == null) return null;

        if (election.Status == "Completed")
            return await BuildDtoAsync(election, Guid.Empty, ct);

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var candidates = await _db.ClassElectionCandidates
                .Where(c => c.ElectionId == electionId)
                .ToListAsync(ct);

            var votes = await _db.ClassElectionVotes
                .Where(v => v.ElectionId == electionId)
                .ToListAsync(ct);

            // Determine winner: highest votes, tie-break by earliest CreatedAt
            var winner = candidates
                .Select(c => new
                {
                    Candidate = c,
                    VoteCount = votes.Count(v => v.CandidateStudentId == c.StudentId),
                })
                .OrderByDescending(x => x.VoteCount)
                .ThenBy(x => x.Candidate.CreatedAt)
                .First()
                .Candidate;

            // Demote any existing class rep in the main class
            var existingReps = await _db.StudentClasses
                .Where(sc => sc.ClassId == election.ClassId && sc.Role == "class_rep")
                .ToListAsync(ct);

            foreach (var rep in existingReps)
                rep.Role = "student";

            // Promote winner
            var winnerMembership = await _db.StudentClasses
                .Where(sc => sc.StudentId == winner.StudentId && sc.ClassId == election.ClassId)
                .FirstOrDefaultAsync(ct);

            if (winnerMembership != null)
                winnerMembership.Role = "class_rep";

            // Finalize election
            election.Status = "Completed";
            election.WinnerStudentId = winner.StudentId;
            election.CompletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            _logger.LogInformation("Election {ElectionId} finalized. Winner: {WinnerId}.", electionId, winner.StudentId);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        return await BuildDtoAsync(election, Guid.Empty, ct);
    }

    // ── Helper: BuildDtoAsync ────────────────────────────────────────────────

    private async Task<ClassElectionDto> BuildDtoAsync(ClassElection election, Guid currentUserId, CancellationToken ct)
    {
        var cls = await _db.Classes
            .Where(c => c.Id == election.ClassId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(ct) ?? string.Empty;

        var candidates = await _db.ClassElectionCandidates
            .Where(c => c.ElectionId == election.Id)
            .Join(_db.Students,
                c => c.StudentId,
                s => s.Id,
                (c, s) => new
                {
                    c.StudentId,
                    FullName = s.FirstName + " " + s.SecondName,
                    StudentNumber = s.Email, // use email as student number if no dedicated field
                    c.CreatedAt,
                })
            .ToListAsync(ct);

        var votes = await _db.ClassElectionVotes
            .Where(v => v.ElectionId == election.Id)
            .ToListAsync(ct);

        var candidateDtos = candidates
            .Select(c => new ElectionCandidateDto(
                c.StudentId,
                c.FullName,
                c.StudentNumber,
                votes.Count(v => v.CandidateStudentId == c.StudentId),
                c.StudentId == currentUserId
            ))
            .ToList();

        var hasVoted = currentUserId != Guid.Empty &&
            votes.Any(v => v.VoterStudentId == currentUserId);

        var isEnrolled = currentUserId != Guid.Empty &&
            await _db.StudentClasses.AnyAsync(
                sc => sc.StudentId == currentUserId && sc.ClassId == election.ClassId && sc.Status == "active", ct);

        var secondsRemaining = election.Status == "Active"
            ? (int)Math.Max(0, (election.EndsAt - DateTime.UtcNow).TotalSeconds)
            : 0;

        var canVote = election.Status == "Active"
            && secondsRemaining > 0
            && isEnrolled
            && !hasVoted;

        string? winnerName = null;
        if (election.WinnerStudentId.HasValue)
        {
            winnerName = await _db.Students
                .Where(s => s.Id == election.WinnerStudentId.Value)
                .Select(s => s.FirstName + " " + s.SecondName)
                .FirstOrDefaultAsync(ct);
        }

        return new ClassElectionDto(
            election.Id,
            election.ClassId,
            cls,
            election.StartedAt,
            election.EndsAt,
            election.Status,
            secondsRemaining,
            canVote,
            hasVoted,
            election.WinnerStudentId,
            winnerName,
            candidateDtos
        );
    }
}
