type BootstrapModal = {
    show(): void;
    hide(): void;
};

type BootstrapModalConstructor = new (element: HTMLElement) => BootstrapModal;

type BootstrapWindow = Window & {
    bootstrap?: {
        Modal?: BootstrapModalConstructor;
    };
};

class AdminPromptsPage {
    private scrollContainer!: HTMLElement;
    private previousButton!: HTMLButtonElement;
    private nextButton!: HTMLButtonElement;
    private promptCards: HTMLFormElement[] = [];

    private modalElement!: HTMLElement;
    private modal: BootstrapModal | null = null;

    private cancelButton!: HTMLButtonElement;
    private discardButton!: HTMLButtonElement;
    private saveButton!: HTMLButtonElement;

    private activeCard: HTMLFormElement | null = null;
    private requestedCard: HTMLFormElement | null = null;

    private requestedScrollDirection: -1 | 1 | null = null;

    private readonly originalValues = new Map<HTMLFormElement, string>();

    init(): void {
        this.scrollContainer = document.getElementById("promptsScroll") as HTMLElement;
        this.previousButton = document.getElementById("promptPrev") as HTMLButtonElement;
        this.nextButton = document.getElementById("promptNext") as HTMLButtonElement;
        this.promptCards = Array.from(document.querySelectorAll<HTMLFormElement>("[data-prompt-card]"));

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
            const textarea = this.getPromptTextarea(card);

            if (textarea) {
                this.originalValues.set(card, textarea.value);
            }
        });
    }

    private initializeModal(): void {
        const modalConstructor = this.getBootstrapModalConstructor();

        if (modalConstructor && this.modalElement) {
            this.modal = new modalConstructor(this.modalElement);
        }
    }
    private requestScroll(direction: -1 | 1): void {
        if (this.activeCard && this.hasUnsavedChanges(this.activeCard)) {
            this.requestedScrollDirection = direction;
            this.showModal();
            return;
        }

        this.scrollPrompts(direction);
    }

    private getBootstrapModalConstructor(): BootstrapModalConstructor | null {
        return (window as BootstrapWindow).bootstrap?.Modal ?? null;
    }

    private bindEvents(): void {
        this.promptCards.forEach((card) => this.bindPromptCard(card));

        this.previousButton.addEventListener("click", () => this.requestScroll(-1));
        this.nextButton.addEventListener("click", () => this.requestScroll(1));

        this.cancelButton?.addEventListener("click", () => this.cancelSwitch());
        this.discardButton?.addEventListener("click", () => this.discardChangesAndSwitch());
        this.saveButton?.addEventListener("click", () => this.activeCard?.submit());
    }

    private bindPromptCard(card: HTMLFormElement): void {
        card.addEventListener("pointerdown", (event) => this.handleCardPointerDown(event, card));

        const title = card.querySelector<HTMLButtonElement>("[data-focus-prompt]");
        title?.addEventListener("click", () => this.requestSwitch(card));
    }

    private handleCardPointerDown(event: PointerEvent, card: HTMLFormElement): void {
        if (!this.shouldConfirmSwitch(card)) {
            this.setActiveCard(card);
            return;
        }

        event.preventDefault();
        this.requestedCard = card;
        this.showModal();
    }

    private scrollPrompts(direction: -1 | 1): void {
        this.scrollContainer.scrollBy({
            left: direction * this.scrollContainer.clientWidth,
            behavior: "smooth"
        });
    }

    private cancelSwitch(): void {
        this.requestedCard = null;
        this.hideModal();
    }

    private requestSwitch(card: HTMLFormElement): void {
        if (this.shouldConfirmSwitch(card)) {
            this.requestedCard = card;
            this.showModal();
            return;
        }

        this.setActiveCard(card);
    }

    private shouldConfirmSwitch(card: HTMLFormElement): boolean {
        return this.activeCard !== null
            && this.activeCard !== card
            && this.hasUnsavedChanges(this.activeCard);
    }

    private setActiveCard(card: HTMLFormElement): void {
        this.promptCards.forEach((promptCard) => {
            promptCard.classList.remove("active");
        });

        card.classList.add("active");
        this.activeCard = card;
    }

    private hasUnsavedChanges(card: HTMLFormElement): boolean {
        const textarea = this.getPromptTextarea(card);
        const originalValue = this.originalValues.get(card) ?? "";

        return textarea !== null && textarea.value !== originalValue;
    }

    private discardChangesAndSwitch(): void {
        if (this.activeCard) {
            const textarea = this.getPromptTextarea(this.activeCard);

            if (textarea) {
                textarea.value = this.originalValues.get(this.activeCard) ?? "";
            }
        }

        this.hideModal();

        if (this.requestedCard) {
            this.setActiveCard(this.requestedCard);
            this.requestedCard = null;
            return;
        }

        if (this.requestedScrollDirection) {
            this.scrollPrompts(this.requestedScrollDirection);
            this.requestedScrollDirection = null;
        }
    }

    private getPromptTextarea(card: HTMLFormElement): HTMLTextAreaElement | null {
        return card.querySelector<HTMLTextAreaElement>(".prompt-text");
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

export function initAdminPrompts(): void {
    new AdminPromptsPage().init();
}

initAdminPrompts();