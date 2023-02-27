using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Common.Database.Infrastructure.Services;

public interface IEntityChangeListenerService<in TContext>
    where TContext : DbContext, ICommonDbContext<TContext>
{
    void PopulateDbContext(TContext context);
    void OnConfiguring(TContext dbContext, DbContextOptionsBuilder optionsBuilder);
    void OnModelCreating(TContext dbContext, ModelBuilder modelBuilder);
    void OnTracked(object? sender, EntityTrackedEventArgs e);
    void OnStateChanged(object? sender, EntityStateChangedEventArgs e);
    void OnSavingChanges(object? sender, SavingChangesEventArgs e);
    void OnSavedChanges(object? sender, SavedChangesEventArgs e);
    void OnSaveChangesFailed(object? sender, SaveChangesFailedEventArgs e);
}