using AcademicSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicSystem.Infrastructure.Persistence.Configurations
{
    public class ClassCourseConfiguration : IEntityTypeConfiguration<ClassCourse>
    {
        public void Configure(EntityTypeBuilder<ClassCourse> builder)
        {
            builder.ToTable("ClassCourses");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.HasOne(x => x.Class)
                .WithMany(c => c.ClassCourses)
                .HasForeignKey(x => x.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Course)
                .WithMany(c => c.ClassCourses)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Lecturer)
                .WithMany(u => u.ClassCoursesTaught)
                .HasForeignKey(x => x.LecturerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
            builder.HasQueryFilter(cc => !cc.IsDeleted);
        }
    }
}
