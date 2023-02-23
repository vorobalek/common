using System.Collections.Generic;
using System.Linq;
using Common.Database.Infrastructure;
using Common.Database.Services;
using Common.Database.Traits;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Database.Traits
{
    public interface IUpdatedByTrait<TKey> : IEntityTrait
    {
        public TKey UpdatedBy { get; set; }
    }

    public abstract class UpdatedByChangeListener<TEntity, TKey> : EntityTraitChangeListener<TEntity, IUpdatedByTrait<TKey>>
        where TEntity : class, IUpdatedByTrait<TKey>
    {
        protected override void OnModelCreating(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasIndex(x => x.UpdatedBy);
        }
        
        protected override void BeforeModified(EntityChange<TEntity> change)
        {
            base.BeforeModified(change);
            change.Entity.UpdatedBy = GetUpdatedBy();
        }
        
        protected abstract TKey GetUpdatedBy();
    }
}

namespace Common.Database.Extensions
{
    public static partial class EntityExtensions
    {
        public static IEnumerable<TEntity> UpdatedBy<TEntity, TKey>(this IEnumerable<TEntity> enumerable, TKey key)
            where TEntity : IUpdatedByTrait<TKey>
        {
            return enumerable.Where(x => x.UpdatedBy.Equals(key));
        }
        
        public static IQueryable<TEntity> UpdatedBy<TEntity, TKey>(this IQueryable<TEntity> enumerable, TKey key)
            where TEntity : IUpdatedByTrait<TKey>
        {
            return enumerable.Where(x => x.UpdatedBy.Equals(key));
        }
    }
}