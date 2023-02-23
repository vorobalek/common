using System;
using Microsoft.EntityFrameworkCore;

namespace Common.Database.Infrastructure.Services;

public interface IEntityChangeListener
{
    Type TargetType { get; }

    void OnModelCreating(ModelBuilder builder);

    void BeforeSave(EntityChange change);

    void AfterSave(EntityChange change);

    void FailSave(EntityChange change);
}

public interface IEntityChangeListener<TEntity> : IEntityChangeListener
    where TEntity : class, IEntity
{
    void BeforeSave(EntityChange<TEntity> change);

    void AfterSave(EntityChange<TEntity> change);

    void FailSave(EntityChange<TEntity> change);
}