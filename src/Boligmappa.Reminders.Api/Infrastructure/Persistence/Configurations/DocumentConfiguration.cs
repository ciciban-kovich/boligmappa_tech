using Boligmappa.Reminders.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Boligmappa.Reminders.Api.Infrastructure.Persistence.Configurations;

internal sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.DocumentType)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(d => d.ExpiryDate)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        // Primary filter for the "expiring documents" query.
        // At 10M+ rows we'd swap this for a covering index:
        //   CREATE INDEX IX_Documents_ExpiryDate_Covering
        //     ON Documents (ExpiryDate) INCLUDE (PropertyId, Name, DocumentType);
        builder.HasIndex(d => d.ExpiryDate)
            .HasDatabaseName("IX_Documents_ExpiryDate");

        builder.HasMany(d => d.Snoozes)
            .WithOne(s => s.Document)
            .HasForeignKey(s => s.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
