class AdminPromptsPage {
    private scrollContainer!: HTMLElement;
    private previousButton!: HTMLButtonElement;
    private nextButton!: HTMLButtonElement;
    private promptCards: HTMLFormElement[] = [];

    private modalElement!: HTMLElement;
    private modal: any = null;

    private cancelButton!: HTMLButtonElement;
    private discardButton!: HTMLButtonElement;
    private saveButton!: HTMLButtonElement;

    private activeCard: HTMLFormElement | null = null;
    private requestedCard: HTMLFormElement | null = null;

    private originalValues = new Map<HTMLFormElement, string>();

    init(): void {
        this.scrollContainer = document.getElementById("promptsScroll") as HTMLElement;
        this.previousButton = document.getElementById("promptPrev") as HTMLButtonElement;
        this.nextButton = document.getElementById("promptNext") as HTMLButtonElement;
        this.promptCards = Array.from(document.querySelectorAll("[data-prompt-card]")) as HTMLFormElement[];

        this.modalElement = document.getElementById("unsavedPromptModal") as HTMLElement;
        this.cancelButton = document.getElementById("cancelSwitchPrompt") as HTMLButtonElement;
        this.discardButton = document.getElementById("discardPromptChanges") as HTMLButtonElement;
        this.saveButton = document.getElementById("savePromptChanges") as HTMLButtonElement;

        if (!this.scrollContainer || !this.previousButton || !this.nextButton || this.promptCards.length === 0) {
            return;
        }

        this.storeOriginalValues();
        this.initializeModal();
        this.bindEvents();
        this.setActiveCard(this.promptCards[0]);
    }

    private storeOriginalValues(): void {
        this.promptCards.forEach((card) => {
            const textarea = card.querySelector(".prompt-text") as HTMLTextAreaElement;

            this.originalValues.set(card, textarea.value);
        });
    }

    private initializeModal(): void {
        const bootstrapInstance = (window as any).bootstrap;

        if (bootstrapInstance?.Modal && this.modalElement) {
            this.modal = new bootstrapInstance.Modal(this.modalElement);
        }
    }

    private bindEvents(): void {
        this.promptCards.forEach((card) => {
            const title = card.querySelector("[data-focus-prompt]") as HTMLButtonElement;

            title?.addEventListener("click", () => {
                this.requestSwitch(card);
            });
        });

        this.previousButton.addEventListener("click", () => {
            this.scrollContainer.scrollBy({
                left: -this.scrollContainer.clientWidth,
                behavior: "smooth"
            });
        });

        this.nextButton.addEventListener("click", () => {
            this.scrollContainer.scrollBy({
                left: this.scrollContainer.clientWidth,
                behavior: "smooth"
            });
        });

        this.cancelButton?.addEventListener("click", () => {
            this.requestedCard = null;
            this.hideModal();
        });

        this.discardButton?.addEventListener("click", () => {
            this.discardChangesAndSwitch();
        });

        this.saveButton?.addEventListener("click", () => {
            this.activeCard?.submit();
        });
    }

    private requestSwitch(card: HTMLFormElement): void {
        if (this.activeCard && this.activeCard !== card && this.hasUnsavedChanges(this.activeCard)) {
            this.requestedCard = card;
            this.showModal();
            return;
        }

        this.setActiveCard(card);
    }

    private setActiveCard(card: HTMLFormElement): void {
        this.promptCards.forEach((promptCard) => {
            promptCard.classList.remove("active");
        });

        card.classList.add("active");
        this.activeCard = card;

        card.scrollIntoView({
            behavior: "smooth",
            inline: "center",
            block: "nearest"
        });
    }

    private hasUnsavedChanges(card: HTMLFormElement): boolean {
        const textarea = card.querySelector(".prompt-text") as HTMLTextAreaElement;
        const originalValue = this.originalValues.get(card) ?? "";

        return textarea.value !== originalValue;
    }

    private discardChangesAndSwitch(): void {
        if (!this.activeCard || !this.requestedCard) {
            this.hideModal();
            return;
        }

        const textarea = this.activeCard.querySelector(".prompt-text") as HTMLTextAreaElement;
        textarea.value = this.originalValues.get(this.activeCard) ?? "";

        this.hideModal();
        this.setActiveCard(this.requestedCard);
        this.requestedCard = null;
    }

    private showModal(): void {
        if (this.modal) {
            this.modal.show();
            return;
        }

        this.modalElement.style.display = "block";
        this.modalElement.classList.add("show");
    }

    private hideModal(): void {
        if (this.modal) {
            this.modal.hide();
            return;
        }

        this.modalElement.classList.remove("show");
        this.modalElement.style.display = "none";
    }
}

document.addEventListener("DOMContentLoaded", () => {
    new AdminPromptsPage().init();
});