import { useCallback, useEffect, useState } from "react";
import { fetchExpiringDocuments, snoozeDocument } from "../../api/client";
import { ALICE_PROPERTY_ID } from "../../api/config";
export function useExpiringDocuments() {
    const [documents, setDocuments] = useState([]);
    const [status, setStatus] = useState("idle");
    const [loadError, setLoadError] = useState(null);
    const [snoozeError, setSnoozeError] = useState(null);
    const [snoozingIds, setSnoozingIds] = useState(new Set());
    const load = useCallback(async (signal) => {
        setStatus("loading");
        setLoadError(null);
        try {
            const docs = await fetchExpiringDocuments(ALICE_PROPERTY_ID, signal);
            setDocuments(docs);
            setStatus("ready");
        }
        catch (err) {
            if (signal?.aborted)
                return;
            setLoadError(err instanceof Error ? err.message : "Failed to load");
            setStatus("error");
        }
    }, []);
    useEffect(() => {
        const controller = new AbortController();
        void load(controller.signal);
        return () => controller.abort();
    }, [load]);
    const snooze = useCallback(async (id) => {
        setSnoozeError(null);
        setSnoozingIds((prev) => new Set(prev).add(id));
        let previousDocs = [];
        setDocuments((docs) => {
            previousDocs = docs;
            return docs.filter((d) => d.id !== id);
        });
        try {
            await snoozeDocument(id);
        }
        catch (err) {
            setDocuments(previousDocs);
            setSnoozeError(err instanceof Error ? err.message : "Snooze failed");
        }
        finally {
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
//# sourceMappingURL=useExpiringDocuments.js.map