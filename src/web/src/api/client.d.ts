import type { ExpiringDocument, SnoozeResponse } from "./types";
export declare class ApiError extends Error {
    readonly status: number;
    constructor(status: number, message: string);
}
export declare function fetchExpiringDocuments(propertyId: string, signal?: AbortSignal): Promise<ExpiringDocument[]>;
export declare function snoozeDocument(documentId: string): Promise<SnoozeResponse>;
//# sourceMappingURL=client.d.ts.map