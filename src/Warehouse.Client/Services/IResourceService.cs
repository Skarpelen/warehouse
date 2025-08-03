namespace Warehouse.Client.Services
{
    using Warehouse.Shared.DTO;

    public readonly record struct GetResourcesResult(ResultCode Result, IEnumerable<ResourceDTO>? Response);
    public readonly record struct GetResourceResult(ResultCode Result, ResourceDTO? Response);
    public readonly record struct CreateResourceResult(ResultCode Result, ResourceDTO? Resource);

    public interface IResourceService
    {
        Task<GetResourcesResult> GetAllResources(bool includeArchived = false);
        Task<GetResourceResult> GetResource(Guid id);
        Task<CreateResourceResult> Create(ResourceDTO dto);
        Task<ActionResult> Update(ResourceDTO dto);
        Task<ActionResult> Archive(Guid id);
    }
}
