using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Testing
{
    public delegate void ConfigureIsolatedContextDelegate(IsolatedContextConfiguration configuration);
    public delegate Task IsolatedActionAsyncDelegate(IsolatedContext context, CancellationToken cancellationToken);
    public delegate Task PrepareIsolatedActionAsyncDelegate(IsolatedContext context, CancellationToken cancellationToken = default);
    public delegate Task FinalizeIsolatedActionAsyncDelegate(IsolatedContext context, CancellationToken cancellationToken = default);
    public delegate Task HandleIsolatedActionExceptionAsyncDelegate(IsolatedContext context, Exception exception, CancellationToken cancellationToken = default); 
    
    public class IsolatedContextConfiguration
    {
        public PrepareIsolatedActionAsyncDelegate PrepareAsync { get; set; }
        public FinalizeIsolatedActionAsyncDelegate FinalizeAsync { get; set; }
        public HandleIsolatedActionExceptionAsyncDelegate HandleExceptionAsync { get; set; }
    }
}