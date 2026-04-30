namespace Boligmappa.Reminders.Api.Domain;

public sealed record ReminderSnooze
{
    public Guid Id { get; init; }
    public Guid DocumentId { get; init; }
    public DateOnly SnoozedUntil { get; init; }
    public DateTime SnoozedAt { get; init; }

    public Document Document { get; init; } = null!;
}
