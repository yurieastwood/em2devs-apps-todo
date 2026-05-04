using EM2Devs.Todo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class TodoDbContext : DbContext
{
    public DbSet<TodoTask> Tasks => Set<TodoTask>();
    public DbSet<RecurringTask> RecurringTasks => Set<RecurringTask>();
    public DbSet<PlayerProfile> PlayerProfiles => Set<PlayerProfile>();
    public DbSet<StreakSnapshot> StreakSnapshots => Set<StreakSnapshot>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Quest> Quests => Set<Quest>();
    public DbSet<Epic> Epics => Set<Epic>();

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodoDbContext).Assembly);
    }
}
