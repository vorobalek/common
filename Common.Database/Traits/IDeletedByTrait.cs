using System.Collections.Generic;
using System.Linq;
using Common.Database.Infrastructure;
using Common.Database.Services;
using Common.Database.Traits;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Database.Traits
{
    public interface IDeletedByTrait<TKey> : IEntityTrait
    {
        public TKey DeletedBy { get; set; }
    }

    public abstract class
        DeletedByChangeListener<TEntity, TKey> : EntityTraitChangeListener<TEntity, IDeletedByTrait<TKey>>
        where TEntity : class, IDeletedByTrait<TKey>
    {
        protected override void OnModelCreating(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasIndex(x => x.DeletedBy);
        }

        protected override void BeforeDeleted(EntityChange<TEntity> change)
        {
            base.BeforeDeleted(change);
            change.Entity.DeletedBy = GetDeletedBy();
        }

        protected abstract TKey GetDeletedBy();
    }
}

namespace Common.Database.Extensions
{
    public static partial class EntityExtensions
    {
        public static IEnumerable<TEntity> DeletedBy<TEntity, TKey>(this IEnumerable<TEntity> enumerable, TKey key)
            where TEntity : IDeletedByTrait<TKey>
        {
            return enumerable.Where(x => x.DeletedBy != null && x.DeletedBy.Equals(key));
        }

        public static IQueryable<TEntity> DeletedBy<TEntity, TKey>(this IQueryable<TEntity> enumerable, TKey key)
            where TEntity : IDeletedByTrait<TKey>
        {
            return enumerable.Where(x => x.DeletedBy != null && x.DeletedBy.Equals(key));
        }
    }
}