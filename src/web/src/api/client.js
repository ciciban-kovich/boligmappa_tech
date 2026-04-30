import { ALICE_OWNER_ID, API_BASE_URL } from "./config";
const defaultHeaders = {
    "Content-Type": "application/json",
    "X-User-Id": ALICE_OWNER_ID,
};
export class ApiError extends Error {
    status;
    constructor(status, message) {
        super(message);
        this.name = "ApiError";
        this.status = status;
    }
}
async function handle(res) {
    if (!res.ok) {
        let message = `Request failed (${res.status})`;
        try {
            const body = await res.json();
            if (body?.title)
                message = body.title;
        }
        catch {
            // body wasn't JSON, keep generic message
        }
        throw new ApiError(res.status, message);
    }
    return res.json();
}
export function fetchExpiringDocuments(propertyId, signal) {
    return fetch(`${API_BASE_URL}/api/properties/${propertyId}/documents/expiring`, { headers: defaultHeaders, signal }).then((handle));
}
export function snoozeDocument(documentId) {
    return fetch(`${API_BASE_URL}/api/documents/${documentId}/snooze`, {
        method: "POST",
        headers: defaultHeaders,
    }).then((handle));
}
//# sourceMappingURL=client.js.map