using System;
using System.Collections.Generic;
using Common.Database.Infrastructure;
using Common.Database.Infrastructure.Services;
using Common.Database.Services;
using Common.Database.Traits;
using Microsoft.EntityFrameworkCore;

namespace Common.Database.Hosts;

public interface IVersionHost<THost, TKey, TModel> : IEntityHost<THost>
    where THost : Entity<THost>, IVersionHost<THost, TKey, TModel>, IIdTrait<TKey>, new()
    where TKey : IEquatable<TKey>
    where TModel : Entity<TModel>, IVersionModelHost<THost, TKey, TModel>, new()
{
    ICollection<TModel> Versions { get; }
}

public sealed class
    VersionHostChangeListener<THost, TKey, TModel> : EntityHostChangeListener<THost, IVersionHost<THost, TKey, TModel>>
    where THost : Entity<THost>, IVersionHost<THost, TKey, TModel>, IIdTrait<TKey>, new()
    where TKey : IEquatable<TKey>
    where TModel : Entity<TModel>, IVersionModelHost<THost, TKey, TModel>, new()
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityChangeListenerServiceCache _changeListenerServiceCache;

    public VersionHostChangeListener(
        IServiceProvider serviceProvider,
        IEntityChangeListenerServiceCache changeListenerServiceCache)
    {
        _serviceProvider = serviceProvider;
        _changeListenerServiceCache = changeListenerServiceCache;
    }

    public override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var listener in _changeListenerServiceCache.GetListeners<TModel>()) listener.OnModelCreating(builder);
    }

    public override void AfterSave(EntityChange<THost> change)
    {
        base.AfterSave(change);

        if (change.IsAdded || change.IsModified)
        {
            var version = new TModel
            {
                EntityId = change.Entity.Id,
                Serialized = change.Entity
            };
            change.NewContext.Add(version);
            change.NeedSaveInNewContext = true;
        }
    }
}