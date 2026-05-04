// reaction/index.ts
import {DomUtils} from '../helpers/utils';

interface ReactionApiResponse {
    ok?: boolean;
    message?: string;
    warning?: string;
    explanation?: string;
    suggestedText?: string;
    isToxic?: boolean;
    saved?: boolean;
    added?: boolean;
    aiUnavailable?: boolean;
    count?: number;
}

export class ReactionHandler {
    init(): void {
        document.querySelectorAll<HTMLButtonElement>(".reaction-emoji-btn").forEach((btn) => {
            btn.addEventListener("click", this.handleEmojiSelection.bind(this));
        });

        document.querySelectorAll<HTMLFormElement>(".reaction-form").forEach((form) => {
            form.addEventListener("submit", this.handleFormSubmit.bind(this));
        });
    }

    private async handleEmojiSelection(e: MouseEvent): Promise<void> {
        const btn = e.currentTarget as HTMLButtonElement;
        const ideaId = btn.dataset.ideaId;
        const emoji = btn.dataset.emoji ?? "";

        if (!ideaId) return;

        const buttonsForIdea = document.querySelectorAll<HTMLButtonElement>(`.reaction-emoji-btn[data-idea-id='${ideaId}']`);
        const previouslySelectedButtons = Array.from(buttonsForIdea).filter((button) =>
            button.classList.contains("selected") && button !== btn
        );

        try {
            const response = await fetch("/api/reactions/toggle-emoji", {
                method: "POST",
                headers: {"Content-Type": "application/json", "Accept": "application/json"},
                body: JSON.stringify({ideaId: parseInt(ideaId, 10), emoji})
            });

            const data: ReactionApiResponse = await response.json();

            if (!response.ok) return;

            this.resetButtons(buttonsForIdea);

            if (data.added) {
                previouslySelectedButtons.forEach((button) => {
                    const previousEmoji = button.dataset.emoji ?? "";
                    this.adjustEmojiCount(ideaId, previousEmoji, -1);
                });
                btn.classList.add("selected", "btn-primary");
                btn.classList.remove("btn-outline-secondary");
            }

            this.updateEmojiCount(ideaId, emoji, data.count);
        } catch (error) {
            console.error("Fout bij togglen van emoji reactie:", error);
        }
    }

    private resetButtons(buttons: NodeListOf<HTMLButtonElement>): void {
        buttons.forEach((b) => {
            b.classList.remove("selected", "btn-primary");
            b.classList.add("btn-outline-secondary");
        });
    }

    private updateEmojiCount(ideaId: string, emoji: string, count?: number): void {
        if (typeof count !== "number") return;
        const countElement = document.querySelector<HTMLElement>(`.reaction-count[data-count-for='${ideaId}-${emoji}']`);
        if (countElement) {
            countElement.textContent = count.toString();
        }
    }

    private adjustEmojiCount(ideaId: string, emoji: string, delta: number): void {
        const countElement = document.querySelector<HTMLElement>(`.reaction-count[data-count-for='${ideaId}-${emoji}']`);
        if (!countElement) return;
        const currentCount = parseInt(countElement.textContent ?? "0", 10);
        countElement.textContent = Math.max(0, currentCount + delta).toString();
    }

    private async handleFormSubmit(e: SubmitEvent): Promise<void> {
        e.preventDefault();
        const form = e.currentTarget as HTMLFormElement;
        const ideaId = form.dataset.ideaId;

        if (!ideaId) return;

        const textArea = form.querySelector("textarea[name='text']") as HTMLTextAreaElement | null;
        if (!textArea) return;

        const text = textArea.value.trim();
        const resultBox = document.getElementById(`reaction-result-${ideaId}`) as HTMLDivElement | null;

        this.clearResultBox(resultBox);

        if (text === "") {
            this.showResultMsg(resultBox, "Schrijf een reactie.", "danger");
            return;
        }

        if (!confirm("Wil je deze reactie zeker verzenden?")) return;

        try {
            const response = await fetch("/api/reactions", {
                method: "POST",
                headers: {"Content-Type": "application/json", "Accept": "application/json"},
                body: JSON.stringify({ideaId: parseInt(ideaId, 10), text})
            });

            const data: ReactionApiResponse = await response.json();

            if (!response.ok) {
                this.showResultMsg(resultBox, data.message ?? "Er ging iets mis.", "danger");
                return;
            }

            if (data.isToxic) {
                this.handleToxicReaction(resultBox, data, ideaId, text, textArea);
                return;
            }

            if (data.aiUnavailable) {
                this.showResultMsg(resultBox, data.message ?? "Reactie doorgestuurd voor moderatie (AI onbeschikbaar).", "info");
                this.resetForm(textArea);
                return;
            }

            if (data.saved) {
                this.showResultMsg(resultBox, data.message ?? "Reactie succesvol toegevoegd.", "success");
                this.appendReactionToList(ideaId, text);
                this.resetForm(textArea);
            }
        } catch (error) {
            console.error("Fout bij verzenden van reactie:", error);
            this.showResultMsg(resultBox, "Er ging iets mis bij het verzenden.", "danger");
        }
    }

