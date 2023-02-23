namespace Common.Database.Infrastructure
{
    public interface IEntityHost<TEntity> : IEntity
        where TEntity : class, IEntityHost<TEntity>, new()
    {
    }
}