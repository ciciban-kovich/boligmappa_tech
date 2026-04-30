namespace Boligmappa.Reminders.Api.Domain;

public sealed record Property
{
    public Guid Id { get; init; }
    public required string Address { get; init; }
    public Guid OwnerId { get; init; }

    public ICollection<Document> Documents { get; init; } = [];
}
