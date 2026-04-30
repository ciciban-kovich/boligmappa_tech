using Boligmappa.Reminders.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Boligmappa.Reminders.Api.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ReminderSnooze> ReminderSnoozes => Set<ReminderSnooze>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
