export type DocumentType =
  | "Other"
  | "FloorPlan"
  | "ElectricalCertificate"
  | "ConditionReport"
  | "EnergyCertificate"
  | "FireSafetyInspection";

export interface ExpiringDocument {
  id: string;
  propertyId: string;
  name: string;
  documentType: DocumentType;
  expiryDate: string;
}

export interface SnoozeResponse {
  documentId: string;
  snoozedUntil: string;
}
