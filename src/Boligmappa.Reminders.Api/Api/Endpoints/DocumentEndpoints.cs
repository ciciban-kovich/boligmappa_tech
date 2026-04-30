using System.Security.Claims;
using Boligmappa.Reminders.Api.Application.Common;
using Boligmappa.Reminders.Api.Application.Documents;
using Boligmappa.Reminders.Api.Domain;
using Boligmappa.Reminders.Api.Infrastructure.Auth;
using Boligmappa.Reminders.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Boligmappa.Reminders.Api.Api.Endpoints;

public static class DocumentEndpoints
{
    private const int MaxPageSize = 100;
    private const int SnoozeDays = 30;

    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var documents = app.MapGroup("/api/documents").WithTags("Documents");

        documents.MapGet("/expiring", GetExpiringDocuments)
            .WithName("GetExpiringDocuments")
            .WithSummary("Internal cross-property feed for the batch notification job (paginated).");

        documents.MapPost("/{documentId:guid}/snooze", SnoozeDocument)
            .WithName("SnoozeDocument")
            .WithSummary("Snooze the document's expiry reminder by 30 days. Owner-only.")
            .RequireAuthorization();

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

    private static async Task<Results<Ok<SnoozeResponse>, NotFound, ForbidHttpResult>> SnoozeDocument(
        Guid documentId,
        AppDbContext db,
        IClock clock,
        IAuthorizationService authz,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var document = await db.Documents
            .Include(d => d.Property)
            .FirstOrDefaultAsync(d => d.Id == documentId, ct);

        if (document is null)
            return TypedResults.NotFound();

        var authResult = await authz.AuthorizeAsync(user, document, AuthPolicies.DocumentOwner);
        if (!authResult.Succeeded)
            return TypedResults.Forbid();

        var snoozedUntil = clock.Today.AddDays(SnoozeDays);

        db.ReminderSnoozes.Add(new ReminderSnooze
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            SnoozedUntil = snoozedUntil,
            SnoozedAt = clock.UtcNow,
        });

        await db.SaveChangesAsync(ct);

        return TypedResults.Ok(new SnoozeResponse(documentId, snoozedUntil));
    }
}
