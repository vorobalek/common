using System;
using System.Collections.Generic;
using System.Linq;
using Common.Database.Infrastructure;
using Common.Database.Services;
using Common.Database.Traits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Database.Traits
{
    public interface IDeletedTrait : IEntityTrait
    {
        bool ForceDeleted { get; set; }
        bool Deleted { get; set; }
    }

    public sealed class DeletedChangeListener<TEntity> : EntityTraitChangeListener<TEntity, IDeletedTrait>
        where TEntity : class, IDeletedTrait
    {
        protected override void OnModelCreating(EntityTypeBuilder<TEntity> builder)
        {
            base.OnModelCreating(builder);

            builder
                .Ignore(e => e.ForceDeleted);
        }

        protected override void BeforeDeleted(EntityChange<TEntity> change)
        {
            base.BeforeDeleted(change);

            if (change.Entity.ForceDeleted) return;
            
            change.Entity.Deleted = true;
            change.EntityEntry.State = EntityState.Modified;
        }
    }
}

namespace Common.Database.Extensions
{
    public static partial class EntityExtensions
    {
        public static TEntity MarkAsForceDeleted<TEntity>(this TEntity entity)
            where TEntity : IDeletedTrait
        {
            entity.ForceDeleted = true;
            return entity;
        }

        public static IEnumerable<TEntity> MarkAsForceDeleted<TEntity>(this IEnumerable<TEntity> enumerable)
            where TEntity : IDeletedTrait
        {
            enumerable ??= Array.Empty<TEntity>();
            var entities = enumerable as TEntity[] ?? enumerable.ToArray();
            foreach (var entity in entities)
            {
                entity.MarkAsForceDeleted();
            }
            return entities;
        }
        
        public static IEnumerable<TEntity> NotDeleted<TEntity>(this IEnumerable<TEntity> enumerable)
            where TEntity : IDeletedTrait
        {
            return enumerable.Where(x => !x.Deleted);
        }
        
        public static IQueryable<TEntity> NotDeleted<TEntity>(this IQueryable<TEntity> queryable)
            where TEntity : IDeletedTrait
        {
            return queryable.Where(x => !x.Deleted);
        }
    }
}