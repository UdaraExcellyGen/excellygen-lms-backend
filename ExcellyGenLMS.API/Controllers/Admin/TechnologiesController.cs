using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,ProjectManager")]  // Updated to include ProjectManager role
    public class TechnologiesController : ControllerBase
    {
        private readonly ITechnologyService _technologyService;

        public TechnologiesController(ITechnologyService technologyService)
        {
            _technologyService = technologyService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TechnologyDto>>> GetAllTechnologies()
        {
            var technologies = await _technologyService.GetAllTechnologiesAsync();
            return Ok(technologies);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TechnologyDto>> GetTechnologyById(string id)
        {
            try
            {
                var technology = await _technologyService.GetTechnologyByIdAsync(id);
                return Ok(technology);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<TechnologyDto>> CreateTechnology([FromBody] CreateTechnologyDto createTechnologyDto)
        {
            try
            {
                var technology = await _technologyService.CreateTechnologyAsync(createTechnologyDto);
                return CreatedAtAction(nameof(GetTechnologyById), new { id = technology.Id }, technology);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TechnologyDto>> UpdateTechnology(string id, [FromBody] UpdateTechnologyDto updateTechnologyDto)
        {
            try
            {
                var technology = await _technologyService.UpdateTechnologyAsync(id, updateTechnologyDto);
                return Ok(technology);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTechnology(string id)
        {
            try
            {
                await _technologyService.DeleteTechnologyAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<TechnologyDto>> ToggleTechnologyStatus(string id)
        {
            try
            {
                var technology = await _technologyService.ToggleTechnologyStatusAsync(id);
                return Ok(technology);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}