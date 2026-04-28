// ideas/create.ts
import {DomUtils} from "../helpers/utils";

type IdeaResponse = {
    ok?: boolean;
    isToxic?: boolean;
    aiUnavailable?: boolean;
    warning?: string;
    explanation?: string;
    suggestedTitle?: string;
    suggestedText?: string;
    message?: string;
};

type ImproveIdeaResponse = {
    ok?: boolean;
    improvedTitle?: string;
    improvedText?: string;
    message?: string;
};

export class IdeaCreator {
    private topicId = "";
    private title = "";
    private text = "";

  //  private aiAlternativeWasUsedd ;
    private aiAlternativeTitle = "";
    private aiAlternativeText = "";

    init(): void {
        document.getElementById("idea-contact-opt-in")?.addEventListener("change", this.toggleContactEmail.bind(this));
        document.getElementById("submit-idea-btn")?.addEventListener("click", this.handleSubmit.bind(this));
        document.getElementById("ai-improve-create-btn")?.addEventListener("click", this.handleImproveWithAi.bind(this));
        document.getElementById("use-ai-improvement-btn")?.addEventListener("click", this.useAiImprovement.bind(this));

      //  document.getElementById("idea-title")?.addEventListener("input", () => this.aiAlternativeWasUsedd = false);
       // document.getElementById("idea-text")?.addEventListener("input", () => this.aiAlternativeWasUsedd = false);

        this.toggleContactEmail();
    }

    private async handleImproveWithAi(): Promise<void> {
        const ideaTitle = document.getElementById("idea-title") as HTMLInputElement | null;
        const ideaText = document.getElementById("idea-text") as HTMLTextAreaElement | null;
        const improveButton = document.getElementById("ai-improve-create-btn") as HTMLButtonElement | null;
        const useButton = document.getElementById("use-ai-improvement-btn") as HTMLButtonElement | null;
        const resultBox = document.getElementById("ai-improve-result") as HTMLDivElement | null;
        const resultText = document.getElementById("ai-improve-text") as HTMLParagraphElement | null;

        if (!ideaText || !ideaTitle || !improveButton || !useButton || !resultBox || !resultText) return;

        if (!ideaText.value.trim()) {
            resultBox.style.display = "block";
            resultText.textContent = "Schrijf eerst een idee voordat je AI gebruikt.";
            useButton.style.display = "none";
            return;
        }

        improveButton.disabled = true;
        improveButton.textContent = "AI denkt na...";

        try {
            const response = await fetch("/api/ideas/improve", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
                body: JSON.stringify({
                    title: ideaTitle.value,
                    text: ideaText.value
                })
            });

            const data = await response.json() as ImproveIdeaResponse;

            resultBox.style.display = "block";

            if (!response.ok || !data.ok) {
                resultText.textContent = data.message ?? "AI kon geen alternatief maken.";
                useButton.style.display = "none";
                return;
            }

            this.aiAlternativeTitle = data.improvedTitle ?? "";
            this.aiAlternativeText = data.improvedText ?? "";

            resultText.textContent = `${this.aiAlternativeTitle}\n\n${this.aiAlternativeText}`;
            useButton.style.display = "inline-block";
        } catch {
            resultBox.style.display = "block";
            resultText.textContent = "AI-assistentie is tijdelijk niet beschikbaar.";
            useButton.style.display = "none";
        } finally {
            improveButton.disabled = false;
            improveButton.textContent = "✨ Herschrijf met AI";
        }
    }

    private useAiImprovement(): void {
        const ideaTitle = document.getElementById("idea-title") as HTMLInputElement | null;
        const ideaText = document.getElementById("idea-text") as HTMLTextAreaElement | null;
        const resultBox = document.getElementById("ai-improve-result") as HTMLDivElement | null;

        if (!ideaTitle || !ideaText) return;

        ideaTitle.value = this.aiAlternativeTitle;
        ideaText.value = this.aiAlternativeText;

        this.aiAlternativeWasUsed = true;

        if (resultBox) {
            resultBox.style.display = "none";
        }

        this.clearAiMessage();
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

        if (!this.title) {
            this.showError("Geef eerst een titel in.");
            return;
        }

        if (!this.text) {
            this.showError("Beschrijf eerst je idee.");
            return;
        }

        const data = await this.postIdea("/api/ideas", false);

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

    private async postIdea(url: string, skipAiModeration: boolean): Promise<IdeaResponse> {
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
                email: contactEmail?.value.trim() ?? "",
                skipAiModeration: skipAiModeration
            })
        });

        const data = await response.json() as IdeaResponse;

        if (!response.ok) {
            return {
                ok: false,
                aiUnavailable: data.aiUnavailable,
                message: data.message || "Er ging iets mis."
            };
        }

        return data;
    }

    private showAiWarning(data: IdeaResponse): void {
        const aiMessage = document.getElementById("idea-ai-message") as HTMLDivElement | null;
        if (!aiMessage) return;

        const suggestedTitle = data.suggestedTitle || this.title;
        const suggestedText = data.suggestedText || "";

        aiMessage.style.display = "block";
        aiMessage.innerHTML = `
            <div class="alert alert-warning mb-0">
                <strong>${DomUtils.escapeHtml(data.warning || "AI: je tekst bevat mogelijk toxische inhoud.")}</strong>
                ${data.explanation ? `<div class="mt-2"><em>${DomUtils.escapeHtml(data.explanation)}</em></div>` : ""}
                <div class="mt-2">
                    <strong>Alternatief:</strong><br>
                    <strong>${DomUtils.escapeHtml(suggestedTitle)}</strong><br>
                    ${DomUtils.escapeHtml(suggestedText)}
                </div>
                <div class="mt-3 d-flex gap-2 flex-wrap">
                    <button type="button" id="force-submit-btn" class="btn btn-outline-danger btn-sm">Toch versturen</button>
                    <button type="button" id="use-alternative-btn" class="btn btn-outline-primary btn-sm">Alternatief gebruiken</button>
                </div>
            </div>
        `;

        document.getElementById("force-submit-btn")?.addEventListener("click", this.handleForceSubmit.bind(this));

        document.getElementById("use-alternative-btn")?.addEventListener("click", () => {
            const ideaTitle = document.getElementById("idea-title") as HTMLInputElement | null;
            const ideaText = document.getElementById("idea-text") as HTMLTextAreaElement | null;

            this.aiAlternativeTitle = suggestedTitle;
            this.aiAlternativeText = suggestedText;

            if (ideaTitle) {
                ideaTitle.value = this.aiAlternativeTitle;
            }

            if (ideaText) {
                ideaText.value = this.aiAlternativeText;
                ideaText.focus();
            }

            this.aiAlternativeWasUsed = true;
            this.clearAiMessage();
        });
    }

    private async handleForceSubmit(): Promise<void> {
        const forceResult = await this.postIdea("/api/ideas/force", false);

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