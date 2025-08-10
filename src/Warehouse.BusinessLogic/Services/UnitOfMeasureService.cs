using NLog;

namespace Warehouse.BusinessLogic.Services
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.BusinessLogic.Repository;
    using Warehouse.Shared.Filters;

    public interface IUnitOfMeasureService
    {
        Task<UnitOfMeasure> CreateAsync(UnitOfMeasure newUnit);
        Task<UnitOfMeasure> GetByIdAsync(Guid id);
        Task<IEnumerable<UnitOfMeasure>> GetAllAsync(bool includeArchived = false);
        Task<IEnumerable<UnitOfMeasure>> GetFilteredAsync(UnitFilter filter);
        Task UpdateAsync(Guid id, UnitOfMeasure newUnit);
        Task DeleteAsync(Guid id);
        Task ArchiveAsync(Guid id);
        Task UnarchiveAsync(Guid id);
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
                throw new InvalidOperationException($"Единица измерения с наименованием '{newUnit.Name}' уже существует.");
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
                throw new KeyNotFoundException($"Единица измерения с идентификатором '{id}' не найдена.");
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
                    throw new InvalidOperationException($"Единица измерения с наименованием '{newUnit.Name}' уже существует.");
                }
            }

            old.Name = newUnit.Name;

            await unitOfWork.UnitsOfMeasure.Update(old);
            await unitOfWork.CompleteAsync();

            _log.Info($"Updated unit [Id={id}].");
        }

        public async Task DeleteAsync(Guid id)
        {
            var unitOfMeasure = await GetByIdAsync(id);

            if (unitOfMeasure.IsDeleted)
            {
                _log.Warn($"Unit already deleted [Id={id}].");
                return;
            }

            var isInUse = await unitOfWork.UnitsOfMeasure.IsInUseAsync(id);

            if (isInUse)
            {
                throw new InvalidOperationException(
                    "Нельзя удалить единицу измерения: она используется в документах. Переведите в архив.");
            }

            await unitOfWork.UnitsOfMeasure.SoftDelete(id);
            await unitOfWork.CompleteAsync();

            _log.Info($"Deleted unit [Id={id}].");
        }

        public async Task ArchiveAsync(Guid id)
        {
            var unitOfMeasure = await GetByIdAsync(id);

            if (unitOfMeasure.IsArchived)
            {
                _log.Warn($"Already archived [Id={id}].");
                return;
            }

            unitOfMeasure.IsArchived = true;
            unitOfMeasure.MarkUpdated();

            await unitOfWork.CompleteAsync();

            _log.Info($"Archived unit [Id={id}].");
        }

        public async Task UnarchiveAsync(Guid id)
        {
            var unitOfMeasure = await GetByIdAsync(id);

            if (!unitOfMeasure.IsArchived)
            {
                _log.Warn($"Unit already unarchived [Id={id}].");
                return;
            }

            unitOfMeasure.IsArchived = false;
            unitOfMeasure.MarkUpdated();

            await unitOfWork.CompleteAsync();

            _log.Info($"Unarchived unitOfMeasure [Id={id}].");
        }
    }
}
