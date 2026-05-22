using System;

namespace AcademicSystem.Application.DTOs.Announcements
{
    public class AnnouncementDto
    {
        public Guid Id { get; set; }
        public Guid? ClassId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
        public DateTime PostedAt { get; set; }
    }
}
