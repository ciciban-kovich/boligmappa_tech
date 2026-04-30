using Boligmappa.Reminders.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Boligmappa.Reminders.Api.Infrastructure.Persistence;

/// <summary>
/// Development-only seed. Idempotent: returns immediately if any property already exists.
/// Dates are computed relative to <see cref="IClock.Today"/> so the demo always has
/// documents that are "currently expiring", regardless of when the database is provisioned.
/// </summary>
public static class DbInitializer
{
    public static readonly Guid OwnerAliceId   = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid OwnerBobId     = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid AlicePropertyId = Guid.Parse("aaaa1111-1111-1111-1111-111111111111");
    public static readonly Guid BobPropertyId   = Guid.Parse("bbbb2222-2222-2222-2222-222222222222");

    public static async Task SeedAsync(AppDbContext db, IClock clock, CancellationToken ct = default)
    {
        if (await db.Properties.AnyAsync(ct)) return;

        var today = clock.Today;
        var now = clock.UtcNow;

        var alice = new Property
        {
            Id = AlicePropertyId,
            Address = "Storgata 1, 0155 Oslo",
            OwnerId = OwnerAliceId,
        };

        var bob = new Property
        {
            Id = BobPropertyId,
            Address = "Karl Johans gate 22, 0159 Oslo",
            OwnerId = OwnerBobId,
        };

        db.Properties.AddRange(alice, bob);

        // Documents on Alice's property — cover every branch of the expiring-snooze rule.
        var notExpiring = new Document
        {
            Id = NewId(),
            PropertyId = alice.Id,
            Name = "Husforsikring 2027",
            DocumentType = DocumentType.Other,
            ExpiryDate = today.AddDays(200),
            CreatedAt = now.AddDays(-30),
        };

        var alreadyExpired = new Document
        {
            Id = NewId(),
            PropertyId = alice.Id,
            Name = "Brannsikkerhetsrapport (utløpt)",
            DocumentType = DocumentType.FireSafetyInspection,
            ExpiryDate = today.AddDays(-10),
            CreatedAt = now.AddDays(-400),
        };

        var expiringNoSnooze1 = new Document
        {
            Id = NewId(),
            PropertyId = alice.Id,
            Name = "Plantegning",
            DocumentType = DocumentType.FloorPlan,
            ExpiryDate = today.AddDays(30),
            CreatedAt = now.AddDays(-60),
        };

        var expiringNoSnooze2 = new Document
        {
            Id = NewId(),
            PropertyId = alice.Id,
            Name = "El-tilsynssertifikat",
            DocumentType = DocumentType.ElectricalCertificate,
            ExpiryDate = today.AddDays(60),
            CreatedAt = now.AddDays(-90),
        };

        var expiringSnoozeActive = new Document
        {
            Id = NewId(),
            PropertyId = alice.Id,
            Name = "Energimerking",
            DocumentType = DocumentType.EnergyCertificate,
            ExpiryDate = today.AddDays(20),
            CreatedAt = now.AddDays(-120),
        };

        var expiringSnoozeExpired = new Document
        {
            Id = NewId(),
            PropertyId = alice.Id,
            Name = "Tilstandsrapport",
            DocumentType = DocumentType.ConditionReport,
            ExpiryDate = today.AddDays(80),
            CreatedAt = now.AddDays(-200),
        };

        // Document on Bob's property — only visible via the cross-property endpoint.
        var bobExpiring = new Document
        {
            Id = NewId(),
            PropertyId = bob.Id,
            Name = "VVS-rapport",
            DocumentType = DocumentType.ConditionReport,
            ExpiryDate = today.AddDays(45),
            CreatedAt = now.AddDays(-15),
        };

        db.Documents.AddRange(
            notExpiring,
            alreadyExpired,
            expiringNoSnooze1,
            expiringNoSnooze2,
            expiringSnoozeActive,
            expiringSnoozeExpired,
            bobExpiring);

        // Active snooze: hides expiringSnoozeActive from the list until today+14.
        db.ReminderSnoozes.Add(new ReminderSnooze
        {
            Id = NewId(),
            DocumentId = expiringSnoozeActive.Id,
            SnoozedUntil = today.AddDays(14),
            SnoozedAt = now.AddDays(-1),
        });

        // Expired snooze: should NOT hide expiringSnoozeExpired anymore.
        db.ReminderSnoozes.Add(new ReminderSnooze
        {
            Id = NewId(),
            DocumentId = expiringSnoozeExpired.Id,
            SnoozedUntil = today.AddDays(-5),
            SnoozedAt = now.AddDays(-35),
        });

        await db.SaveChangesAsync(ct);
    }

    private static Guid NewId() => Guid.NewGuid();
}
