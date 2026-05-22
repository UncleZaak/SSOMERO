using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AcademicSystem.Application.DTOs.Courses;
using AcademicSystem.Application.Common.Interfaces.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AcademicClassesController : ControllerBase
    {
        private readonly ICourseService _courseService; // placeholder: adapt if there's an IAcademicClassService
        private readonly IMapper _mapper;

        public AcademicClassesController(ICourseService courseService, IMapper mapper)
        {
            _courseService = courseService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetAll()
        {
            var items = await _courseService.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<CourseDto>>(items));
        }
    }
}
