using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ssomero.Models;

public class UserItem : INotifyPropertyChanged
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Program { get; set; }
    public string? StaffId { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }

    bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected == value) return;
            isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string DisplaySubtitle =>
        Role == "Student" ? (Program ?? "No program") : (StaffId ?? "Lecturer");

    public string Initials
    {
        get
        {
            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2
                ? $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant()
                : (parts.Length == 1 ? parts[0][..1].ToUpperInvariant() : "?");
        }
    }

    public bool IsActive => Status == "Active";
    public bool IsSuspended => Status == "Suspended";

    /// <summary>True for lecturers awaiting admin approval.</summary>
    public bool IsPendingApproval => Role == "Lecturer" && !IsApproved;
}
