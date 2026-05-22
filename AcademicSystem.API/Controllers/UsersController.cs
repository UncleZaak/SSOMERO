using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Application.Common.Interfaces.Services;
using AcademicSystem.Application.DTOs.Users;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IMapper _mapper;

        public UsersController(IUserService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(200, Type = typeof(IEnumerable<AcademicSystem.Application.DTOs.Users.UserDto>))]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var items = await _service.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<UserDto>>(items));
        }

        [HttpGet("{id}")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(200, Type = typeof(AcademicSystem.Application.DTOs.Users.UserDto))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(404, "Not found")]
        public async Task<ActionResult<UserDto>> Get(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<UserDto>(item));
        }

        [HttpPost]
        [AllowAnonymous]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(201, Type = typeof(AcademicSystem.Application.DTOs.Users.UserDto))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(400, "Bad request")]
        public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
        {
            var entity = _mapper.Map<Domain.Entities.User>(dto);
            var created = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, _mapper.Map<UserDto>(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
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
