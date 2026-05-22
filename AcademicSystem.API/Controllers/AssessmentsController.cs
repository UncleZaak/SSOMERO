using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Application.Common.Interfaces.Services;
using AcademicSystem.Application.DTOs.Assessments;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AssessmentsController : ControllerBase
    {
        private readonly IAssessmentService _service;
        private readonly IMapper _mapper;

        public AssessmentsController(IAssessmentService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(200, Type = typeof(IEnumerable<AcademicSystem.Application.DTOs.Assessments.AssessmentDto>))]
        public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAll()
        {
            var items = await _service.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<AssessmentDto>>(items));
        }

        [HttpGet("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(200, Type = typeof(AcademicSystem.Application.DTOs.Assessments.AssessmentDto))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(404, "Not found")]
        public async Task<ActionResult<AssessmentDto>> Get(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<AssessmentDto>(item));
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrInstructor")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(201, Type = typeof(AcademicSystem.Application.DTOs.Assessments.AssessmentDto))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(400, "Bad request")]
        public async Task<ActionResult<AssessmentDto>> Create([FromBody] CreateAssessmentDto dto)
        {
            var entity = _mapper.Map<Domain.Entities.Assessment>(dto);
            var created = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, _mapper.Map<AssessmentDto>(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAssessmentDto dto)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();
            _mapper.Map(dto, existing);
            await _service.UpdateAsync(existing);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
