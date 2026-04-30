namespace Boligmappa.Reminders.Api.Application.Documents;

public sealed record SnoozeResponse(Guid DocumentId, DateOnly SnoozedUntil);
