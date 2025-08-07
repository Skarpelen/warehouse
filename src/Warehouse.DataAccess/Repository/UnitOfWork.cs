using Microsoft.EntityFrameworkCore.Storage;

namespace Warehouse.DataAccess.Repository
{
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.DataAccess.Models;

    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly WarehouseContext _context;
        private IDbContextTransaction? _tx;

        public IBalanceRepository Balances { get; }

        public IClientRepository Clients { get; }

        public IResourceRepository Resources { get; }

        public IShipmentDocumentRepository ShipmentDocuments { get; }

        public IShipmentItemRepository ShipmentItems { get; }

        public ISupplyDocumentRepository SupplyDocuments { get; }

        public ISupplyItemRepository SupplyItems { get; }

        public IUnitOfMeasureRepository UnitsOfMeasure { get; }

        public UnitOfWork(WarehouseContext context)
        {
            _context = context;
            Balances = new BalanceRepository(context);
            Clients = new ClientRepository(context);
            Resources = new ResourceRepository(context);
            ShipmentDocuments = new ShipmentDocumentRepository(context);
            ShipmentItems = new ShipmentItemRepository(context);
            SupplyDocuments = new SupplyDocumentRepository(context);
            SupplyItems = new SupplyItemRepository(context);
            UnitsOfMeasure = new UnitOfMeasureRepository(context);
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_tx == null)
            {
                _tx = await _context.Database.BeginTransactionAsync();
            }
        }

        public async Task CommitTransactionAsync()
        {
            if (_tx != null)
            {
                await _tx.CommitAsync();
                await _tx.DisposeAsync();
                _tx = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_tx != null)
            {
                await _tx.RollbackAsync();
                await _tx.DisposeAsync();
                _tx = null;
            }
        }

        public async void ClearTracking()
        {
            _context.ChangeTracker.Clear();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
