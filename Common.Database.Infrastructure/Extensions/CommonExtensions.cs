namespace Common.Database.Infrastructure.Extensions
{
    public static class CommonExtensions
    {
        
        public static EntityChange<TEntity> AsEntityChanges<TEntity>(this EntityChange entityChanges) 
            where TEntity : class, IEntity
            => entityChanges is EntityChange<TEntity> result
                ? result
                : new EntityChange<TEntity>(entityChanges);


        public static PropertyChange<TEntity> AsPropertyChange<TEntity>(this PropertyChange propertyChange)
            => propertyChange is PropertyChange<TEntity> result
                ? result
                : new PropertyChange<TEntity>(propertyChange);
    }
}