using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Common.Database.Infrastructure;

public class PropertyChange
{
    private readonly object? _entryOriginalValue;

    public PropertyChange(PropertyEntry property, EntityChange entityChange)
    {
        PropertyEntry = property;
        EntityChange = entityChange;
        Name = property.Metadata.Name;
        _entryOriginalValue = entityChange.OriginalValues[property.Metadata];
    }

    public PropertyChange(PropertyChange propertyChange) : this(propertyChange.PropertyEntry,
        propertyChange.EntityChange)
    {
    }

    private PropertyEntry PropertyEntry { get; }
    private EntityChange EntityChange { get; }

    public string Name { get; }

    public object? OriginalValue
    {
        get
        {
            return EntityChange.State switch
            {
                EntityState.Added => PropertyEntry.Metadata.ClrType.GetDefaultValue(),
                EntityState.Modified or EntityState.Unchanged => _entryOriginalValue,
                _ => Value
            };
        }
    }

    public virtual object? Value => PropertyEntry.CurrentValue;

    public virtual bool IsModified => EntityChange.State is EntityState.Modified or EntityState.Unchanged &&
                                      !Equals(Value, OriginalValue);

    public virtual bool IsChanged => EntityChange.State == EntityState.Added || !Equals(Value, OriginalValue);
}

public class PropertyChange<T> : PropertyChange
{
    public PropertyChange(PropertyChange propertyChange) : base(propertyChange)
    {
    }

    public new T? Value => (T?)base.Value;

    public new T? OriginalValue =>
        base.OriginalValue == null
            ? default
            : (T)base.OriginalValue;
}