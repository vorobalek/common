using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Common.Database.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Common.Database.Infrastructure
{
    public class EntityChange
    {
        private object _originalEntity;
        private IReadOnlyDictionary<string, PropertyChange> _properties;

        public EntityEntry EntityEntry { get; }
        public DbContext Context => EntityEntry.Context;

        private DbContext _newContext;
        public virtual DbContext NewContext
        {
            get => _newContext ?? Context;
            set => _newContext = value;
        }
        
        public virtual bool NeedSaveInNewContext { get; set; }

        public EntityChange(EntityEntry entry)
        {
            EntityEntry = entry;
            State = entry.State;
            OriginalValues = entry.OriginalValues.Clone();
        }

        public EntityChange(EntityChange entityChange)
        {
            EntityEntry = entityChange.EntityEntry;
            State = entityChange.State;
            OriginalValues = entityChange.OriginalValues;
            _properties = entityChange._properties;
        }

        public EntityState State { get; }
        public Type EntityType => EntityEntry.Metadata.ClrType;
        public object Entity => EntityEntry.Entity;
        public object OriginalEntity => _originalEntity ??= OriginalValues.ToObject();

        public bool IsAdded => State == EntityState.Added;
        public bool IsModified => State == EntityState.Modified || ModifiedProperties.Count > 0;
        public bool IsDeleted => State == EntityState.Deleted;
        public bool IsUnchanged => State == EntityState.Unchanged;
        public bool IsDetached => State == EntityState.Detached;

        public PropertyValues OriginalValues { get; }
        public IReadOnlyDictionary<string, PropertyChange> Properties => _properties ??= GetPropertiesFromEntry();
        public IReadOnlyDictionary<string, PropertyChange> ModifiedProperties => 
            Properties
                .Where(p => p.Value.IsModified)
                .ToDictionary(p => p.Key, p => p.Value);
        public IReadOnlyDictionary<string, PropertyChange> ChangedProperties => 
            Properties
                .Where(p => p.Value.IsChanged)
                .ToDictionary(p => p.Key, p => p.Value);

        public PropertyChange this[string propertyName]
            => Property(propertyName);

        private PropertyChange Property(string propertyName)
        {
            return _properties != null 
                ? _properties[propertyName] 
                : new PropertyChange(EntityEntry.Property(propertyName), this);
        }
        private IReadOnlyDictionary<string, PropertyChange> GetPropertiesFromEntry()
        {
            return EntityEntry.Properties
                .Select(entry => new PropertyChange(entry, this))
                .ToDictionary(changes => changes.Name);
        }
    }
    
    public class EntityChange<TEntity> : EntityChange
        where TEntity : class, IEntity
    {
        private EntityChange _entityChange;
        
        internal EntityChange([NotNull] EntityChange entityChange) : base(entityChange)
        {
            _entityChange = entityChange;
        }

        public override DbContext NewContext
        {
            get => _entityChange.NewContext;
            set => _entityChange.NewContext = value;
        }

        public override bool NeedSaveInNewContext 
        { 
            get => _entityChange.NeedSaveInNewContext;
            set => _entityChange.NeedSaveInNewContext = value;
        }

        public new TEntity Entity => (TEntity) base.Entity;
        public new TEntity OriginalEntity => (TEntity) base.OriginalEntity;
        public PropertyChange this[Expression<Func<TEntity, object>> propertyExpression] => Property(propertyExpression);

        private PropertyChange<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            return Property<TProperty>(propertyExpression.GetPropertyAccess().Name);
        }

        private PropertyChange<TProperty> Property<TProperty>(string name)
        {
            return this[name].AsPropertyChange<TProperty>();
        }
    }
}