using System;
using System.Dynamic;

namespace Common.Testing;

public class IsolatedContext
{
    private readonly dynamic _context;

    public IsolatedContext()
    {
        _context = new ExpandoObject();
    }

    public T? Get<T>(Func<dynamic, dynamic> func, T? defaultValue = default)
    {
        try
        {
            var value = func.Invoke(_context);
            return (T)value;
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    public bool Get<T>(Func<dynamic, dynamic> func, out T? value, T? defaultValue = default)
    {
        try
        {
            value = (T)func.Invoke(_context);
            return true;
        }
        catch (Exception)
        {
            value = defaultValue;
            return false;
        }
    }

    public void Set(Action<dynamic> action)
    {
        action.Invoke(_context);
    }
}