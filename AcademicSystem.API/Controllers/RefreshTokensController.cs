using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Application.Common.Interfaces.Services;
using AcademicSystem.Application.DTOs.Auth;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RefreshTokensController : ControllerBase
    {
        private readonly IRefreshTokenService _service;
        private readonly IMapper _mapper;

        public RefreshTokensController(IRefreshTokenService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(200, Type = typeof(IEnumerable<AcademicSystem.Application.DTOs.Auth.RefreshTokenDto>))]
        public async Task<ActionResult<IEnumerable<RefreshTokenDto>>> GetAll()
        {
            var items = await _service.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<RefreshTokenDto>>(items));
        }

        [HttpGet("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(200, Type = typeof(AcademicSystem.Application.DTOs.Auth.RefreshTokenDto))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(404, "Not found")]
        public async Task<ActionResult<RefreshTokenDto>> Get(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<RefreshTokenDto>(item));
        }

        [HttpPost]
        public async Task<ActionResult<RefreshTokenDto>> Create([FromBody] RefreshTokenDto dto)
        {
            var entity = _mapper.Map<Domain.Entities.RefreshToken>(dto);
            var created = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, _mapper.Map<RefreshTokenDto>(created));
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
