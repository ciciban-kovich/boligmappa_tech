using Boligmappa.Reminders.Api.Application.Common;
using Boligmappa.Reminders.Api.Application.Documents;
using Boligmappa.Reminders.Api.Domain;
using Boligmappa.Reminders.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Boligmappa.Reminders.Api.Api.Endpoints;

public static class DocumentEndpoints
{
    private const int MaxPageSize = 100;

    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var documents = app.MapGroup("/api/documents").WithTags("Documents");

        documents.MapGet("/expiring", GetExpiringDocuments)
            .WithName("GetExpiringDocuments")
            .WithSummary("Internal cross-property feed for the batch notification job (paginated).");

        return app;
    }

    private static async Task<Results<Ok<PagedResult<ExpiringDocumentDto>>, ValidationProblem>> GetExpiringDocuments(
        AppDbContext db,
        IClock clock,
        CancellationToken ct,
        int page = 1,
        int pageSize = 50)
    {
        Dictionary<string, string[]> errors = [];
        if (page < 1)
            errors[nameof(page)] = ["Page must be 1 or greater."];
        if (pageSize is < 1 or > MaxPageSize)
            errors[nameof(pageSize)] = [$"PageSize must be between 1 and {MaxPageSize}."];
        if (errors.Count > 0)
            return TypedResults.ValidationProblem(errors);

        var today = clock.Today;

        var baseQuery = db.Documents.AsNoTracking().WhereExpiring(today);

        var totalCount = await baseQuery.LongCountAsync(ct);

        var items = await baseQuery
            .OrderBy(d => d.ExpiryDate)
            .ThenBy(d => d.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectToDto()
            .ToListAsync(ct);

        return TypedResults.Ok(new PagedResult<ExpiringDocumentDto>(items, page, pageSize, totalCount));
    }
}
