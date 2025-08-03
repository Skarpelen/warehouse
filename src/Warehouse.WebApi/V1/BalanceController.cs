using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace Warehouse.WebApi.V1
{
    using Warehouse.BusinessLogic.Services;
    using Warehouse.Shared.DTO;
    using Warehouse.Shared.Filters;

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class BalanceController(IBalanceService balanceService, IMapper mapper)
        : ControllerBase
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [EndpointSummary("Returns all balances with filtering support")]
        public async Task<ActionResult<IEnumerable<BalanceDTO>>> GetAll([FromQuery] BalanceFilter filter)
        {
            _log.Trace("GetAll Balances Filter={@Filter}", filter);
            var list = await balanceService.GetAllAsync(filter);
            var dtos = list.Select(mapper.Map<BalanceDTO>);
            return Ok(dtos);
        }
    }
}
