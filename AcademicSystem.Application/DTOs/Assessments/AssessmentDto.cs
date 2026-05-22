using System;

namespace AcademicSystem.Application.DTOs.Assessments
{
    public class AssessmentDto
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
