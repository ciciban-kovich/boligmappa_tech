import { ALICE_OWNER_ID, API_BASE_URL } from "./config";
import type { ExpiringDocument, SnoozeResponse } from "./types";

const defaultHeaders = {
  "Content-Type": "application/json",
  "X-User-Id": ALICE_OWNER_ID,
};

export class ApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

async function handle<T>(res: Response): Promise<T> {
  if (!res.ok) {
    let message = `Request failed (${res.status})`;
    try {
      const body = await res.json();
      if (body?.title) message = body.title;
    } catch {
      // body wasn't JSON, keep generic message
    }
    throw new ApiError(res.status, message);
  }
  return res.json() as Promise<T>;
}

export function fetchExpiringDocuments(
  propertyId: string,
  signal?: AbortSignal,
): Promise<ExpiringDocument[]> {
  return fetch(
    `${API_BASE_URL}/api/properties/${propertyId}/documents/expiring`,
    { headers: defaultHeaders, signal },
  ).then(handle<ExpiringDocument[]>);
}

export function snoozeDocument(documentId: string): Promise<SnoozeResponse> {
  return fetch(`${API_BASE_URL}/api/documents/${documentId}/snooze`, {
    method: "POST",
    headers: defaultHeaders,
  }).then(handle<SnoozeResponse>);
}
