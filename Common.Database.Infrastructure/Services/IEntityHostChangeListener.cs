namespace Common.Database.Infrastructure.Services;

public interface IEntityHostChangeListener<TEntity, TTrait> : IEntityChangeListener<TEntity>
    where TEntity : class, TTrait, new()
    where TTrait : IEntityHost<TEntity>
{
}