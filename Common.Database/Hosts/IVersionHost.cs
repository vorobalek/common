using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
}

public sealed class
    VersionHostChangeListener<THost, TKey, TModel> : EntityHostChangeListener<THost,
    IVersionHost<THost, TKey, TModel>>
    where THost : Entity<THost>, IVersionHost<THost, TKey, TModel>, IIdTrait<TKey>, new()
    where TKey : IEquatable<TKey>
    where TModel : Entity<TModel>, IVersionModelHost<THost, TKey, TModel>, new()
{
    private readonly IEnumerable<IEntityChangeListener<TModel>> _listeners;

    public VersionHostChangeListener(IEnumerable<IEntityChangeListener<TModel>> listeners)
    {
        _listeners = listeners;
    }

    public override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var listener in _listeners) listener.OnModelCreating(builder);
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

public static class EntityExtensions
{
    public static TModel? GetPreviousVersion<THost, TKey, TModel>(this THost host, DbContext? dbContext = null)
        where THost : Entity<THost>, IVersionHost<THost, TKey, TModel>, IIdTrait<TKey>, new()
        where TModel : Entity<TModel>, IVersionModelHost<THost, TKey, TModel>, new()
        where TKey : IEquatable<TKey>
    {
        var context = dbContext
                      ?? host.DbContext
                      ?? throw new InvalidOperationException("Unable to access DbContext");

        return context
            .Set<TModel>()
            .Where(e => e.EntityId.Equals(host.Id))
            .OrderByDescending(e => e.Number)
            .Skip(1)
            .FirstOrDefault();
    }

    public static THost? RestorePreviousVersion<THost, TKey, TModel>(this THost host, DbContext? dbContext = null)
        where THost : Entity<THost>, IVersionHost<THost, TKey, TModel>, IIdTrait<TKey>, new()
        where TModel : Entity<TModel>, IVersionModelHost<THost, TKey, TModel>, new()
        where TKey : IEquatable<TKey>
    {
        var context = dbContext
                      ?? host.DbContext
                      ?? throw new InvalidOperationException("Unable to access DbContext");

        var previousVersionModel = host.GetPreviousVersion<THost, TKey, TModel>(dbContext);

        if (previousVersionModel is null) return null;

        var previousVersionHost = previousVersionModel.Serialized;

        if (previousVersionHost is null)
            throw new InvalidOperationException("Unable to deserialize entity version")
            {
                Data =
                {
                    [nameof(previousVersionModel.Serialized)] = previousVersionModel
                        .EntityEntry
                        ?.Property(nameof(previousVersionModel.Serialized))
                        .OriginalValue
                }
            };

        context.Entry(host).CurrentValues.SetValues(previousVersionHost);
        context.SaveChanges();

        return previousVersionHost;
    }


    public static Task<TModel?> GetPreviousVersionAsync<THost, TKey, TModel>(this THost host,
        DbContext? dbContext = null, CancellationToken cancellationToken = default)
        where THost : Entity<THost>, IVersionHost<THost, TKey, TModel>, IIdTrait<TKey>, new()
        where TModel : Entity<TModel>, IVersionModelHost<THost, TKey, TModel>, new()
        where TKey : IEquatable<TKey>
    {
        var context = dbContext
                      ?? host.DbContext
                      ?? throw new InvalidOperationException("Unable to access DbContext");

        return context
            .Set<TModel>()
            .Where(e => e.EntityId.Equals(host.Id))
            .OrderByDescending(e => e.Number)
            .Skip(1)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<THost?> RestorePreviousVersionAsync<THost, TKey, TModel>(this THost host,
        DbContext? dbContext = null, CancellationToken cancellationToken = default)
        where THost : Entity<THost>, IVersionHost<THost, TKey, TModel>, IIdTrait<TKey>, new()
        where TModel : Entity<TModel>, IVersionModelHost<THost, TKey, TModel>, new()
        where TKey : IEquatable<TKey>
    {
        var context = dbContext
                      ?? host.DbContext
                      ?? throw new InvalidOperationException("Unable to access DbContext");

        var previousVersionModel =
            await host.GetPreviousVersionAsync<THost, TKey, TModel>(dbContext, cancellationToken)
                .ConfigureAwait(false);

        if (previousVersionModel is null) return null;

        var previousVersionHost = previousVersionModel.Serialized;

        if (previousVersionHost is null)
            throw new InvalidOperationException("Unable to deserialize entity version")
            {
                Data =
                {
                    [nameof(previousVersionModel.Serialized)] = previousVersionModel
                        .EntityEntry
                        ?.Property(nameof(previousVersionModel.Serialized))
                        .OriginalValue
                }
            };

        context.Entry(host).CurrentValues.SetValues(previousVersionHost);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return previousVersionHost;
    }
}