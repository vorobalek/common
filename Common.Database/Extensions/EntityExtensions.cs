using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Common.Database.Extensions;

/// <summary>
/// Aggregated class for whole traits extensions
/// </summary>
public static partial class EntityExtensions
{
    public static void RemoveRange<TEntity>(this IEnumerable<TEntity> enumerable, DbContext? dbContext = null)
        where TEntity : Entity<TEntity>
    {
        enumerable ??= Array.Empty<TEntity>();
        var entities = enumerable as TEntity[] ?? enumerable.ToArray();
        foreach (var entity in entities) entity.Remove(dbContext);
    }
}