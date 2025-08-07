using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace Warehouse.WebApi.V1
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Services;
    using Warehouse.Shared;
    using Warehouse.Shared.DTO;
    using Warehouse.Shared.Filters;

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class ShipmentController(IShipmentDocumentService service, IMapper mapper)
        : ControllerBase
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [EndpointSummary("Returns all shipments with filtering support")]
        public async Task<ActionResult<IEnumerable<ShipmentDocumentDTO>>> GetAll([FromQuery] DocumentFilter filter)
        {
            _log.Trace("GetAll Shipments Filter={@Filter}", filter);
            var list = await service.GetFilteredAsync(filter);
            var dtos = list.Select(mapper.Map<ShipmentDocumentDTO>);
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Returns a shipment by ID")]
        public async Task<ActionResult<ShipmentDocumentDTO>> Get(Guid id)
        {
            _log.Trace("Get Shipment Id={Id}", id);

            try
            {
                var doc = await service.GetByIdAsync(id);
                return Ok(mapper.Map<ShipmentDocumentDTO>(doc));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EndpointSummary("Creates a new shipment")]
        public async Task<ActionResult<ShipmentDocumentDTO>> Create(ShipmentDocumentDTO dto)
        {
            _log.Trace("Create Shipment Number={Number}", dto.Number);
            var model = mapper.Map<ShipmentDocument>(dto);

            try
            {
                var created = await service.CreateAsync(model);
                var result = mapper.Map<ShipmentDocumentDTO>(created);
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
        [EndpointSummary("Updates an existing shipment")]
        public async Task<ActionResult<ShipmentDocumentDTO>> Update(Guid id, ShipmentDocumentDTO dto)
        {
            _log.Trace("Update Shipment Id={Id}", id);

            if (id != dto.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                _ = await service.GetByIdAsync(id, false);
                var model = mapper.Map<ShipmentDocument>(dto);
                await service.UpdateAsync(id, model);
                var updated = await service.GetByIdAsync(id);
                return Ok(mapper.Map<ShipmentDocumentDTO>(updated));
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
        [EndpointSummary("Soft-deletes a shipment by revoking if needed")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _log.Trace("Delete Shipment Id={Id}", id);

            try
            {
                await service.ChangeStatusAsync(id, ShipmentStatus.Revoked);
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

        [HttpPost("{id:guid}/sign")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EndpointSummary("Signs the shipment")]
        public async Task<IActionResult> Sign(Guid id)
        {
            _log.Trace("Sign Shipment Id={Id}", id);

            try
            {
                await service.ChangeStatusAsync(id, ShipmentStatus.Signed);
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

        [HttpPost("{id:guid}/revoke")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EndpointSummary("Revokes the shipment")]
        public async Task<IActionResult> Revoke(Guid id)
        {
            _log.Trace("Revoke Shipment Id={Id}", id);

            try
            {
                await service.ChangeStatusAsync(id, ShipmentStatus.Revoked);
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
    }
}
