using System;
using System.Collections.Generic;
using Common.Database.Infrastructure;
using Common.Database.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Database.Services
{
    public class EntityChangeListenerServiceCache : IEntityChangeListenerServiceCache
    {
        private readonly IServiceProvider _serviceProvider;

        public EntityChangeListenerServiceCache(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<IEntityChangeListener> GetListeners()
        {
            return _serviceProvider.GetServices<IEntityChangeListener>();
        }
        public IEnumerable<IEntityChangeListener> GetListeners(Type type)
        {
            return _serviceProvider.GetServices(typeof(IEntityChangeListener<>).MakeGenericType(type)) as
                IEnumerable<IEntityChangeListener>;
        }

        public IEnumerable<IEntityChangeListener<T>> GetListeners<T>() where T : class, IEntity
        {
            return _serviceProvider.GetServices<IEntityChangeListener<T>>();
        }
    }
}