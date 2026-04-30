using Boligmappa.Reminders.Api.Application.Documents;
using Boligmappa.Reminders.Api.Domain;
using Boligmappa.Reminders.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Boligmappa.Reminders.Api.Api.Endpoints;

public static class PropertyEndpoints
{
    public static IEndpointRouteBuilder MapPropertyEndpoints(this IEndpointRouteBuilder app)
    {
        var properties = app.MapGroup("/api/properties").WithTags("Properties");

        properties.MapGet("/{propertyId:guid}/documents/expiring", GetExpiringDocumentsForProperty)
            .WithName("GetExpiringDocumentsForProperty");

        return app;
    }

    private static async Task<Results<Ok<IReadOnlyList<ExpiringDocumentDto>>, NotFound>> GetExpiringDocumentsForProperty(
        Guid propertyId,
        AppDbContext db,
        IClock clock,
        CancellationToken ct)
    {
        var propertyExists = await db.Properties.AnyAsync(p => p.Id == propertyId, ct);
        if (!propertyExists) return TypedResults.NotFound();

        var today = clock.Today;

        var documents = await db.Documents
            .AsNoTracking()
            .Where(d => d.PropertyId == propertyId)
            .WhereExpiring(today)
            .OrderBy(d => d.ExpiryDate)
            .ThenBy(d => d.Name)
            .ProjectToDto()
            .ToListAsync(ct);

        return TypedResults.Ok<IReadOnlyList<ExpiringDocumentDto>>(documents);
    }
}
