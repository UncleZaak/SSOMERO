using System;
using System.Threading.Tasks;
using AcademicSystem.Application.Common.Interfaces;
using AcademicSystem.Application.DTOs.Auth;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public AuthController(IAuthService authService, IMapper mapper)
        {
            _authService = authService;
            _mapper = mapper;
        }

        [HttpPost("login")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(200, Type = typeof(AcademicSystem.Application.DTOs.Auth.AuthResponseDto))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(401, "Unauthorized")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var resp = await _authService.LoginAsync(dto);
                return Ok(resp);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        [HttpPost("register")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(200, Type = typeof(AcademicSystem.Application.DTOs.Auth.AuthResponseDto))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(400, "Bad Request")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var resp = await _authService.RegisterAsync(dto);
                return Ok(resp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("refresh")]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(200, Type = typeof(AcademicSystem.Application.DTOs.Auth.AuthResponseDto))]
        [Swashbuckle.AspNetCore.Annotations.SwaggerResponse(401, "Unauthorized")]
        public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var resp = await _authService.RefreshTokenAsync(dto.RefreshToken);
                return Ok(resp);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }
    }
}
