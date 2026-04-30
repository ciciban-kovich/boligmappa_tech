namespace Boligmappa.Reminders.Api.Application.Common;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long TotalCount)
{
    public bool HasMore => (long)Page * PageSize < TotalCount;
}
