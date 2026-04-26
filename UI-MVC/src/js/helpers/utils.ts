// utils.ts
export class DomUtils {
    static escapeHtml(str: string): string {
        return (str || "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    static getProjectRedirectUrl(basePath: string): string {
        const params = new URLSearchParams(window.location.search);
        const projectId = params.get("projectId");
        const pathSegments = window.location.pathname.split("/").filter(Boolean);
        const subplatform = pathSegments[0] || "";

        return projectId
            ? `/${subplatform}/${basePath}?projectId=${projectId}`
            : `/${subplatform}/${basePath}`;
    }
}