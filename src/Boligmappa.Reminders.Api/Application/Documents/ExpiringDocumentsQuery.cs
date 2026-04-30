using Boligmappa.Reminders.Api.Domain;

namespace Boligmappa.Reminders.Api.Application.Documents;

/// <summary>
/// Canonical query used by both the per-property and cross-property endpoints.
///
/// Rules:
///   - ExpiryDate is in the closed interval [today, today + WindowDays].
///   - Already-expired documents (ExpiryDate &lt; today) are intentionally excluded —
///     "expiring within 90 days" is a forward-looking signal, not a backlog.
///   - A document is hidden while ANY snooze on it has SnoozedUntil &gt; today.
///     Once SnoozedUntil &lt;= today the snooze has lapsed and the document
///     reappears in the list.
///
/// EF Core translates `!d.Snoozes.Any(...)` to `NOT EXISTS (SELECT 1 ...)`,
/// which is supported by the composite index (DocumentId, SnoozedUntil).
/// </summary>
public static class ExpiringDocumentsQuery
{
    public const int WindowDays = 90;

    public static IQueryable<Document> WhereExpiring(this IQueryable<Document> source, DateOnly today)
    {
        var horizon = today.AddDays(WindowDays);
        return source.Where(d =>
            d.ExpiryDate >= today &&
            d.ExpiryDate <= horizon &&
            !d.Snoozes.Any(s => s.SnoozedUntil > today));
    }

    public static IQueryable<ExpiringDocumentDto> ProjectToDto(this IQueryable<Document> source) =>
        source.Select(d => new ExpiringDocumentDto(
            d.Id,
            d.PropertyId,
            d.Name,
            d.DocumentType,
            d.ExpiryDate));
}
