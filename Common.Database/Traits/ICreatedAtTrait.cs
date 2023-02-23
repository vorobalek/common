using System;
using Common.Database.Infrastructure;
using Common.Database.Services;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Database.Traits
{
    public interface ICreatedAtTrait : IEntityTrait
    {
        public DateTime CreatedAt { get; set; }
    }
        
    public sealed class CreatedAtChangeListener<TEntity> : EntityTraitChangeListener<TEntity, ICreatedAtTrait>
        where TEntity : class, ICreatedAtTrait
    {
        protected override void OnModelCreating(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasIndex(x => x.CreatedAt);
        }
        
        protected override void BeforeAdded(EntityChange<TEntity> change)
        {
            base.BeforeAdded(change);
            change.Entity.CreatedAt = DateTime.UtcNow;
        }
    }
}
