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
    public class SubmissionsController : ControllerBase
    {
        private readonly ISubmissionRepository _repo; // quick placeholder; adapt to service if available
        private readonly IMapper _mapper;

        public SubmissionsController(ISubmissionRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var items = await _repo.GetAllAsync();
            return Ok(items);
        }
    }
}
