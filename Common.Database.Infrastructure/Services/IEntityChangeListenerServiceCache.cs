using System;
using System.Collections.Generic;

namespace Common.Database.Infrastructure.Services
{
    public interface IEntityChangeListenerServiceCache
    {
        IEnumerable<IEntityChangeListener> GetListeners();
        IEnumerable<IEntityChangeListener> GetListeners(Type type);
        IEnumerable<IEntityChangeListener<T>> GetListeners<T>() where T : class, IEntity;
    }
}