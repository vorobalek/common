using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using Common.Database.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;

namespace Common.Database
{
    public abstract class Entity : IEntity
    {
        [JsonIgnore, NotMapped]
        public DbContext DbContext { get; internal set; }
        
        [JsonIgnore, NotMapped]
        public EntityEntry EntityEntry { get; internal set; }

        private ILazyLoader _lazyLoader;
        private ILazyLoader LazyLoader
        {
            get => _lazyLoader ??= DbContext?.GetService<ILazyLoader>();
            set => _lazyLoader = value;
        }
        
        protected TRelated Lazy<TRelated>(
            ref TRelated navigationField,
            [CallerMemberName] string navigationName = null) 
            where TRelated : class
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return navigationField ??= LazyLoader?.Load(this, ref navigationField, navigationName);
        }

        public abstract void Update(DbContext dbContext = null);

        public abstract void Remove(DbContext dbContext = null);
    }

    public abstract class Entity<TEntity> : Entity
        where TEntity : Entity<TEntity>
    {
        public sealed override void Update(DbContext dbContext = null)
        {
            var context = dbContext ?? this.DbContext;
            context.Update(this);
        }

        public sealed override void Remove(DbContext dbContext = null)
        {
            var context = dbContext ?? this.DbContext;
            BeforeRemove(context);
            if (CanBeRemoved(context))
                context.Remove(this);
        }

        protected virtual void BeforeRemove(DbContext context)
        {
        }

        protected virtual bool CanBeRemoved(DbContext context) => 
            context?.Set<TEntity>()?.Local?.Contains(this) ?? false;
    }
}