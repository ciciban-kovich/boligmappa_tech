using Boligmappa.Reminders.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Boligmappa.Reminders.Api.Infrastructure.Persistence.Configurations;

internal sealed class ReminderSnoozeConfiguration : IEntityTypeConfiguration<ReminderSnooze>
{
    public void Configure(EntityTypeBuilder<ReminderSnooze> builder)
    {
        builder.ToTable("ReminderSnoozes");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SnoozedUntil)
            .IsRequired();

        builder.Property(s => s.SnoozedAt)
            .IsRequired();

        // Supports the NOT EXISTS subquery in the expiring-documents query.
        builder.HasIndex(s => new { s.DocumentId, s.SnoozedUntil })
            .HasDatabaseName("IX_ReminderSnoozes_DocumentId_SnoozedUntil");
    }
}
