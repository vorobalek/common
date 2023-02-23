namespace Common.Database.Infrastructure.Services;

public interface IEntityTraitChangeListener<TEntity, TTrait> : IEntityChangeListener<TEntity>
    where TEntity : class, TTrait
    where TTrait : IEntityTrait
{
}