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
    using Warehouse.Shared.Filters;

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class SupplyController(ISupplyDocumentService supplyService, IMapper mapper)
        : ControllerBase
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [EndpointSummary("Returns all supplies with filtering support")]
        public async Task<ActionResult<IEnumerable<SupplyDocumentDTO>>> GetAll([FromQuery] DocumentFilter filter)
        {
            _log.Trace("GetAll Supplies Filter={@Filter}", filter);
            var list = await supplyService.GetFilteredAsync(filter);
            var dtos = list.Select(mapper.Map<SupplyDocumentDTO>);
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Returns a supply by ID")]
        public async Task<ActionResult<SupplyDocumentDTO>> Get(Guid id)
        {
            _log.Trace("Get Supply Id={Id}", id);

            try
            {
                var doc = await supplyService.GetByIdAsync(id, false);
                return Ok(mapper.Map<SupplyDocumentDTO>(doc));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EndpointSummary("Creates a new supply")]
        public async Task<ActionResult<SupplyDocumentDTO>> Create(SupplyDocumentDTO dto)
        {
            _log.Trace("Create Supply Number={Number}", dto.Number);
            var model = mapper.Map<SupplyDocument>(dto);

            try
            {
                var created = await supplyService.CreateAsync(model);
                var result = mapper.Map<SupplyDocumentDTO>(created);
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
        [EndpointSummary("Updates an existing supply")]
        public async Task<ActionResult<SupplyDocumentDTO>> Update(Guid id, SupplyDocumentDTO dto)
        {
            _log.Trace("Update Supply Id={Id}", id);

            if (id != dto.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                _ = await supplyService.GetByIdAsync(id, false);
                var model = mapper.Map<SupplyDocument>(dto);
                await supplyService.UpdateAsync(id, model);
                var updated = await supplyService.GetByIdAsync(id, false);
                return Ok(mapper.Map<SupplyDocumentDTO>(updated));
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
        [EndpointSummary("Deletes (soft) a supply by ID")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _log.Trace("Delete Supply Id={Id}", id);

            try
            {
                await supplyService.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
