// ideation/create-project-ideation.ts
const MIN_TOPICS = 1;
const MAX_TOPICS = 5;
const MIN_IDEAS_PER_BATCH = 3;
const MAX_IDEAS_PER_BATCH = 10;
const MIN_EXTRA_REQUESTS = 0;
const MAX_EXTRA_REQUESTS = 10;

type TopicFieldName = "Title" | "Description";

export class ProjectIdeationBuilder {
    private topicsContainer = document.getElementById("topicsContainer") as HTMLDivElement | null;
    private validationSummary = document.querySelector('[asp-validation-summary], [data-valmsg-summary="true"], .validation-summary-errors, .validation-summary-valid') as HTMLDivElement | null;

    init(): void {
        document.getElementById("addTopicButton")?.addEventListener("click", this.handleAddTopic.bind(this));
        document.getElementById("ideationForm")?.addEventListener("submit", this.handleSubmit.bind(this));

        this.topicsContainer?.addEventListener("click", this.handleTopicContainerClick.bind(this));
        this.topicsContainer?.addEventListener("input", this.handleTopicContainerInput.bind(this));

        const ideasPerBatchInput = document.getElementById("IdeasPerBatch") as HTMLInputElement | null;
        const maxExtraRequestsInput = document.getElementById("MaxExtraRequests") as HTMLInputElement | null;

        this.setupNumberInput(ideasPerBatchInput, MIN_IDEAS_PER_BATCH, MAX_IDEAS_PER_BATCH);
        this.setupNumberInput(maxExtraRequestsInput, MIN_EXTRA_REQUESTS, MAX_EXTRA_REQUESTS);

        if (this.getTopicCards().length === 0 && this.topicsContainer) {
            this.topicsContainer.appendChild(this.createTopicCard(0));
        }

        this.updateTopicIndexes();
    }

    private clamp(value: number, min: number, max: number): number {
        return Math.min(Math.max(value, min), max);
    }

    private getTopicCards(): HTMLDivElement[] {
        if (!this.topicsContainer) return [];
        return Array.from(this.topicsContainer.querySelectorAll(".topic-card")) as HTMLDivElement[];
    }

    private handleTopicContainerClick(event: MouseEvent): void {
        const target = event.target as HTMLElement | null;
        const removeButton = target?.closest(".remove-topic-btn") as HTMLButtonElement | null;
        const card = removeButton?.closest(".topic-card") as HTMLDivElement | null;

        if (removeButton && card) {
            this.clearSummaryMessage();
            this.removeTopic(card);
        }
    }

    private handleTopicContainerInput(event: Event): void {
        const target = event.target as HTMLElement | null;
        const card = target?.closest(".topic-card") as HTMLDivElement | null;

        if (!card) return;

        if (target instanceof HTMLInputElement && target.classList.contains("topic-title")) {
            this.clearFieldError(card, "Title");
        } else if (target instanceof HTMLTextAreaElement && target.classList.contains("topic-description")) {
            this.clearFieldError(card, "Description");
        }
    }

    private handleAddTopic(): void {
        this.clearSummaryMessage();

        const cards = this.getTopicCards();
        if (cards.length >= MAX_TOPICS || !this.topicsContainer) return;

        const newCard = this.createTopicCard(cards.length);
        this.topicsContainer.appendChild(newCard);
        this.updateTopicIndexes();

        newCard.querySelector<HTMLInputElement>(".topic-title")?.focus();
    }

    private removeTopic(card: HTMLDivElement): void {
        const cards = this.getTopicCards();
        if (cards.length <= MIN_TOPICS) return;

        card.remove();
        this.updateTopicIndexes();
    }

