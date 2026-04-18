const MIN_TOPICS = 1;
const MAX_TOPICS = 5;
const MIN_IDEAS_PER_BATCH = 3;
const MAX_IDEAS_PER_BATCH = 10;
const MIN_EXTRA_REQUESTS = 0;
const MAX_EXTRA_REQUESTS = 10;

type TopicFieldName = "Title" | "Description";

function getIdeationElements() {
    const form = document.getElementById("ideationForm") as HTMLFormElement | null;
    const topicsContainer = document.getElementById("topicsContainer") as HTMLDivElement | null;
    const addTopicButton = document.getElementById("addTopicButton") as HTMLButtonElement | null;
    const ideasPerBatchInput = document.getElementById("IdeasPerBatch") as HTMLInputElement | null;
    const maxExtraRequestsInput = document.getElementById("MaxExtraRequests") as HTMLInputElement | null;
    const validationSummary = form?.querySelector('[asp-validation-summary], [data-valmsg-summary="true"], .validation-summary-errors, .validation-summary-valid') as HTMLDivElement | null;

    return {
        form,
        topicsContainer,
        addTopicButton,
        ideasPerBatchInput,
        maxExtraRequestsInput,
        validationSummary
    };
}

function clamp(value: number, min: number, max: number): number {
    return Math.min(Math.max(value, min), max);
}

function getTopicCards(topicsContainer: HTMLDivElement): HTMLDivElement[] {
    return Array.from(topicsContainer.querySelectorAll(".topic-card")) as HTMLDivElement[];
}

function getValidationSpan(card: HTMLDivElement, fieldName: TopicFieldName): HTMLSpanElement | null {
    const selector = fieldName === "Title"
        ? ".topic-title-validation"
        : ".topic-description-validation";

    return card.querySelector(selector) as HTMLSpanElement | null;
}

function setFieldError(card: HTMLDivElement, fieldName: TopicFieldName, message: string): void {
    const validationSpan = getValidationSpan(card, fieldName);
    if (!validationSpan) {
        return;
    }

    validationSpan.textContent = message;
    validationSpan.classList.remove("field-validation-valid");
    validationSpan.classList.add("field-validation-error");
}

function clearFieldError(card: HTMLDivElement, fieldName: TopicFieldName): void {
    const validationSpan = getValidationSpan(card, fieldName);
    if (!validationSpan) {
        return;
    }

    validationSpan.textContent = "";
    validationSpan.classList.remove("field-validation-error");
    validationSpan.classList.add("field-validation-valid");
}

function setSummaryMessage(validationSummary: HTMLDivElement | null, message: string): void {
    if (!validationSummary) {
        return;
    }

    validationSummary.innerHTML = `<ul><li>${message}</li></ul>`;
    validationSummary.classList.remove("validation-summary-valid");
    validationSummary.classList.add("validation-summary-errors");
}

function clearSummaryMessage(validationSummary: HTMLDivElement | null): void {
    if (!validationSummary) {
        return;
    }

    validationSummary.innerHTML = "";
    validationSummary.classList.remove("validation-summary-errors");
    validationSummary.classList.add("validation-summary-valid");
}

function updateTopicIndexes(topicsContainer: HTMLDivElement, addTopicButton: HTMLButtonElement): void {
    const cards = getTopicCards(topicsContainer);

    cards.forEach((card, index) => {
        card.dataset.index = index.toString();

        const label = card.querySelector(".topic-label") as HTMLElement | null;
        const titleInput = card.querySelector(".topic-title") as HTMLInputElement | null;
        const descriptionInput = card.querySelector(".topic-description") as HTMLTextAreaElement | null;
        const titleValidation = card.querySelector(".topic-title-validation") as HTMLSpanElement | null;
        const descriptionValidation = card.querySelector(".topic-description-validation") as HTMLSpanElement | null;
        const removeButton = card.querySelector(".remove-topic-btn") as HTMLButtonElement | null;

        if (label) {
            label.textContent = `Topic ${index + 1}`;
        }

        if (titleInput) {
            titleInput.name = `Topics[${index}].Title`;
            titleInput.id = `Topics_${index}__Title`;
        }

        if (descriptionInput) {
            descriptionInput.name = `Topics[${index}].Description`;
            descriptionInput.id = `Topics_${index}__Description`;
        }

        if (titleValidation) {
            titleValidation.setAttribute("data-valmsg-for", `Topics[${index}].Title`);
        }

        if (descriptionValidation) {
            descriptionValidation.setAttribute("data-valmsg-for", `Topics[${index}].Description`);
        }

        if (removeButton) {
            const disabled = cards.length <= MIN_TOPICS;
            removeButton.disabled = disabled;
            removeButton.classList.toggle("opacity-50", disabled);
            removeButton.classList.toggle("cursor-not-allowed", disabled);
        }
    });

    const disableAddButton = cards.length >= MAX_TOPICS;
    addTopicButton.disabled = disableAddButton;
    addTopicButton.classList.toggle("opacity-50", disableAddButton);
    addTopicButton.classList.toggle("cursor-not-allowed", disableAddButton);
}

