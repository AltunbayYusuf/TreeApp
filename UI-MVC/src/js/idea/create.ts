// ideas/create.ts
import {DomUtils} from '../helpers/utils';

type IdeaResponse = {
    ok?: boolean;
    isToxic?: boolean;
    aiUnavailable?: boolean;
    warning?: string;
    explanation?: string;
    suggestedText?: string;
    message?: string;
};

export class IdeaCreator {
    private topicId: string = "";
    private title: string = "";
    private text: string = "";

    init(): void {
        document.getElementById("idea-contact-opt-in")?.addEventListener("change", this.toggleContactEmail.bind(this));
        document.getElementById("submit-idea-btn")?.addEventListener("click", this.handleSubmit.bind(this));

        // Initial state set
        this.toggleContactEmail();
    }

    private toggleContactEmail(): void {
        const contactOptIn = document.getElementById("idea-contact-opt-in") as HTMLInputElement | null;
        const contactEmailWrapper = document.getElementById("idea-contact-email-wrapper") as HTMLDivElement | null;
        const contactEmail = document.getElementById("idea-contact-email") as HTMLInputElement | null;

        const isChecked = contactOptIn?.checked ?? false;

        if (contactEmailWrapper) {
            contactEmailWrapper.style.display = isChecked ? "block" : "none";
        }

        if (!isChecked && contactEmail) {
            contactEmail.value = "";
        }
    }

    private clearAiMessage(): void {
        const aiMessage = document.getElementById("idea-ai-message") as HTMLDivElement | null;
        if (!aiMessage) return;
        aiMessage.style.display = "none";
        aiMessage.innerHTML = "";
    }

    private showError(message: string): void {
        const aiMessage = document.getElementById("idea-ai-message") as HTMLDivElement | null;
        if (!aiMessage) return;
        aiMessage.style.display = "block";
        aiMessage.innerHTML = `<div class="alert alert-danger mb-0">${DomUtils.escapeHtml(message)}</div>`;
    }

    private async handleSubmit(e: Event): Promise<void> {
        e.preventDefault();

        const ideaTopic = document.getElementById("idea-topic") as HTMLSelectElement | null;
        const ideaTitle = document.getElementById("idea-title") as HTMLInputElement | null;
        const ideaText = document.getElementById("idea-text") as HTMLTextAreaElement | null;

        this.topicId = ideaTopic?.value ?? "";
        this.title = ideaTitle?.value.trim() ?? "";
        this.text = ideaText?.value.trim() ?? "";

        this.clearAiMessage();

        if (!this.topicId || this.topicId === "0") {
            this.showError("Kies eerst een topic.");
            return;
        }

        if (!this.text) {
            this.showError("Beschrijf eerst je idee.");
            return;
        }

        const data = await this.postIdea("/api/ideas");

        if (!data.ok) {
            this.showError(data.message || "Er ging iets mis.");
            return;
        }

        if (data.isToxic) {
            this.showAiWarning(data);
            return;
        }

        if (data.aiUnavailable) {
            alert(data.message || "Je idee werd doorgestuurd voor moderatie.");
            window.location.href = DomUtils.getProjectRedirectUrl("Idea");
            return;
        }

        alert("Idee succesvol verstuurd.");
        window.location.href = DomUtils.getProjectRedirectUrl("Idea");
    }

    private async postIdea(url: string): Promise<IdeaResponse> {
        const contactOptIn = document.getElementById("idea-contact-opt-in") as HTMLInputElement | null;
        const contactEmail = document.getElementById("idea-contact-email") as HTMLInputElement | null;

        const response = await fetch(url, {
            method: "POST",
            headers: {"Content-Type": "application/json"},
            body: JSON.stringify({
                topicId: Number(this.topicId),
                title: this.title,
                text: this.text,
                contactOptIn: contactOptIn?.checked ?? false,
                email: contactEmail?.value.trim() ?? ""
            })
        });

        const data = await response.json() as IdeaResponse;
        if (!response.ok) {
            return {ok: false, aiUnavailable: data.aiUnavailable, message: data.message || "Er ging iets mis."};
        }
        return data;
    }

    private showAiWarning(data: IdeaResponse): void {
        const aiMessage = document.getElementById("idea-ai-message") as HTMLDivElement | null;
        if (!aiMessage) return;

        aiMessage.style.display = "block";
        aiMessage.innerHTML = `
            <div class="alert alert-warning mb-0">
                <strong>${DomUtils.escapeHtml(data.warning || "AI: je tekst bevat mogelijk toxische inhoud.")}</strong>
                ${data.explanation ? `<div class="mt-2"><em>${DomUtils.escapeHtml(data.explanation)}</em></div>` : ""}
                ${data.suggestedText ? `<div class="mt-2"><strong>Alternatief:</strong><br>${DomUtils.escapeHtml(data.suggestedText)}</div>` : ""}
                <div class="mt-3 d-flex gap-2 flex-wrap">
                    <button type="button" id="force-submit-btn" class="btn btn-outline-danger btn-sm">Toch versturen</button>
                    <button type="button" id="use-alternative-btn" class="btn btn-outline-primary btn-sm">Alternatief gebruiken</button>
                </div>
            </div>
        `;

        document.getElementById("force-submit-btn")?.addEventListener("click", this.handleForceSubmit.bind(this));
        document.getElementById("use-alternative-btn")?.addEventListener("click", () => {
            const ideaText = document.getElementById("idea-text") as HTMLTextAreaElement | null;
            if (ideaText) {
                ideaText.value = data.suggestedText || "";
                ideaText.focus();
            }
            this.clearAiMessage();
        });
    }

    private async handleForceSubmit(): Promise<void> {
        const forceResult = await this.postIdea("/api/ideas/force");
        if (!forceResult.ok) {
            this.showError(forceResult.message || "Er ging iets mis.");
            return;
        }

        this.clearAiMessage();
        alert("Idee doorgestuurd voor moderatie.");
        window.location.href = DomUtils.getProjectRedirectUrl("Idea");
    }
}

document.addEventListener("DOMContentLoaded", () => new IdeaCreator().init());