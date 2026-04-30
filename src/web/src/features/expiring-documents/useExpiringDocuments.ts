import { useCallback, useEffect, useState } from "react";
import { fetchExpiringDocuments, snoozeDocument } from "../../api/client";
import { ALICE_PROPERTY_ID } from "../../api/config";
import type { ExpiringDocument } from "../../api/types";

export type LoadStatus = "idle" | "loading" | "ready" | "error";

export interface UseExpiringDocumentsResult {
  documents: ExpiringDocument[];
  status: LoadStatus;
  loadError: string | null;
  snoozeError: string | null;
  snoozingIds: ReadonlySet<string>;
  snooze: (id: string) => Promise<void>;
  refresh: () => Promise<void>;
}

export function useExpiringDocuments(): UseExpiringDocumentsResult {
  const [documents, setDocuments] = useState<ExpiringDocument[]>([]);
  const [status, setStatus] = useState<LoadStatus>("idle");
  const [loadError, setLoadError] = useState<string | null>(null);
  const [snoozeError, setSnoozeError] = useState<string | null>(null);
  const [snoozingIds, setSnoozingIds] = useState<ReadonlySet<string>>(new Set());

  const load = useCallback(async (signal?: AbortSignal) => {
    setStatus("loading");
    setLoadError(null);
    try {
      const docs = await fetchExpiringDocuments(ALICE_PROPERTY_ID, signal);
      setDocuments(docs);
      setStatus("ready");
    } catch (err) {
      if (signal?.aborted) return;
      setLoadError(err instanceof Error ? err.message : "Failed to load");
      setStatus("error");
    }
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    void load(controller.signal);
    return () => controller.abort();
  }, [load]);

  const snooze = useCallback(async (id: string) => {
    setSnoozeError(null);
    setSnoozingIds((prev) => new Set(prev).add(id));

    let previousDocs: ExpiringDocument[] = [];
    setDocuments((docs) => {
      previousDocs = docs;
      return docs.filter((d) => d.id !== id);
    });

    try {
      await snoozeDocument(id);
    } catch (err) {
      setDocuments(previousDocs);
      setSnoozeError(err instanceof Error ? err.message : "Snooze failed");
    } finally {
      setSnoozingIds((prev) => {
        const next = new Set(prev);
        next.delete(id);
        return next;
      });
    }
  }, []);

  const refresh = useCallback(() => load(), [load]);

  return { documents, status, loadError, snoozeError, snoozingIds, snooze, refresh };
}
