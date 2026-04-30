namespace Boligmappa.Reminders.Api.Domain;

public sealed record Document
{
    public Guid Id { get; init; }
    public Guid PropertyId { get; init; }
    public required string Name { get; init; }
    public DocumentType DocumentType { get; init; }
    public DateOnly ExpiryDate { get; init; }
    public DateTime CreatedAt { get; init; }

    public Property Property { get; init; } = null!;
    public ICollection<ReminderSnooze> Snoozes { get; init; } = [];
}
