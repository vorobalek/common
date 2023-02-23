using System.Collections.Generic;
using System.Linq;
using Common.Database.Infrastructure;
using Common.Database.Services;
using Common.Database.Traits;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Database.Traits
{
    public interface ICreatedByTrait<TKey> : IEntityTrait
    {
        public TKey CreatedBy { get; set; }
    }

    public abstract class
        CreatedByChangeListener<TEntity, TKey> : EntityTraitChangeListener<TEntity, ICreatedByTrait<TKey>>
        where TEntity : class, ICreatedByTrait<TKey>
    {
        protected override void OnModelCreating(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasIndex(x => x.CreatedBy);
        }

        protected override void BeforeAdded(EntityChange<TEntity> change)
        {
            base.BeforeAdded(change);
            change.Entity.CreatedBy = GetCreatedBy();
        }

        protected abstract TKey GetCreatedBy();
    }
}


namespace Common.Database.Extensions
{
    public static partial class EntityExtensions
    {
        public static IEnumerable<TEntity> CreatedBy<TEntity, TKey>(this IEnumerable<TEntity> enumerable, TKey key)
            where TEntity : ICreatedByTrait<TKey>
        {
            return enumerable.Where(x => x.CreatedBy != null && x.CreatedBy.Equals(key));
        }

        public static IQueryable<TEntity> CreatedBy<TEntity, TKey>(this IQueryable<TEntity> enumerable, TKey key)
            where TEntity : ICreatedByTrait<TKey>
        {
            return enumerable.Where(x => x.CreatedBy != null && x.CreatedBy.Equals(key));
        }
    }
}