    private updateTopicIndexes(): void {
        const cards = this.getTopicCards();
        const addTopicButton = document.getElementById("addTopicButton") as HTMLButtonElement | null;

        cards.forEach((card, index) => {
            card.dataset.index = index.toString();

            const label = card.querySelector(".topic-label");
            const titleInput = card.querySelector(".topic-title") as HTMLInputElement | null;
            const descriptionInput = card.querySelector(".topic-description") as HTMLTextAreaElement | null;
            const titleValidation = card.querySelector(".topic-title-validation");
            const descriptionValidation = card.querySelector(".topic-description-validation");
            const removeButton = card.querySelector(".remove-topic-btn") as HTMLButtonElement | null;

            if (label) label.textContent = `Topic ${index + 1}`;
            if (titleInput) {
                titleInput.name = `Topics[${index}].Title`;
                titleInput.id = `Topics_${index}__Title`;
            }
            if (descriptionInput) {
                descriptionInput.name = `Topics[${index}].Description`;
                descriptionInput.id = `Topics_${index}__Description`;
            }
            if (titleValidation) titleValidation.setAttribute("data-valmsg-for", `Topics[${index}].Title`);
            if (descriptionValidation) descriptionValidation.setAttribute("data-valmsg-for", `Topics[${index}].Description`);

            if (removeButton) {
                const disabled = cards.length <= MIN_TOPICS;
                removeButton.disabled = disabled;
                removeButton.classList.toggle("opacity-50", disabled);
                removeButton.classList.toggle("cursor-not-allowed", disabled);
            }
        });

        if (addTopicButton) {
            const disableAddButton = cards.length >= MAX_TOPICS;
            addTopicButton.disabled = disableAddButton;
            addTopicButton.classList.toggle("opacity-50", disableAddButton);
            addTopicButton.classList.toggle("cursor-not-allowed", disableAddButton);
        }
    }

    private createTopicCard(index: number): HTMLDivElement {
        const card = document.createElement("div");
        card.className = "topic-card rounded-2xl border border-slate-200 bg-slate-50 p-5";
        card.dataset.index = index.toString();

        card.innerHTML = `
            <div class="flex items-start justify-between mb-3">
                <h3 class="topic-label text-sm font-semibold text-slate-700">Topic ${index + 1}</h3>
                <button type="button" class="remove-topic-btn inline-flex h-11 w-11 items-center justify-center rounded-lg border border-red-200 bg-white text-red-500 hover:bg-red-50 transition" aria-label="Verwijder topic ${index + 1}">🗑</button>
            </div>
            <div class="flex gap-3 items-start">
                <div class="flex-1">
                    <input name="Topics[${index}].Title" id="Topics_${index}__Title" class="topic-title w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 placeholder:text-slate-400 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100" placeholder="bv. Acties om mentaal welzijn te verbeteren" />
                    <span class="topic-title-validation mt-1 block text-sm text-red-600 field-validation-valid" data-valmsg-for="Topics[${index}].Title" data-valmsg-replace="true"></span>
                </div>
            </div>
            <div class="mt-4">
                <textarea name="Topics[${index}].Description" id="Topics_${index}__Description" rows="4" class="topic-description w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 placeholder:text-slate-400 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100" placeholder="Beschrijving/context voor dit topic (optioneel)..."></textarea>
                <span class="topic-description-validation mt-1 block text-sm text-red-600 field-validation-valid" data-valmsg-for="Topics[${index}].Description" data-valmsg-replace="true"></span>
            </div>
        `;
        return card;
    }

    private getValidationSpan(card: HTMLDivElement, fieldName: TopicFieldName): HTMLSpanElement | null {
        const selector = fieldName === "Title" ? ".topic-title-validation" : ".topic-description-validation";
        return card.querySelector(selector) as HTMLSpanElement | null;
    }

    private setFieldError(card: HTMLDivElement, fieldName: TopicFieldName, message: string): void {
        const span = this.getValidationSpan(card, fieldName);
        if (!span) return;
        span.textContent = message;
        span.classList.remove("field-validation-valid");
        span.classList.add("field-validation-error");
    }

