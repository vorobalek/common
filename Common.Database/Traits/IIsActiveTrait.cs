using System.Collections.Generic;
using System.Linq;
using Common.Database.Infrastructure;
using Common.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Database.Traits;

public interface IIsActiveTrait : IEntityTrait
{
    bool IsActive { get; set; }
}

public sealed class IsActiveChangeListener<TEntity> : EntityTraitChangeListener<TEntity, IIsActiveTrait>
    where TEntity : class, IIsActiveTrait
{
    protected override void OnModelCreating(EntityTypeBuilder<TEntity> builder)
    {
        base.OnModelCreating(builder);

        builder
            .Property(e => e.IsActive)
            .HasDefaultValue(true);
    }
}

public static partial class EntityExtensions
{
    public static IEnumerable<TEntity> OnlyActive<TEntity>(this IEnumerable<TEntity> enumerable)
        where TEntity : IIsActiveTrait
    {
        return enumerable.Where(x => x.IsActive);
    }

    public static IQueryable<TEntity> OnlyActive<TEntity>(this IQueryable<TEntity> enumerable)
        where TEntity : IIsActiveTrait
    {
        return enumerable.Where(x => x.IsActive);
    }

    public static void Activate<TEntity>(this DbSet<TEntity> dbSet, TEntity schedule)
        where TEntity : class, IIsActiveTrait
    {
        schedule.IsActive = true;
        dbSet.Update(schedule);
    }

    public static void Inactivate<TEntity>(this DbSet<TEntity> dbSet, TEntity schedule)
        where TEntity : class, IIsActiveTrait
    {
        schedule.IsActive = false;
        dbSet.Update(schedule);
    }
}