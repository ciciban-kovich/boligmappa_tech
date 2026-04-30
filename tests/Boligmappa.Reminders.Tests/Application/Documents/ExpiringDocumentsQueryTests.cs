using Boligmappa.Reminders.Api.Application.Documents;
using Boligmappa.Reminders.Api.Domain;
using Boligmappa.Reminders.Api.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Boligmappa.Reminders.Tests.Application.Documents;

/// <summary>
/// Focused tests for the snooze branch of the expiring-documents query
/// (brief's explicit bonus: verify snoozed documents are correctly excluded).
///
/// Backed by SQLite in-memory rather than EF's in-memory provider — SQLite
/// runs the actual translated SQL, so the NOT EXISTS subquery is exercised
/// for real, not just at the LINQ-to-objects level.
/// </summary>
public sealed class ExpiringDocumentsQueryTests : IAsyncLifetime
{
    private static readonly DateOnly Today = new(2026, 4, 30);
    private static readonly Guid PropertyId = Guid.Parse("aaaa1111-1111-1111-1111-111111111111");
    private static readonly Guid OwnerId    = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly SqliteConnection _connection = new("Filename=:memory:");
    private AppDbContext _db = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new AppDbContext(options);
        await _db.Database.EnsureCreatedAsync();

        _db.Properties.Add(new Property
        {
            Id = PropertyId,
            Address = "Storgata 1",
            OwnerId = OwnerId,
        });
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task Document_with_no_snooze_is_included()
    {
        var doc = await AddDocumentExpiringIn(30);

        var ids = await _db.Documents.WhereExpiring(Today).Select(d => d.Id).ToListAsync();

        Assert.Contains(doc.Id, ids);
    }

    [Fact]
    public async Task Document_with_active_snooze_is_excluded()
    {
        var doc = await AddDocumentExpiringIn(30);
        await AddSnooze(doc.Id, snoozedUntil: Today.AddDays(7));

        var ids = await _db.Documents.WhereExpiring(Today).Select(d => d.Id).ToListAsync();

        Assert.DoesNotContain(doc.Id, ids);
    }

    [Fact]
    public async Task Document_with_lapsed_snooze_is_included()
    {
        var doc = await AddDocumentExpiringIn(30);
        await AddSnooze(doc.Id, snoozedUntil: Today.AddDays(-1));

        var ids = await _db.Documents.WhereExpiring(Today).Select(d => d.Id).ToListAsync();

        Assert.Contains(doc.Id, ids);
    }

    [Fact]
    public async Task Snooze_ending_today_is_treated_as_lapsed()
    {
        // Rule is `SnoozedUntil > today` (strict). So when today == SnoozedUntil
        // the snooze has just ended and the document should reappear.
        var doc = await AddDocumentExpiringIn(30);
        await AddSnooze(doc.Id, snoozedUntil: Today);

        var ids = await _db.Documents.WhereExpiring(Today).Select(d => d.Id).ToListAsync();

        Assert.Contains(doc.Id, ids);
    }

    [Fact]
    public async Task Document_with_one_active_snooze_among_many_is_excluded()
    {
        var doc = await AddDocumentExpiringIn(30);
        await AddSnooze(doc.Id, snoozedUntil: Today.AddDays(-30));  // lapsed
        await AddSnooze(doc.Id, snoozedUntil: Today.AddDays(-1));   // lapsed
        await AddSnooze(doc.Id, snoozedUntil: Today.AddDays(7));    // active
        await AddSnooze(doc.Id, snoozedUntil: Today.AddDays(-90));  // lapsed

        var ids = await _db.Documents.WhereExpiring(Today).Select(d => d.Id).ToListAsync();

        Assert.DoesNotContain(doc.Id, ids);
    }

    private async Task<Document> AddDocumentExpiringIn(int days)
    {
        var doc = new Document
        {
            Id = Guid.NewGuid(),
            PropertyId = PropertyId,
            Name = "Test document",
            DocumentType = DocumentType.Other,
            ExpiryDate = Today.AddDays(days),
            CreatedAt = DateTime.UtcNow,
        };
        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();
        return doc;
    }

    private async Task AddSnooze(Guid documentId, DateOnly snoozedUntil)
    {
        _db.ReminderSnoozes.Add(new ReminderSnooze
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            SnoozedUntil = snoozedUntil,
            SnoozedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
    }
}
