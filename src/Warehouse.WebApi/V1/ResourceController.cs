using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace Warehouse.WebApi.V1
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Services;
    using Warehouse.Shared.DTO;

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class ResourceController(IResourceService resourceService, IMapper mapper)
        : ControllerBase
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [EndpointSummary("Returns all resources")]
        public async Task<ActionResult<IEnumerable<ResourceDTO>>> GetAll([FromQuery] bool includeArchived = false)
        {
            _log.Trace("GetAll Resources (IncludeArchived={IncludeArchived})", includeArchived);
            var list = await resourceService.GetAllAsync(includeArchived);
            var dtos = list.Select(mapper.Map<ResourceDTO>);
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Returns a resource by ID")]
        public async Task<ActionResult<ResourceDTO>> Get(Guid id)
        {
            _log.Trace("Get Resource Id={Id}", id);

            try
            {
                var res = await resourceService.GetByIdAsync(id);
                return Ok(mapper.Map<ResourceDTO>(res));
            }
            catch (KeyNotFoundException)
            {
                _log.Error("Get Resource not found Id={Id}", id);
                return NotFound();
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EndpointSummary("Creates a new resource")]
        public async Task<ActionResult<ResourceDTO>> Create(ResourceDTO dto)
        {
            _log.Trace("Create Resource Name={Name}", dto.Name);
            var model = new Resource
            {
                Name = dto.Name
            };

            try
            {
                var created = await resourceService.CreateAsync(model);
                var result = mapper.Map<ResourceDTO>(created);
                return CreatedAtAction(nameof(Get), new { id = result.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() }, result);
            }
            catch (InvalidOperationException ex)
            {
                _log.Error(ex, "Create Resource failed: duplicate '{Name}'", dto.Name);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Updates a resource")]
        public async Task<ActionResult<ResourceDTO>> Update(Guid id, ResourceDTO dto)
        {
            _log.Trace("Update Resource Id={Id}", id);

            if (id != dto.Id)
            {
                _log.Error("Id mismatch {Route} vs {Body}", id, dto.Id);
                return BadRequest("ID mismatch");
            }

            try
            {
                _ = await resourceService.GetByIdAsync(id);
                var model = new Resource
                {
                    Id = dto.Id,
                    Name = dto.Name
                };

                await resourceService.UpdateAsync(id, model);
                var updated = await resourceService.GetByIdAsync(id);
                return Ok(mapper.Map<ResourceDTO>(updated));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Deletes a resource")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _log.Trace("Delete Resource Id={Id}", id);

            try
            {
                _ = await resourceService.GetByIdAsync(id);
                await resourceService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("archive/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Archives a resource")]
        public async Task<IActionResult> Archive(Guid id)
        {
            _log.Trace("Archive Resource Id={Id}", id);

            try
            {
                _ = await resourceService.GetByIdAsync(id);
                await resourceService.ArchiveAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("unarchive/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Unarchives a resource")]
        public async Task<IActionResult> Unarchive(Guid id)
        {
            _log.Trace("Unarchive Resource Id={Id}", id);

            try
            {
                _ = await resourceService.GetByIdAsync(id);
                await resourceService.UnarchiveAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
