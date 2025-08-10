namespace Warehouse.Client.Services
{
    using Warehouse.Shared.DTO;

    public readonly record struct GetClientsResult(ResultCode Result, IEnumerable<ClientDTO>? Response);
    public readonly record struct GetClientResult(ResultCode Result, ClientDTO? Response);

    public interface IClientService
    {
        Task<GetClientsResult> GetClients(bool includeArchived = false);
        Task<GetClientResult> GetClient(Guid id);
        Task<ActionResult> CreateClient(ClientDTO dto);
        Task<ActionResult> UpdateClient(ClientDTO dto);
        Task<ActionResult> DeleteClient(Guid id);
        Task<ActionResult> ArchiveClient(Guid id);
        Task<ActionResult> UnarchiveClient(Guid id);
    }
}
