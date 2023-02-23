using System;
using Common.Database.Infrastructure;
using Common.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Database.Traits;

public interface IDeletedAtTrait : IEntityTrait
{
    public DateTime DeletedAt { get; set; }
}

public sealed class DeletedAtChangeListener<TEntity> : EntityTraitChangeListener<TEntity, IDeletedAtTrait>
    where TEntity : class, IDeletedAtTrait
{
    protected override void OnModelCreating(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasIndex(x => x.DeletedAt);
        builder.Property(x => x.DeletedAt).HasDefaultValue(DateTime.UnixEpoch);
    }

    protected override void BeforeDeleted(EntityChange<TEntity> change)
    {
        base.BeforeDeleted(change);
        change.Entity.DeletedAt = DateTime.UtcNow;
    }
}