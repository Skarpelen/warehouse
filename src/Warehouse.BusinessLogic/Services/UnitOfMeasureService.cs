using NLog;

namespace Warehouse.BusinessLogic.Services
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.Shared;
    using Warehouse.Shared.Filters;

    public interface IUnitOfMeasureService
    {
        Task<UnitOfMeasure> CreateAsync(UnitOfMeasure newUnit);
        Task<UnitOfMeasure> GetByIdAsync(Guid id);
        Task<IEnumerable<UnitOfMeasure>> GetAllAsync(bool includeArchived = false);
        Task<IEnumerable<UnitOfMeasure>> GetFilteredAsync(UnitFilter filter);
        Task UpdateAsync(Guid id, UnitOfMeasure newUnit);
        Task ArchiveAsync(Guid id);
    }

    public class UnitOfMeasureService(IUnitOfWork unitOfWork) : IUnitOfMeasureService
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public async Task<UnitOfMeasure> CreateAsync(UnitOfMeasure newUnit)
        {
            var exist = await unitOfWork.UnitsOfMeasure.GetAll();

            if (exist.Any(x => x.Name.Equals(newUnit.Name, StringComparison.OrdinalIgnoreCase)))
            {
                _log.Warn($"Duplicate unit '{newUnit.Name}'.");
                throw new InvalidOperationException($"Unit '{newUnit.Name}' already exists.");
            }

            await unitOfWork.UnitsOfMeasure.Add(newUnit);
            await unitOfWork.CompleteAsync();

            _log.Info($"Created unit [Id={newUnit.Id}].");
            return newUnit;
        }

        public async Task<UnitOfMeasure> GetByIdAsync(Guid id)
        {
            var unitOfMeasure = await unitOfWork.UnitsOfMeasure.Get(id);

            if (unitOfMeasure == null)
            {
                _log.Warn($"Unit not found [Id={id}].");
                throw new KeyNotFoundException($"Unit '{id}' not found.");
            }

            return unitOfMeasure;
        }

        public async Task<IEnumerable<UnitOfMeasure>> GetAllAsync(bool includeArchived = false)
        {
            var filter = new UnitFilter { IncludeArchived = includeArchived };
            return await unitOfWork.UnitsOfMeasure.GetAllFiltered(filter);
        }

        public async Task<IEnumerable<UnitOfMeasure>> GetFilteredAsync(UnitFilter filter)
        {
            return await unitOfWork.UnitsOfMeasure.GetAllFiltered(filter);
        }

        public async Task UpdateAsync(Guid id, UnitOfMeasure newUnit)
        {
            var old = await GetByIdAsync(id);

            if (!old.Name.Equals(newUnit.Name, StringComparison.OrdinalIgnoreCase))
            {
                var all = await unitOfWork.UnitsOfMeasure.GetAll();

                if (all.Any(x => x.Name.Equals(newUnit.Name, StringComparison.OrdinalIgnoreCase) && x.Id != id))
                {
                    _log.Warn($"Rename to duplicate '{newUnit.Name}'.");
                    throw new InvalidOperationException($"Unit '{newUnit.Name}' already exists.");
                }
            }

            old.Name = newUnit.Name;
            old.IsDeleted = newUnit.IsDeleted;

            await unitOfWork.UnitsOfMeasure.Update(old);
            await unitOfWork.CompleteAsync();

            _log.Info($"Updated unit [Id={id}].");
        }

        public async Task ArchiveAsync(Guid id)
        {
            var unitOfMeasure = await GetByIdAsync(id);

            if (unitOfMeasure.IsDeleted)
            {
                _log.Warn($"Already archived [Id={id}].");
                return;
            }

            await unitOfWork.UnitsOfMeasure.SoftDelete(id);
            await unitOfWork.CompleteAsync();

            _log.Info($"Archived unit [Id={id}].");
        }
    }
}
