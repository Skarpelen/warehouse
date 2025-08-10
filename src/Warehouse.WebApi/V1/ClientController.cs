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
    public class ClientController(IClientService clientService, IMapper mapper)
        : ControllerBase
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [EndpointSummary("Returns all clients")]
        public async Task<ActionResult<IEnumerable<ClientDTO>>> GetClients([FromQuery] bool includeArchived = false)
        {
            _log.Trace("GetClients called (IncludeArchived={IncludeArchived})", includeArchived);
            var clients = await clientService.GetAllAsync(includeArchived);
            var dtos = clients.Select(mapper.Map<ClientDTO>);
            _log.Trace("GetClients returned {Count} clients", dtos.Count());
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Returns a client by ID")]
        public async Task<ActionResult<ClientDTO>> GetClient(Guid id)
        {
            _log.Trace("GetClient called for Id={Id}", id);

            try
            {
                var client = await clientService.GetByIdAsync(id);
                return Ok(mapper.Map<ClientDTO>(client));
            }
            catch (KeyNotFoundException)
            {
                _log.Error("GetClient: id={Id} not found", id);
                return NotFound();
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EndpointSummary("Creates a new client")]
        public async Task<ActionResult<ClientDTO>> CreateClient(ClientDTO dto)
        {
            _log.Trace("CreateClient called with Name={Name}", dto.Name);
            var model = new Client
            {
                Name = dto.Name,
                Address = dto.Address
            };

            try
            {
                var created = await clientService.CreateAsync(model);
                var resultDto = mapper.Map<ClientDTO>(created);
                _log.Trace("CreateClient succeeded for Id={Id}", resultDto.Id);
                return CreatedAtAction(nameof(GetClient), new { id = resultDto.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() }, resultDto);
            }
            catch (InvalidOperationException ex)
            {
                _log.Error(ex, "CreateClient failed: duplicate name '{Name}'", dto.Name);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Updates an existing client")]
        public async Task<ActionResult<ClientDTO>> UpdateClient(Guid id, ClientDTO dto)
        {
            _log.Trace("UpdateClient called for Id={Id}", id);

            if (id != dto.Id)
            {
                _log.Error("UpdateClient id mismatch: routeId={RouteId}, bodyId={BodyId}", id, dto.Id);
                return BadRequest("ID in URL does not match ID in body.");
            }

            try
            {
                _ = await clientService.GetByIdAsync(id);

                var toUpdate = new Client
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Address = dto.Address
                };

                await clientService.UpdateAsync(id, toUpdate);
                var updated = await clientService.GetByIdAsync(id);
                _log.Trace("UpdateClient succeeded for Id={Id}", id);
                return Ok(mapper.Map<ClientDTO>(updated));
            }
            catch (KeyNotFoundException)
            {
                _log.Error("UpdateClient: id={Id} not found", id);
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                _log.Error(ex, "UpdateClient failed for Id={Id}: {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Deletes a client")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _log.Trace("Delete Client Id={Id}", id);

            try
            {
                _ = await clientService.GetByIdAsync(id);
                await clientService.DeleteAsync(id);
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
        [EndpointSummary("Archives a client by ID")]
        public async Task<IActionResult> Archive(Guid id)
        {
            _log.Trace("Archive called for Id={Id}", id);

            try
            {
                _ = await clientService.GetByIdAsync(id);
                await clientService.ArchiveAsync(id);
                _log.Trace("Archive archived Id={Id}", id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                _log.Error("Archive: id={Id} not found", id);
                return NotFound();
            }
        }

        [HttpPost("unarchive/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [EndpointSummary("Unarchives a client by ID")]
        public async Task<IActionResult> Unarchive(Guid id)
        {
            _log.Trace("Unarchive called for Id={Id}", id);

            try
            {
                _ = await clientService.GetByIdAsync(id);
                await clientService.UnarchiveAsync(id);
                _log.Trace("Unarchive Id={Id}", id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                _log.Error("Unarchive: id={Id} not found", id);
                return NotFound();
            }
        }
    }
}
