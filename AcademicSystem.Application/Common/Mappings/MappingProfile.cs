using AutoMapper;
using AcademicSystem.Domain.Entities;
using AcademicSystem.Application.DTOs.Students;
using AcademicSystem.Application.DTOs.Courses;
using AcademicSystem.Application.DTOs.Enrollments;
using AcademicSystem.Application.DTOs.Assessments;
using AcademicSystem.Application.DTOs.Announcements;
using AcademicSystem.Application.DTOs.Auth;
using AcademicSystem.Application.DTOs;

namespace AcademicSystem.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Students
            CreateMap<Student, StudentDto>().ReverseMap();
            CreateMap<CreateStudentDto, Student>()
                .ForMember(d => d.Id, opt => opt.Ignore());
            CreateMap<UpdateStudentDto, Student>()
                .ForMember(d => d.Id, opt => opt.Ignore());

            // Courses
            CreateMap<Course, CourseDto>().ReverseMap();
            CreateMap<CreateCourseDto, Course>()
                .ForMember(d => d.Id, opt => opt.Ignore());
            CreateMap<UpdateCourseDto, Course>()
                .ForMember(d => d.Id, opt => opt.Ignore());

            // Enrollments
            CreateMap<Enrollment, EnrollmentDto>()
                .ForMember(d => d.StudentId, opt => opt.MapFrom(s => s.StudentId))
                .ForMember(d => d.ClassId, opt => opt.MapFrom(s => s.ClassId))
                .ReverseMap();
            CreateMap<CreateEnrollmentDto, Enrollment>()
                .ForMember(d => d.Id, opt => opt.Ignore());
            CreateMap<UpdateEnrollmentDto, Enrollment>()
                .ForMember(d => d.Id, opt => opt.Ignore());

            // Assessments
            CreateMap<Assessment, AssessmentDto>().ReverseMap();
            CreateMap<CreateAssessmentDto, Assessment>()
                .ForMember(d => d.Id, opt => opt.Ignore());
            CreateMap<UpdateAssessmentDto, Assessment>()
                .ForMember(d => d.Id, opt => opt.Ignore());

            // Announcements
            CreateMap<Announcement, AnnouncementDto>().ReverseMap();
            CreateMap<CreateAnnouncementDto, Announcement>()
                .ForMember(d => d.Id, opt => opt.Ignore());
            CreateMap<UpdateAnnouncementDto, Announcement>()
                .ForMember(d => d.Id, opt => opt.Ignore());

            // Auth / Users
            // RegisterUserDto -> User: do NOT map plain password to PasswordHash here; hashing should be handled in the service.
            CreateMap<RegisterUserDto, User>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.PasswordHash, opt => opt.Ignore());

            // LoginRequestDto -> no direct mapping to entity

            // RefreshToken mapping: map expirations and ids; token hashing should be handled in service.
            CreateMap<RefreshTokenDto, RefreshToken>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.TokenHash, opt => opt.Ignore())
                .ForMember(d => d.ExpiresAt, opt => opt.MapFrom(s => s.Expires));
            CreateMap<RefreshToken, RefreshTokenDto>()
                .ForMember(d => d.Expires, opt => opt.MapFrom(s => s.ExpiresAt));
        }
    }
}
