using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Application.Common.Interfaces.Services;
using AcademicSystem.Application.DTOs.Courses;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IMapper _mapper;

        public CoursesController(ICourseService courseService, IMapper mapper)
        {
            _courseService = courseService;
            _mapper = mapper;
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(200, Type = typeof(IEnumerable<AcademicSystem.Application.DTOs.Courses.CourseDto>))]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetAll()
        {
            var items = await _courseService.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<CourseDto>>(items));
        }

        [HttpGet("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(200, Type = typeof(AcademicSystem.Application.DTOs.Courses.CourseDto))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(404, "Not found")]
        public async Task<ActionResult<CourseDto>> Get(Guid id)
        {
            var item = await _courseService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<CourseDto>(item));
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(201, Type = typeof(AcademicSystem.Application.DTOs.Courses.CourseDto))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(400, "Bad request")]
        public async Task<ActionResult<CourseDto>> Create([FromBody] CreateCourseDto dto)
        {
            var entity = _mapper.Map<Domain.Entities.Course>(dto);
            var created = await _courseService.CreateAsync(entity);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, _mapper.Map<CourseDto>(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseDto dto)
        {
            var existing = await _courseService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            _mapper.Map(dto, existing);
            await _courseService.UpdateAsync(existing);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _courseService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _courseService.DeleteAsync(id);
            return NoContent();
        }
    }
}
