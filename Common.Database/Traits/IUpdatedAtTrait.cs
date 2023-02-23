using System;
using Common.Database.Infrastructure;
using Common.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Database.Traits;

public interface IUpdatedAtTrait : IEntityTrait
{
    public DateTime UpdatedAt { get; set; }
}

public sealed class UpdatedAtChangeListener<TEntity> : EntityTraitChangeListener<TEntity, IUpdatedAtTrait>
    where TEntity : class, IUpdatedAtTrait
{
    protected override void OnModelCreating(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasIndex(x => x.UpdatedAt);
        builder.Property(x => x.UpdatedAt).HasDefaultValue(DateTime.UnixEpoch);
    }

    protected override void BeforeModified(EntityChange<TEntity> change)
    {
        base.BeforeModified(change);
        change.Entity.UpdatedAt = DateTime.UtcNow;
    }
}