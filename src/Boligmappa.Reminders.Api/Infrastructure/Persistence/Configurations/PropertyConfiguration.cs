using Boligmappa.Reminders.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Boligmappa.Reminders.Api.Infrastructure.Persistence.Configurations;

internal sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Address)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.OwnerId)
            .IsRequired();

        builder.HasIndex(p => p.OwnerId)
            .HasDatabaseName("IX_Properties_OwnerId");

        builder.HasMany(p => p.Documents)
            .WithOne(d => d.Property)
            .HasForeignKey(d => d.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
