using Common.Database.Infrastructure;
using Common.Database.Infrastructure.Services;

namespace Common.Database.Services;

public abstract class EntityHostChangeListener<TEntity, TTrait> : EntityChangeListener<TEntity>,
    IEntityHostChangeListener<TEntity, TTrait>
    where TEntity : class, TTrait, new()
    where TTrait : IEntityHost<TEntity>
{
}