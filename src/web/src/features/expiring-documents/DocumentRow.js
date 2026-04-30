import { jsx as _jsx, jsxs as _jsxs } from "react/jsx-runtime";
import styles from "./DocumentRow.module.css";
const TYPE_LABELS = {
    Other: "Other",
    FloorPlan: "Floor plan",
    ElectricalCertificate: "Electrical certificate",
    ConditionReport: "Condition report",
    EnergyCertificate: "Energy certificate",
    FireSafetyInspection: "Fire safety inspection",
};
export function DocumentRow({ document, isSnoozing, onSnooze }) {
    const days = daysUntil(document.expiryDate);
    const urgency = days <= 14 ? styles.urgent
        : days <= 30 ? styles.warning
            : styles.normal;
    return (_jsxs("li", { className: `${styles.row} ${urgency}`, children: [_jsxs("div", { className: styles.info, children: [_jsx("h3", { className: styles.name, children: document.name }), _jsx("p", { className: styles.type, children: TYPE_LABELS[document.documentType] ?? document.documentType })] }), _jsxs("div", { className: styles.expiry, children: [_jsx("span", { className: styles.date, children: document.expiryDate }), _jsx("span", { className: styles.daysLabel, children: days > 1 ? `in ${days} days` : days === 1 ? "tomorrow" : days === 0 ? "today" : "overdue" })] }), _jsx("button", { type: "button", className: styles.snooze, onClick: onSnooze, disabled: isSnoozing, "aria-label": `Snooze ${document.name} 30 days`, children: isSnoozing ? "Snoozing…" : "Snooze 30 days" })] }));
}
function daysUntil(isoDate) {
    const expiry = new Date(`${isoDate}T00:00:00Z`);
    const today = new Date();
    today.setUTCHours(0, 0, 0, 0);
    return Math.round((expiry.getTime() - today.getTime()) / 86_400_000);
}
//# sourceMappingURL=DocumentRow.js.map