function createTopicCard(index: number): HTMLDivElement {
    const card = document.createElement("div");
    card.className = "topic-card rounded-2xl border border-slate-200 bg-slate-50 p-5";
    card.dataset.index = index.toString();

    card.innerHTML = `
        <div class="flex items-start justify-between mb-3">
            <h3 class="topic-label text-sm font-semibold text-slate-700">Topic ${index + 1}</h3>

            <button type="button"
                    class="remove-topic-btn inline-flex h-11 w-11 items-center justify-center rounded-lg border border-red-200 bg-white text-red-500 hover:bg-red-50 transition"
                    aria-label="Verwijder topic ${index + 1}">
                🗑
            </button>
        </div>

        <div class="flex gap-3 items-start">
            <div class="flex-1">
                <input name="Topics[${index}].Title"
                       id="Topics_${index}__Title"
                       class="topic-title w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 placeholder:text-slate-400 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100"
                       placeholder="bv. Acties om mentaal welzijn te verbeteren" />
                <span class="topic-title-validation mt-1 block text-sm text-red-600 field-validation-valid"
                      data-valmsg-for="Topics[${index}].Title"
                      data-valmsg-replace="true"></span>
            </div>
        </div>

        <div class="mt-4">
            <textarea name="Topics[${index}].Description"
                      id="Topics_${index}__Description"
                      rows="4"
                      class="topic-description w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 placeholder:text-slate-400 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100"
                      placeholder="Beschrijving/context voor dit topic (optioneel)..."></textarea>
            <span class="topic-description-validation mt-1 block text-sm text-red-600 field-validation-valid"
                  data-valmsg-for="Topics[${index}].Description"
                  data-valmsg-replace="true"></span>
        </div>
    `;

    return card;
}

function removeTopic(card: HTMLDivElement, topicsContainer: HTMLDivElement, addTopicButton: HTMLButtonElement): void {
    const cards = getTopicCards(topicsContainer);
    if (cards.length <= MIN_TOPICS) {
        return;
    }

    card.remove();
    updateTopicIndexes(topicsContainer, addTopicButton);
}

function addTopic(topicsContainer: HTMLDivElement, addTopicButton: HTMLButtonElement): void {
    const cards = getTopicCards(topicsContainer);
    if (cards.length >= MAX_TOPICS) {
        return;
    }

    const newCard = createTopicCard(cards.length);
    topicsContainer.appendChild(newCard);
    updateTopicIndexes(topicsContainer, addTopicButton);

    const titleInput = newCard.querySelector(".topic-title") as HTMLInputElement | null;
    titleInput?.focus();
}

function validateTopics(topicsContainer: HTMLDivElement, validationSummary: HTMLDivElement | null): boolean {
    const cards = getTopicCards(topicsContainer);

    if (cards.length < MIN_TOPICS || cards.length > MAX_TOPICS) {
        setSummaryMessage(validationSummary, `Het aantal topics moet tussen ${MIN_TOPICS} en ${MAX_TOPICS} liggen.`);
        return false;
    }

    let isValid = true;

    cards.forEach((card) => {
        const titleInput = card.querySelector(".topic-title") as HTMLInputElement | null;
        if (!titleInput) {
            return;
        }

        if (!titleInput.value.trim()) {
            setFieldError(card, "Title", "Een topic titel is verplicht.");
            if (isValid) {
                titleInput.focus();
            }
            isValid = false;
        } else {
            clearFieldError(card, "Title");
        }

        clearFieldError(card, "Description");
    });

    if (!isValid) {
        setSummaryMessage(validationSummary, "Vul voor elk topic een titel in.");
    } else {
        clearSummaryMessage(validationSummary);
    }

    return isValid;
}

