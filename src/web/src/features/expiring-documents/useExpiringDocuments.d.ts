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
export declare function useExpiringDocuments(): UseExpiringDocumentsResult;
//# sourceMappingURL=useExpiringDocuments.d.ts.map