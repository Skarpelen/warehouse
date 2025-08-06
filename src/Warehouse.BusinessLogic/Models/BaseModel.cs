using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.BusinessLogic.Models
{
    /// <summary>
    /// Базовая модель для всех моделей
    /// Предоставляет Id и мягкое удаление
    /// </summary>
    public abstract class BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        [Column("deleted_at")]
        public DateTimeOffset? DeletedAt { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        public void MarkCreated()
        {
            if (CreatedAt != default(DateTimeOffset))
            {
                throw new InvalidOperationException("Entity is already marked created");
            }

            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void MarkUpdated()
        {
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Delete()
        {
            if (IsDeleted)
            {
                throw new InvalidOperationException("Cannot delete a deleted entity.");
            }

            IsDeleted = true;
            DeletedAt = DateTimeOffset.UtcNow;
        }
    }
}
