using System;

namespace AcademicSystem.Application.DTOs.Announcements
{
    public class UpdateAnnouncementDto
    {
        public Guid? ClassId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
    }
}
