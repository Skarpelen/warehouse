using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace Warehouse.WebApi.V1
{
    using Warehouse.BusinessLogic.Services;
    using Warehouse.BusinessLogic.Models;
    using Warehouse.Shared.DTO;

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class UnitController(IUnitOfMeasureService service, IMapper mapper)
        : ControllerBase
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [EndpointSummary("Returns all units of measure")]
        public async Task<ActionResult<IEnumerable<UnitDTO>>> GetAll([FromQuery] bool includeArchived = false)
        {
            _log.Trace("GetAll Units (IncludeArchived={IncludeArchived})", includeArchived);
            var list = await service.GetAllAsync(includeArchived);
            var dtos = list.Select(mapper.Map<UnitDTO>);
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Returns a unit of measure by ID")]
        public async Task<ActionResult<UnitDTO>> Get(Guid id)
        {
            _log.Trace("Get Unit Id={Id}", id);

            try
            {
                var unit = await service.GetByIdAsync(id);
                return Ok(mapper.Map<UnitDTO>(unit));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EndpointSummary("Creates a new unit of measure")]
        public async Task<ActionResult<UnitDTO>> Create(UnitDTO dto)
        {
            _log.Trace("Create Unit Name={Name}", dto.Name);

            var model = new UnitOfMeasure
            {
                Name = dto.Name
            };

            try
            {
                var created = await service.CreateAsync(model);
                var result = mapper.Map<UnitDTO>(created);
                return CreatedAtAction(nameof(Get), new { id = result.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Updates a unit of measure")]
        public async Task<ActionResult<UnitDTO>> Update(Guid id, UnitDTO dto)
        {
            _log.Trace("Update Unit Id={Id}", id);

            if (id != dto.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                _ = await service.GetByIdAsync(id);
                var model = new UnitOfMeasure
                {
                    Id = dto.Id,
                    Name = dto.Name
                };
                await service.UpdateAsync(id, model);
                var updated = await service.GetByIdAsync(id);
                return Ok(mapper.Map<UnitDTO>(updated));
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Deletes a unit of measure")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _log.Trace("Delete Unit Id={Id}", id);

            try
            {
                _ = await service.GetByIdAsync(id);
                await service.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("archive/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Archives a unit by ID")]
        public async Task<IActionResult> Archive(Guid id)
        {
            _log.Trace("Archive called for Id={Id}", id);

            try
            {
                _ = await service.GetByIdAsync(id);
                await service.ArchiveAsync(id);
                _log.Trace("Unit archived Id={Id}", id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                _log.Error("Archive: Unit id={Id} not found", id);
                return NotFound();
            }
        }

        [HttpPost("unarchive/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Unarchives a unit of measure")]
        public async Task<IActionResult> Unarchive(Guid id)
        {
            _log.Trace("Unarchive Unit Id={Id}", id);

            try
            {
                _ = await service.GetByIdAsync(id);
                await service.UnarchiveAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
