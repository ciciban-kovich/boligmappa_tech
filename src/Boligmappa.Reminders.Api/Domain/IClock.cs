namespace Boligmappa.Reminders.Api.Domain;

public interface IClock
{
    DateOnly Today { get; }
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
    public DateTime UtcNow => DateTime.UtcNow;
}