    private clearResultBox(box: HTMLDivElement | null): void {
        if (!box) return;
        box.style.display = "none";
        box.innerHTML = "";
    }

    private showResultMsg(box: HTMLDivElement | null, msg: string, type: "success" | "danger" | "info"): void {
        if (!box) return;
        box.style.display = "block";
        box.innerHTML = `<div class="alert alert-${type} mb-0">${DomUtils.escapeHtml(msg)}</div>`;
    }

    private resetForm(textArea: HTMLTextAreaElement): void {
        textArea.value = "";
    }

    private appendReactionToList(ideaId: string, text: string): void {
        const reactionsList = document.getElementById(`reactions-list-${ideaId}`) as HTMLUListElement | null;
        const noReactionsMessage = document.getElementById(`no-reactions-${ideaId}`) as HTMLElement | null;

        if (reactionsList) {
            const li = document.createElement("li");
            li.innerHTML = `<span>${DomUtils.escapeHtml(text)}</span>`;
            reactionsList.prepend(li);
            reactionsList.style.display = "block";
        }

        if (noReactionsMessage) {
            noReactionsMessage.style.display = "none";
            noReactionsMessage.remove();
        }
    }

    private handleToxicReaction(box: HTMLDivElement | null, data: ReactionApiResponse, ideaId: string, text: string, textArea: HTMLTextAreaElement): void {
        if (!box) return;

        box.style.display = "block";
        box.innerHTML = `
            <div class="alert alert-warning mb-0">
                <strong>${DomUtils.escapeHtml(data.warning ?? "AI: je reactie bevat mogelijk toxische inhoud.")}</strong>
                ${data.explanation ? `<div class="mt-2"><em>${DomUtils.escapeHtml(data.explanation)}</em></div>` : ""}
                ${data.suggestedText ? `<div class="mt-2"><strong>Alternatief:</strong><br>${DomUtils.escapeHtml(data.suggestedText)}</div>` : ""}
                <div class="mt-3 d-flex gap-2 flex-wrap">
                    <button type="button" class="btn btn-outline-primary btn-sm use-alternative-btn">Alternatief gebruiken</button>
                    <button type="button" class="btn btn-outline-danger btn-sm force-submit-btn">Toch versturen</button>
                </div>
            </div>
        `;

        box.querySelector(".force-submit-btn")?.addEventListener("click", async () => {
            const forceResponse = await fetch("/api/reactions/force", {
                method: "POST",
                headers: {"Content-Type": "application/json"},
                body: JSON.stringify({ideaId: parseInt(ideaId, 10), text})
            });

            const forceData: ReactionApiResponse = await forceResponse.json();

            if (!forceResponse.ok) {
                this.showResultMsg(box, forceData.message ?? "Fout bij toch versturen.", "danger");
                return;
            }

            this.showResultMsg(box, forceData.message ?? "Reactie doorgestuurd voor moderatie.", "info");
            this.resetForm(textArea);
        });

        box.querySelector(".use-alternative-btn")?.addEventListener("click", () => {
            textArea.value = data.suggestedText || "";
            this.clearResultBox(box);
            textArea.focus();
        });
    }
}

document.addEventListener("DOMContentLoaded", () => new ReactionHandler().init());