    private clearFieldError(card: HTMLDivElement, fieldName: TopicFieldName): void {
        const span = this.getValidationSpan(card, fieldName);
        if (!span) return;
        span.textContent = "";
        span.classList.remove("field-validation-error");
        span.classList.add("field-validation-valid");
    }

    private setSummaryMessage(message: string): void {
        if (!this.validationSummary) return;
        this.validationSummary.innerHTML = `<ul><li>${message}</li></ul>`;
        this.validationSummary.classList.remove("validation-summary-valid");
        this.validationSummary.classList.add("validation-summary-errors");
    }

    private clearSummaryMessage(): void {
        if (!this.validationSummary) return;
        this.validationSummary.innerHTML = "";
        this.validationSummary.classList.remove("validation-summary-errors");
        this.validationSummary.classList.add("validation-summary-valid");
    }

    private setupNumberInput(input: HTMLInputElement | null, min: number, max: number): void {
        if (!input) return;
        input.min = min.toString();
        input.max = max.toString();

        input.addEventListener("input", () => {
            if (input.value.trim() === "") return;
            const val = Number(input.value);
            if (!Number.isNaN(val)) input.value = this.clamp(val, min, max).toString();
        });

        input.addEventListener("blur", () => {
            if (input.value.trim() === "") {
                input.value = min.toString();
                return;
            }
            const val = Number(input.value);
            input.value = Number.isNaN(val) ? min.toString() : this.clamp(val, min, max).toString();
        });
    }

    private validateNumberInput(input: HTMLInputElement | null, min: number, max: number, message: string): boolean {
        if (!input) return true;
        if (input.value.trim() === "") input.value = min.toString();

        const val = Number(input.value);
        if (Number.isNaN(val) || val < min || val > max) {
            this.setSummaryMessage(message);
            input.focus();
            return false;
        }

        input.value = this.clamp(val, min, max).toString();
        return true;
    }

    private handleSubmit(event: Event): void {
        this.clearSummaryMessage();

        const cards = this.getTopicCards();
        let topicsValid = true;

        if (cards.length < MIN_TOPICS || cards.length > MAX_TOPICS) {
            this.setSummaryMessage(`Het aantal topics moet tussen ${MIN_TOPICS} en ${MAX_TOPICS} liggen.`);
            topicsValid = false;
        }

        cards.forEach((card) => {
            const titleInput = card.querySelector(".topic-title") as HTMLInputElement | null;
            if (titleInput && !titleInput.value.trim()) {
                this.setFieldError(card, "Title", "Een topic titel is verplicht.");
                if (topicsValid) titleInput.focus(); // Focus first invalid
                topicsValid = false;
            } else {
                this.clearFieldError(card, "Title");
            }
            this.clearFieldError(card, "Description");
        });

        if (!topicsValid) {
            this.setSummaryMessage("Vul voor elk topic een titel in.");
        }

        const ideasPerBatchInput = document.getElementById("IdeasPerBatch") as HTMLInputElement | null;
        const maxExtraRequestsInput = document.getElementById("MaxExtraRequests") as HTMLInputElement | null;

        const ideasValid = this.validateNumberInput(ideasPerBatchInput, MIN_IDEAS_PER_BATCH, MAX_IDEAS_PER_BATCH, `Aantal ideeën per keer moet tussen ${MIN_IDEAS_PER_BATCH} en ${MAX_IDEAS_PER_BATCH} liggen.`);
        const extraValid = this.validateNumberInput(maxExtraRequestsInput, MIN_EXTRA_REQUESTS, MAX_EXTRA_REQUESTS, `Max keer extra opvragen moet tussen ${MIN_EXTRA_REQUESTS} en ${MAX_EXTRA_REQUESTS} liggen.`);

        if (!topicsValid || !ideasValid || !extraValid) {
            event.preventDefault();
        }
    }
}

document.addEventListener("DOMContentLoaded", () => new ProjectIdeationBuilder().init());