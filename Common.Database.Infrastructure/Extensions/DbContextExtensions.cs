using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Database.Infrastructure.Extensions;

public static class DbContextExtensions
{
    public static void CommonConfiguring<TContext>(this TContext context, DbContextOptionsBuilder optionsBuilder)
        where TContext : DbContext, ICommonDbContext<TContext>
    {
        context.EntityChangeListenerService.OnConfiguring(context, optionsBuilder);
    }

    public static void CommonModelCreating<TContext>(this TContext context, ModelBuilder modelBuilder)
        where TContext : DbContext, ICommonDbContext<TContext>
    {
        context.EntityChangeListenerService.OnModelCreating(context, modelBuilder);
        context.AddDbFunctions(modelBuilder);
    }

    public static void SubscribeCommonDbContext<TContext>(this TContext context)
        where TContext : DbContext, ICommonDbContext<TContext>
    {
        context.EntityChangeListenerService.PopulateDbContext(context);
        context.ChangeTracker.Tracked += context.EntityChangeListenerService.OnTracked;
        context.ChangeTracker.StateChanged += context.EntityChangeListenerService.OnStateChanged;
        context.SavingChanges += context.EntityChangeListenerService.OnSavingChanges;
        context.SavedChanges += context.EntityChangeListenerService.OnSavedChanges;
        context.SaveChangesFailed += context.EntityChangeListenerService.OnSaveChangesFailed;
    }

    public static async Task UseNewContextAsync<TContext>(this TContext context,
        IServiceScopeFactory serviceScopeFactory,
        Func<TContext, Task> action)
        where TContext : DbContext
    {
        using var scope = serviceScopeFactory.CreateScope();
        await action((scope.ServiceProvider.GetRequiredService(context.GetType()) as TContext)!).ConfigureAwait(false);
    }
}