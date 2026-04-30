import { ALICE_PROPERTY_ID } from "../../api/config";
import { DocumentRow } from "./DocumentRow";
import styles from "./ExpiringDocumentsPage.module.css";
import { useExpiringDocuments } from "./useExpiringDocuments";

export function ExpiringDocumentsPage() {
  const { documents, status, loadError, snoozeError, snoozingIds, snooze, refresh } =
    useExpiringDocuments();

  return (
    <section className={styles.page}>
      <header className={styles.header}>
        <h1 className={styles.title}>Expiring documents</h1>
        <p className={styles.subtitle}>
          Property <code className={styles.code}>{ALICE_PROPERTY_ID}</code> — within the next 90 days
        </p>
      </header>

      {status === "loading" && (
        <div className={styles.state} role="status" aria-live="polite">
          Loading…
        </div>
      )}

      {status === "error" && (
        <div className={`${styles.state} ${styles.errorState}`} role="alert">
          <p>Could not load documents — {loadError}</p>
          <button type="button" className={styles.retry} onClick={() => void refresh()}>
            Retry
          </button>
        </div>
      )}

      {status === "ready" && documents.length === 0 && (
        <div className={styles.state}>No documents expiring in the next 90 days.</div>
      )}

      {status === "ready" && documents.length > 0 && (
        <ul className={styles.list}>
          {documents.map((doc) => (
            <DocumentRow
              key={doc.id}
              document={doc}
              isSnoozing={snoozingIds.has(doc.id)}
              onSnooze={() => void snooze(doc.id)}
            />
          ))}
        </ul>
      )}

      {snoozeError && (
        <div className={styles.toast} role="alert">
          Snooze failed: {snoozeError}
        </div>
      )}
    </section>
  );
}
