using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Application.Common.Interfaces.Services;
using AcademicSystem.Application.DTOs.Students;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IMapper _mapper;

        public StudentsController(IStudentService studentService, IMapper mapper)
        {
            _studentService = studentService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all students
        /// </summary>
        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(200, Type = typeof(IEnumerable<AcademicSystem.Application.DTOs.Students.StudentDto>))]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetAll()
        {
            var students = await _studentService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<StudentDto>>(students);
            return Ok(dtos);
        }

        /// <summary>
        /// Get student by id
        /// </summary>
        [HttpGet("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(200, Type = typeof(AcademicSystem.Application.DTOs.Students.StudentDto))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(404, "Not found")]
        public async Task<ActionResult<StudentDto>> GetStudent(Guid id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student == null) return NotFound();
            var dto = _mapper.Map<StudentDto>(student);
            return Ok(dto);
        }

        /// <summary>
        /// Create a new student
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(201, Type = typeof(AcademicSystem.Application.DTOs.Students.StudentDto))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(400, "Bad request")]
        public async Task<ActionResult<StudentDto>> CreateStudent([FromBody] CreateStudentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = _mapper.Map<Domain.Entities.Student>(dto);
            var created = await _studentService.CreateAsync(entity);
            var resultDto = _mapper.Map<StudentDto>(created);
            return CreatedAtAction(nameof(GetStudent), new { id = resultDto.Id }, resultDto);
        }

        /// <summary>
        /// Update an existing student
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(204, "No Content")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(404, "Not found")]
        public async Task<IActionResult> UpdateStudent(Guid id, [FromBody] UpdateStudentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _studentService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            // Map updated fields into existing entity
            _mapper.Map(dto, existing);
            await _studentService.UpdateAsync(existing);
            return NoContent();
        }

        /// <summary>
        /// Delete a student
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(204, "No Content")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(404, "Not found")]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            var existing = await _studentService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _studentService.DeleteAsync(id);
            return NoContent();
        }
    }
}
