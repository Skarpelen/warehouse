namespace Warehouse.Client.Services
{
    using Warehouse.Shared.DTO;

    public readonly record struct GetClientsResult(ResultCode Result, IEnumerable<ClientDTO>? Response);
    public readonly record struct GetClientResult(ResultCode Result, ClientDTO? Response);
    public readonly record struct CreateClientResult(ResultCode Result, ClientDTO? Client);

    public interface IClientService
    {
        Task<GetClientsResult> GetClients(bool includeArchived = false);
        Task<GetClientResult> GetClient(Guid id);
        Task<CreateClientResult> CreateClient(ClientDTO dto);
        Task<ActionResult> UpdateClient(ClientDTO dto);
        Task<ActionResult> DeleteClient(Guid id);
    }
}
