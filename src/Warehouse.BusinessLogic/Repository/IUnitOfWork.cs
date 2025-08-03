namespace Warehouse.BusinessLogic.Repository
{
    public interface IUnitOfWork
    {
        IBalanceRepository Balances { get; }

        IClientRepository Clients { get; }

        IResourceRepository Resources { get; }

        IShipmentDocumentRepository ShipmentDocuments { get; }

        IShipmentItemRepository ShipmentItems { get; }

        ISupplyDocumentRepository SupplyDocuments { get; }

        ISupplyItemRepository SupplyItems { get; }

        IUnitOfMeasureRepository UnitsOfMeasure { get; }

        Task CompleteAsync();
    }
}
