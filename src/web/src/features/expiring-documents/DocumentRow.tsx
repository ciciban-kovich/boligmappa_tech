import type { DocumentType, ExpiringDocument } from "../../api/types";
import styles from "./DocumentRow.module.css";

interface Props {
  document: ExpiringDocument;
  isSnoozing: boolean;
  onSnooze: () => void;
}

const TYPE_LABELS: Record<DocumentType, string> = {
  Other:                  "Other",
  FloorPlan:              "Floor plan",
  ElectricalCertificate:  "Electrical certificate",
  ConditionReport:        "Condition report",
  EnergyCertificate:      "Energy certificate",
  FireSafetyInspection:   "Fire safety inspection",
};

export function DocumentRow({ document, isSnoozing, onSnooze }: Props) {
  const days = daysUntil(document.expiryDate);
  const urgency =
    days <= 14 ? styles.urgent
    : days <= 30 ? styles.warning
    : styles.normal;

  return (
    <li className={`${styles.row} ${urgency}`}>
      <div className={styles.info}>
        <h3 className={styles.name}>{document.name}</h3>
        <p className={styles.type}>{TYPE_LABELS[document.documentType] ?? document.documentType}</p>
      </div>
      <div className={styles.expiry}>
        <span className={styles.date}>{document.expiryDate}</span>
        <span className={styles.daysLabel}>
          {days > 1 ? `in ${days} days` : days === 1 ? "tomorrow" : days === 0 ? "today" : "overdue"}
        </span>
      </div>
      <button
        type="button"
        className={styles.snooze}
        onClick={onSnooze}
        disabled={isSnoozing}
        aria-label={`Snooze ${document.name} 30 days`}
      >
        {isSnoozing ? "Snoozing…" : "Snooze 30 days"}
      </button>
    </li>
  );
}

function daysUntil(isoDate: string): number {
  const expiry = new Date(`${isoDate}T00:00:00Z`);
  const today = new Date();
  today.setUTCHours(0, 0, 0, 0);
  return Math.round((expiry.getTime() - today.getTime()) / 86_400_000);
}
