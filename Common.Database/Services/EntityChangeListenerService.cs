using System;
using System.Collections.Concurrent;
using System.Linq;
using Common.Database.Infrastructure;
using Common.Database.Infrastructure.Extensions;
using Common.Database.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Common.Database.Services
{
    public class EntityChangeListenerService<TContext> : IEntityChangeListenerService<TContext>
        where TContext : DbContext, ICommonDbContext<TContext>
    {
        private readonly IEntityChangeListenerServiceCache _cache;
        private readonly ConcurrentQueue<EntityChange> _entriesCache;
        private readonly IServiceProvider _serviceProvider;
        private TContext _dbContext;

        public EntityChangeListenerService(
            IEntityChangeListenerServiceCache cache, 
            IServiceProvider serviceProvider)
        {
            _cache = cache;
            _serviceProvider = serviceProvider;
            _entriesCache = new ConcurrentQueue<EntityChange>();
        }

        public void PopulateDbContext(TContext context)
        {
            _dbContext = context;
        }
        
        public void OnTracked(object sender, EntityTrackedEventArgs e)
        {
            if (e.Entry.Entity is Entity entity)
            {
                entity.DbContext = _dbContext;
                entity.EntityEntry = e.Entry;
            }
        }
        
        public void OnStateChanged(object sender, EntityStateChangedEventArgs e)
        {
        }

        public void OnModelCreating(TContext dbContext, ModelBuilder modelBuilder)
        {
            var listeners = _cache.GetListeners();
            foreach (var listener in listeners)
            {
                var targetType = listener.TargetType;
                if (dbContext.GetType().GetProperties().Any(p => p.PropertyType == typeof(DbSet<>).MakeGenericType(targetType)))
                {
                    listener.OnModelCreating(modelBuilder);
                }
            }
        }
        
        public void OnSavingChanges(object sender, SavingChangesEventArgs e)
        {
            if (sender is not TContext dbContext) return;
            
            var entries = dbContext.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                if (entry.Entity is IEntity model && 
                    _cache.GetListeners(model.GetType()) is { } listeners)
                {
                    var change = new EntityChange(entry);
                    foreach (var listener in listeners)
                    {
                        listener.BeforeSave(change);
                    }
                    _entriesCache.Enqueue(change);
                }
            }
        }

        public void OnSavedChanges(object sender, SavedChangesEventArgs e)
        {
            if (sender is not TContext dbContext) return;
            
            dbContext.UseNewContextAsync(_serviceProvider, async newContext =>
            {
                var needSaveInNewContext = false;
                
                while (!_entriesCache.IsEmpty)
                {
                    if (_entriesCache.TryDequeue(out var change))
                    {
                        if (change.Entity is IEntity model && 
                            _cache.GetListeners(model.GetType()) is { } listeners)
                            foreach (var listener in listeners)
                            {
                                change.NewContext = newContext;
                                listener.AfterSave(change);
                                needSaveInNewContext |= change.NeedSaveInNewContext;
                            }
                    }
                }

                if (needSaveInNewContext)
                    await newContext.SaveChangesAsync();
            }).RunSync();
        }

        public void OnSaveChangesFailed(object sender, SaveChangesFailedEventArgs e)
        {
            if (sender is not TContext dbContext) return;
            
            dbContext.UseNewContextAsync(_serviceProvider, async newContext =>
            {
                var needSaveInNewContext = false;
                
                while (!_entriesCache.IsEmpty)
                {
                    if (_entriesCache.TryDequeue(out var change))
                    {
                        if (change.Entity is IEntity model &&
                            _cache.GetListeners(model.GetType()) is { } listeners)
                            foreach (var listener in listeners)
                            {
                                change.NewContext = newContext;
                                listener.FailSave(change);
                                needSaveInNewContext |= change.NeedSaveInNewContext;
                            }
                    }
                }

                if (needSaveInNewContext)
                    await newContext.SaveChangesAsync();
            }).RunSync();
        }
    }
}