using System.Collections.Generic;
using System.Threading.Tasks;
using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface ICoursesService
{
    Task<IEnumerable<CourseDto>> GetCoursesAsync();
    Task<CourseDto?> GetCourseAsync(string id);
}