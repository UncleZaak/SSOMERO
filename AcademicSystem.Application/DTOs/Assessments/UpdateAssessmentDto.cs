using System;

namespace AcademicSystem.Application.DTOs.Assessments
{
    public class UpdateAssessmentDto
    {
        public string Title { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
