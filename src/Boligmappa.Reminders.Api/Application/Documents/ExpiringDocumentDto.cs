using Boligmappa.Reminders.Api.Domain;

namespace Boligmappa.Reminders.Api.Application.Documents;

public sealed record ExpiringDocumentDto(
    Guid Id,
    Guid PropertyId,
    string Name,
    DocumentType DocumentType,
    DateOnly ExpiryDate);
