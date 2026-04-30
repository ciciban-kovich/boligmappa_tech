// Demo shortcuts: in production these come from auth (current user) and a
// "select property" UI. Here they mirror the seeded constants on
// DbInitializer so the take-home runs without a login flow.
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5154";
export const ALICE_OWNER_ID = "11111111-1111-1111-1111-111111111111";
export const ALICE_PROPERTY_ID = "aaaa1111-1111-1111-1111-111111111111";
//# sourceMappingURL=config.js.map