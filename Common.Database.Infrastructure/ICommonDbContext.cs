using Common.Database.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Common.Database.Infrastructure
{
    public interface ICommonDbContext<in TContext>
        where TContext : DbContext, ICommonDbContext<TContext>
    {
        IEntityChangeListenerService<TContext> EntityChangeListenerService { get; }
    }
}