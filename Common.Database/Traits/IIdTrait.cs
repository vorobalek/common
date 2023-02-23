using System;
using Common.Database.Infrastructure;
using Common.Database.Services;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Database.Traits;

public interface IIdTrait<TKey> : IEntityTrait
    where TKey : IEquatable<TKey>
{
    TKey Id { get; set; }
}

public abstract class IdChangeListener<TEntity, TKey> : EntityTraitChangeListener<TEntity, IIdTrait<TKey>>
    where TEntity : class, IIdTrait<TKey>
    where TKey : IEquatable<TKey>
{
    protected override void OnModelCreating(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);
    }
}