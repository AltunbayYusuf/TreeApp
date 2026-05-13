export class DomUtils {
    static escapeHtml(str: string): string {
        return (str || "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    static getSubplatform(): string {
        return document.body.getAttribute("data-subplatform") ?? "";
    }

    static getProjectRedirectUrl(basePath: string): string {
        const params = new URLSearchParams(window.location.search);
        const projectId = params.get("projectId");
        const subplatform = DomUtils.getSubplatform();
        const prefix = subplatform ? `/${subplatform}` : "";

        return projectId
            ? `${prefix}/${basePath}?projectId=${projectId}`
            : `${prefix}/${basePath}`;
    }

    static getAntiForgeryToken(): string {
        return document.querySelector<HTMLInputElement>("input[name='__RequestVerificationToken']")?.value ?? "";
    }

    static openModal(id: string): void {
        const modal = document.getElementById(id);
        if (!modal) return;
        modal.style.display = "block";
        modal.classList.add("show");
        document.body.classList.add("modal-open");
    }

    static closeModal(id: string): void {
        const modal = document.getElementById(id);
        if (!modal) return;
        modal.style.display = "none";
        modal.classList.remove("show");
        document.body.classList.remove("modal-open");
    }
}
