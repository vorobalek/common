using Common.Database.Infrastructure;
using Common.Database.Infrastructure.Services;

namespace Common.Database.Services;

public abstract class EntityTraitChangeListener<TEntity, TTrait> : EntityChangeListener<TEntity>,
    IEntityTraitChangeListener<TEntity, TTrait>
    where TEntity : class, TTrait
    where TTrait : IEntityTrait
{
}