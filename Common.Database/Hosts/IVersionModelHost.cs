using System;
using Common.Database.Infrastructure;
using Common.Database.Services;
using Common.Database.Traits;
using Common.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Common.Database.Hosts;

public interface IVersionModelHost<THost, TKey, TModel> : IEntityHost<TModel>
    where THost : Entity<THost>, IVersionHost<THost, TKey, TModel>, IIdTrait<TKey>, new()
    where TKey : IEquatable<TKey>
    where TModel : Entity<TModel>, IVersionModelHost<THost, TKey, TModel>, new()
{
    int Number { get; set; }
    TKey EntityId { get; set; }
    THost? Serialized { get; set; }
}

public sealed class
    VersionModelHostChangeListener<THost, TKey, TModel> : EntityHostChangeListener<TModel,
    IVersionModelHost<THost, TKey, TModel>>
    where THost : Entity<THost>, IVersionHost<THost, TKey, TModel>, IIdTrait<TKey>, new()
    where TKey : IEquatable<TKey>
    where TModel : Entity<TModel>, IVersionModelHost<THost, TKey, TModel>, new()
{
    public override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TModel>()
            .HasKey(e => e.Number);

        builder.Entity<TModel>()
            .Property(e => e.Number)
            .ValueGeneratedOnAdd();

        builder.Entity<TModel>()
            .HasIndex(e => e.EntityId)
            .IsUnique(false);

        var jsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        builder.Entity<TModel>()
            .Property(e => e.Serialized)
            .HasConversion(
                v => ExecutionHelper
                    .TryIgnore(() =>
                        JsonConvert.SerializeObject(v, jsonSerializerSettings)),
                v => ExecutionHelper
                    .TryIgnore(() =>
                        JsonConvert.DeserializeObject<THost>(v ?? string.Empty, jsonSerializerSettings)));

        if (builder.Entity<TModel>().Metadata.GetTableName() != 
            builder.Entity<TModel>().Metadata.GetDefaultTableName()) return;

        var hostTableName = builder.Entity<THost>().Metadata.GetTableName();
        builder.Entity<TModel>()
            .ToTable($"{hostTableName}_Versions");
    }
}