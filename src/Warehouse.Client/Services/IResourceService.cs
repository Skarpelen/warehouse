namespace Warehouse.Client.Services
{
    using Warehouse.Shared.DTO;

    public readonly record struct GetResourcesResult(ResultCode Result, IEnumerable<ResourceDTO>? Response);
    public readonly record struct GetResourceResult(ResultCode Result, ResourceDTO? Response);

    public interface IResourceService
    {
        Task<GetResourcesResult> GetAllResources(bool includeArchived = false);
        Task<GetResourceResult> GetResource(Guid id);
        Task<ActionResult> CreateResource(ResourceDTO dto);
        Task<ActionResult> UpdateResource(ResourceDTO dto);
        Task<ActionResult> DeleteResource(Guid id);
        Task<ActionResult> ArchiveResource(Guid id);
        Task<ActionResult> UnarchiveResource(Guid id);
    }
}
