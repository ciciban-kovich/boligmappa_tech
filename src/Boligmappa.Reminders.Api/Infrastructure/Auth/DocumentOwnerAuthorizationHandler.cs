using System.Security.Claims;
using Boligmappa.Reminders.Api.Domain;
using Microsoft.AspNetCore.Authorization;

namespace Boligmappa.Reminders.Api.Infrastructure.Auth;

/// <summary>
/// Resource-based authorization: succeeds when the current principal owns the
/// property the document belongs to.
///
/// Contract: callers must load <see cref="Document.Property"/> before invoking
/// <c>IAuthorizationService.AuthorizeAsync(user, document, AuthPolicies.DocumentOwner)</c>.
/// We deliberately do not lazy-load the property here — keeping this handler
/// pure (no DbContext dependency) makes it trivial to unit test and avoids
/// silent extra round-trips inside the authz pipeline.
/// </summary>
public sealed class DocumentOwnerAuthorizationHandler
    : AuthorizationHandler<DocumentOwnerRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DocumentOwnerRequirement requirement,
        Document document)
    {
        var subject = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(subject, out var userId)
            && document.Property is { } property
            && property.OwnerId == userId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