function validateNumberInput(
    input: HTMLInputElement | null,
    min: number,
    max: number,
    message: string,
    validationSummary: HTMLDivElement | null
): boolean {
    if (!input) {
        return true;
    }

    if (input.value.trim() === "") {
        input.value = min.toString();
    }

    const value = Number(input.value);
    if (Number.isNaN(value) || value < min || value > max) {
        setSummaryMessage(validationSummary, message);
        input.focus();
        return false;
    }

    input.value = clamp(value, min, max).toString();
    return true;
}

function setupNumberInput(input: HTMLInputElement | null, min: number, max: number): void {
    if (!input) {
        return;
    }

    input.min = min.toString();
    input.max = max.toString();

    input.addEventListener("input", () => {
        if (input.value.trim() === "") {
            return;
        }

        const value = Number(input.value);
        if (!Number.isNaN(value)) {
            input.value = clamp(value, min, max).toString();
        }
    });

    input.addEventListener("blur", () => {
        if (input.value.trim() === "") {
            input.value = min.toString();
            return;
        }

        const value = Number(input.value);
        input.value = Number.isNaN(value)
            ? min.toString()
            : clamp(value, min, max).toString();
    });
}

function initializeCreateProjectIdeation(): void {
    const {
        form,
        topicsContainer,
        addTopicButton,
        ideasPerBatchInput,
        maxExtraRequestsInput,
        validationSummary
    } = getIdeationElements();

    if (!form || !topicsContainer || !addTopicButton) {
        return;
    }

    if (getTopicCards(topicsContainer).length === 0) {
        topicsContainer.appendChild(createTopicCard(0));
    }

    addTopicButton.addEventListener("click", () => {
        clearSummaryMessage(validationSummary);
        addTopic(topicsContainer, addTopicButton);
    });

    topicsContainer.addEventListener("click", (event) => {
        const target = event.target as HTMLElement | null;
        const removeButton = target?.closest(".remove-topic-btn") as HTMLButtonElement | null;
        if (!removeButton) {
            return;
        }

        const card = removeButton.closest(".topic-card") as HTMLDivElement | null;
        if (!card) {
            return;
        }

        clearSummaryMessage(validationSummary);
        removeTopic(card, topicsContainer, addTopicButton);
    });

    topicsContainer.addEventListener("input", (event) => {
        const target = event.target as HTMLElement | null;
        const card = target?.closest(".topic-card") as HTMLDivElement | null;
        if (!card) {
            return;
        }

        if (target instanceof HTMLInputElement && target.classList.contains("topic-title")) {
            clearFieldError(card, "Title");
        }

        if (target instanceof HTMLTextAreaElement && target.classList.contains("topic-description")) {
            clearFieldError(card, "Description");
        }
    });

    form.addEventListener("submit", (event) => {
        clearSummaryMessage(validationSummary);

        const topicsValid = validateTopics(topicsContainer, validationSummary);
        const ideasPerBatchValid = validateNumberInput(
            ideasPerBatchInput,
            MIN_IDEAS_PER_BATCH,
            MAX_IDEAS_PER_BATCH,
            `Aantal ideeën per keer moet tussen ${MIN_IDEAS_PER_BATCH} en ${MAX_IDEAS_PER_BATCH} liggen.`,
            validationSummary
        );
        const maxExtraRequestsValid = validateNumberInput(
            maxExtraRequestsInput,
            MIN_EXTRA_REQUESTS,
            MAX_EXTRA_REQUESTS,
            `Max keer extra opvragen moet tussen ${MIN_EXTRA_REQUESTS} en ${MAX_EXTRA_REQUESTS} liggen.`,
            validationSummary
        );

        if (!topicsValid || !ideasPerBatchValid || !maxExtraRequestsValid) {
            event.preventDefault();
        }
    });

    updateTopicIndexes(topicsContainer, addTopicButton);
    setupNumberInput(ideasPerBatchInput, MIN_IDEAS_PER_BATCH, MAX_IDEAS_PER_BATCH);
    setupNumberInput(maxExtraRequestsInput, MIN_EXTRA_REQUESTS, MAX_EXTRA_REQUESTS);
}

document.addEventListener("DOMContentLoaded", initializeCreateProjectIdeation);
