using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.Database.Infrastructure;
using Common.Database.Infrastructure.Extensions;
using Common.Database.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Database.Services;

public class EntityChangeListenerService<TContext> : IEntityChangeListenerService<TContext>
    where TContext : DbContext, ICommonDbContext<TContext>
{
    private readonly ConcurrentQueue<EntityChange> _entriesCache;
    private readonly IEnumerable<IEntityChangeListener> _listeners;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private TContext? _dbContext;

    public EntityChangeListenerService(
        IEnumerable<IEntityChangeListener> listeners,
        IServiceScopeFactory serviceScopeFactory)
    {
        _listeners = listeners;
        _serviceScopeFactory = serviceScopeFactory;
        _entriesCache = new ConcurrentQueue<EntityChange>();
    }

    public void PopulateDbContext(TContext context)
    {
        _dbContext = context;
    }

    public void OnTracked(object? sender, EntityTrackedEventArgs e)
    {
        if (e.Entry.Entity is Entity entity)
        {
            entity.DbContext = _dbContext;
            entity.EntityEntry = e.Entry;
        }
    }

    public void OnStateChanged(object? sender, EntityStateChangedEventArgs e)
    {
    }

    public void OnModelCreating(TContext dbContext, ModelBuilder modelBuilder)
    {
        foreach (var listener in _listeners)
        {
            var targetType = listener.TargetType;
            if (dbContext.GetType().GetProperties()
                .Any(p => p.PropertyType == typeof(DbSet<>).MakeGenericType(targetType)))
                listener.OnModelCreating(modelBuilder);
        }
    }

    public void OnSavingChanges(object? sender, SavingChangesEventArgs e)
    {
        if (sender is not TContext dbContext) return;

        var entries = dbContext.ChangeTracker.Entries().ToList();
        foreach (var entry in entries)
            if (entry.Entity is IEntity model &&
                _listeners.Where(listener => listener.TargetType == model.GetType()) is { } listeners)
            {
                var change = new EntityChange(entry);
                foreach (var listener in listeners) listener.BeforeSave(change);
                _entriesCache.Enqueue(change);
            }
    }

    public void OnSavedChanges(object? sender, SavedChangesEventArgs e)
    {
        if (sender is not TContext dbContext) return;

        dbContext.UseNewContextAsync(_serviceScopeFactory, async newContext =>
        {
            var needSaveInNewContext = false;

            while (!_entriesCache.IsEmpty)
                if (_entriesCache.TryDequeue(out var change))
                    if (change.Entity is IEntity model &&
                        _listeners.Where(listener => listener.TargetType == model.GetType()) is { } listeners)
                        foreach (var listener in listeners)
                        {
                            change.NewContext = newContext;
                            listener.AfterSave(change);
                            needSaveInNewContext |= change.NeedSaveInNewContext;
                        }

            if (needSaveInNewContext)
                await newContext.SaveChangesAsync().ConfigureAwait(false);
        }).RunSync();
    }

    public void OnSaveChangesFailed(object? sender, SaveChangesFailedEventArgs e)
    {
        if (sender is not TContext dbContext) return;

        dbContext.UseNewContextAsync(_serviceScopeFactory, async newContext =>
        {
            var needSaveInNewContext = false;

            while (!_entriesCache.IsEmpty)
                if (_entriesCache.TryDequeue(out var change))
                    if (change.Entity is IEntity model &&
                        _listeners.Where(listener => listener.TargetType == model.GetType()) is { } listeners)
                        foreach (var listener in listeners)
                        {
                            change.NewContext = newContext;
                            listener.FailSave(change);
                            needSaveInNewContext |= change.NeedSaveInNewContext;
                        }

            if (needSaveInNewContext)
                await newContext.SaveChangesAsync().ConfigureAwait(false);
        }).RunSync();
    }
}