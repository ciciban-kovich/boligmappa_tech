namespace Boligmappa.Reminders.Api.Domain;

public enum DocumentType : byte
{
    Other = 0,
    FloorPlan = 1,
    ElectricalCertificate = 2,
    ConditionReport = 3,
    EnergyCertificate = 4,
    FireSafetyInspection = 5,
}
