using System;
using Common.Database.Infrastructure;
using Common.Database.Infrastructure.Extensions;
using Common.Database.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Database.Services;

public abstract class EntityChangeListener<TEntity> : IEntityChangeListener<TEntity>
    where TEntity : class, IEntity
{
    public Type TargetType => typeof(TEntity);

    public virtual void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    public virtual void OnModelCreating(ModelBuilder builder)
    {
        OnModelCreating(builder.Entity<TEntity>());
    }

    public void BeforeSave(EntityChange change)
    {
        if (change.Entity is TEntity) BeforeSave(change.AsEntityChanges<TEntity>());
    }

    public void AfterSave(EntityChange change)
    {
        if (change.Entity is TEntity) AfterSave(change.AsEntityChanges<TEntity>());
    }

    public void FailSave(EntityChange change)
    {
        if (change.Entity is TEntity) FailSave(change.AsEntityChanges<TEntity>());
    }

    public virtual void BeforeSave(EntityChange<TEntity> change)
    {
        if (change.IsAdded)
            BeforeAdded(change);

        if (change.IsModified)
            BeforeModified(change);

        if (change.IsDeleted)
            BeforeDeleted(change);

        if (change.IsUnchanged)
            BeforeUnchanged(change);

        if (change.IsDetached)
            BeforeDetached(change);
    }

    public virtual void AfterSave(EntityChange<TEntity> change)
    {
        if (change.IsAdded)
            AfterAdded(change);

        if (change.IsModified)
            AfterModified(change);

        if (change.IsDeleted)
            AfterDeleted(change);

        if (change.IsUnchanged)
            AfterUnchanged(change);

        if (change.IsDetached)
            AfterDetached(change);
    }

    public virtual void FailSave(EntityChange<TEntity> change)
    {
        switch (change.State)
        {
            case EntityState.Detached:
                FailDetached(change);
                break;
            case EntityState.Unchanged:
                FailUnchanged(change);
                break;
            case EntityState.Deleted:
                FailDeleted(change);
                break;
            case EntityState.Modified:
                FailModified(change);
                break;
            case EntityState.Added:
                FailAdded(change);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(change.State), change.State, null);
        }
    }

    protected virtual void OnModelCreating(EntityTypeBuilder<TEntity> builder)
    {
    }

    protected virtual void BeforeDetached(EntityChange<TEntity> change)
    {
    }

    protected virtual void BeforeUnchanged(EntityChange<TEntity> change)
    {
    }

    protected virtual void BeforeDeleted(EntityChange<TEntity> change)
    {
    }

    protected virtual void BeforeModified(EntityChange<TEntity> change)
    {
    }

    protected virtual void BeforeAdded(EntityChange<TEntity> change)
    {
    }

    protected virtual void AfterDetached(EntityChange<TEntity> change)
    {
    }

    protected virtual void AfterUnchanged(EntityChange<TEntity> change)
    {
    }

    protected virtual void AfterDeleted(EntityChange<TEntity> change)
    {
    }

    protected virtual void AfterModified(EntityChange<TEntity> change)
    {
    }

    protected virtual void AfterAdded(EntityChange<TEntity> change)
    {
    }

    protected virtual void FailDetached(EntityChange<TEntity> change)
    {
    }

    protected virtual void FailUnchanged(EntityChange<TEntity> change)
    {
    }

    protected virtual void FailDeleted(EntityChange<TEntity> change)
    {
    }

    protected virtual void FailModified(EntityChange<TEntity> change)
    {
    }

    protected virtual void FailAdded(EntityChange<TEntity> change)
    {
    }
}