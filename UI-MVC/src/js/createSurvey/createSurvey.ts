import { DomUtils } from "../helpers/utils";
import type { ConditionalData, QuestionData, SectionData } from "../helpers/types";

type SaveSurveyResponse = {
    ok: boolean;
    message?: string;
    redirectUrl?: string;
};

type GenerateSurveyResponse = {
    ok: boolean;
    message?: string;
    survey?: {
        sections: SectionData[];
    };
};

type QuestionType = "single" | "multiple" | "range" | "open" | "";

export class SurveyBuilder {
    private questionCount = 0;
    private readonly maxQuestions = 20;
    private sectionCount = 0;
    private isLoading = false;
    private saveTimeout?: number;

    init(): void {
        const form = document.getElementById("surveyForm") as HTMLFormElement | null;
        const sectionsContainer = document.getElementById("sections");

        if (!form || !sectionsContainer) {
            return;
        }

        this.bindEvents(form);
        this.loadInitialData();
        this.updateCounter();
    }

    private bindEvents(form: HTMLFormElement): void {
        document.getElementById("addSectionBtn")?.addEventListener("click", () => {
            this.addSection();
        });

        document.getElementById("generateSurveyWithAiBtn")?.addEventListener("click", async () => {
            await this.generateSurveyWithAi();
        });

        form.addEventListener("submit", async (event) => {
            event.preventDefault();

            const submitter = (event as SubmitEvent).submitter as HTMLElement | null;
            await this.handleSave(form, submitter);
        });

        form.addEventListener("input", () => {
            this.handleInputDebounce();
        });
    }

    private loadInitialData(): void {
        const initialData = this.readInitialServerData();

        if (this.hasQuestions(initialData)) {
            sessionStorage.setItem("surveyDraft", JSON.stringify(initialData));
            this.loadFromLocalStorage(initialData);
            return;
        }

        const storedData = sessionStorage.getItem("surveyDraft");

        if (!storedData) {
            this.createInitialSurvey();
            return;
        }

        const parsedData = this.parseSections(storedData);

        if (this.hasQuestions(parsedData)) {
            this.loadFromLocalStorage(parsedData);
            return;
        }

        sessionStorage.removeItem("surveyDraft");
        this.createInitialSurvey();
    }

    private readInitialServerData(): SectionData[] {
        const element = document.getElementById("initialSurveyData");
        const text = element?.textContent?.trim();

        if (!text) {
            return [];
        }

        return this.parseSections(text);
    }

    private parseSections(value: string): SectionData[] {
        try {
            const parsed = JSON.parse(value) as SectionData[];
            return Array.isArray(parsed) ? parsed : [];
        } catch {
            return [];
        }
    }

    private hasQuestions(sections: SectionData[]): boolean {
        return sections.some(section => section.questions?.length > 0);
    }

    private async handleSave(form: HTMLFormElement, submitter: HTMLElement | null): Promise<void> {
        const url = form.dataset.saveUrl;
        const errorBox = document.getElementById("surveyError") as HTMLDivElement | null;

        if (!url) {
            return;
        }

        this.hideError(errorBox);

        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": DomUtils.getAntiForgeryToken()
            },
            body: JSON.stringify({ sections: this.getSurveyData() })
        });

        const result = await response.json() as SaveSurveyResponse;

        if (!response.ok || !result.ok) {
            this.showError(errorBox, result.message ?? "Er is iets fout gegaan.");
            return;
        }

        if (submitter?.hasAttribute("data-final-save")) {
            await this.finalizeProject(form);
            return;
        }

        const redirectUrl = submitter?.dataset.redirectUrl ?? result.redirectUrl;

        if (redirectUrl) {
            window.location.href = redirectUrl;
        }
    }

    private async finalizeProject(form: HTMLFormElement): Promise<void> {
        const finalizeUrl = form.dataset.finalizeUrl;

        if (!finalizeUrl) {
            window.alert("Finalize-url ontbreekt.");
            return;
        }

        const response = await fetch(finalizeUrl, {
            method: "POST",
            headers: {
                "RequestVerificationToken": DomUtils.getAntiForgeryToken()
            }
        });

        if (!response.ok) {
            window.alert("Project kon niet opgeslagen worden. Controleer of alle tabs ingevuld zijn.");
            return;
        }

        window.location.href = response.url;
    }

    private handleInputDebounce(): void {
        if (this.saveTimeout !== undefined) {
            window.clearTimeout(this.saveTimeout);
        }

        this.saveTimeout = window.setTimeout(() => {
            this.saveToLocalStorage();
        }, 300);
    }

    private updateCounter(): void {
        const counter = document.getElementById("questionCounter");

        if (counter) {
            counter.textContent = `${this.questionCount} / ${this.maxQuestions} vragen`;
        }
    }

    private addSection(): void {
        const container = document.getElementById("sections");

        if (!container) {
            return;
        }

        this.sectionCount++;

        const section = document.createElement("div");
        section.className = "section card mb-3";

        const cardBody = document.createElement("div");
        cardBody.className = "card-body";

        const header = document.createElement("div");
        header.className = "d-flex justify-content-between align-items-center gap-3 mb-3";

        const titleInput = document.createElement("input");
        titleInput.type = "text";
        titleInput.className = "section-title form-control form-control-sm";
        titleInput.placeholder = `Sectie ${this.sectionCount} titel...`;

        const removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.className = "btn btn-outline-danger btn-sm flex-shrink-0";
        removeButton.textContent = "🗑";
        removeButton.addEventListener("click", () => {
            this.removeSection(removeButton);
        });

        const questionsContainer = document.createElement("div");
        questionsContainer.className = "questions d-flex flex-column gap-3";

        const addQuestionButton = document.createElement("button");
        addQuestionButton.type = "button";
        addQuestionButton.className = "add-question-btn btn btn-outline-primary btn-sm mt-3";
        addQuestionButton.textContent = "+ Vraag toevoegen";
        addQuestionButton.addEventListener("click", () => {
            this.addQuestion(addQuestionButton);
        });

        header.append(titleInput, removeButton);
        cardBody.append(header, questionsContainer, addQuestionButton);
        section.appendChild(cardBody);
        container.appendChild(section);

        this.saveToLocalStorage();
    }

    private removeSection(button: HTMLElement): void {
        const sections = document.querySelectorAll(".section");

        if (sections.length <= 1) {
            window.alert("Er moet minstens 1 sectie zijn");
            return;
        }

        const section = button.closest(".section");

        if (!section) {
            return;
        }

        this.questionCount -= section.querySelectorAll(".question").length;
        section.remove();

        this.updateCounter();
        this.saveToLocalStorage();
    }

    private addQuestion(button: HTMLElement): void {
        if (this.questionCount >= this.maxQuestions) {
            window.alert(`Max ${this.maxQuestions} vragen toegestaan`);
            return;
        }

        const section = button.closest(".section");
        const container = section?.querySelector(".questions");

        if (!container) {
            return;
        }

        this.questionCount++;
        this.updateCounter();

        const question = document.createElement("div");
        question.className = "question border rounded p-3";

        const header = document.createElement("div");
        header.className = "d-flex justify-content-between align-items-center mb-2";

        const label = document.createElement("span");
        label.className = "text-muted small";
        label.textContent = "Vraag";

        const removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.className = "btn btn-outline-danger btn-sm";
        removeButton.textContent = "🗑";
        removeButton.addEventListener("click", () => {
            this.removeQuestion(removeButton);
        });

        const titleInput = document.createElement("input");
        titleInput.type = "text";
        titleInput.className = "question-title form-control form-control-sm mb-3";
        titleInput.placeholder = "Vraag titel...";

        const typeSelect = this.createQuestionTypeSelect();

        const answersContainer = document.createElement("div");
        answersContainer.className = "answers d-flex flex-column gap-2";

        const conditionalButton = document.createElement("button");
        conditionalButton.type = "button";
        conditionalButton.className = "add-conditional-btn btn btn-link btn-sm mt-2 p-0 text-decoration-none";
        conditionalButton.textContent = "+ Conditionele vraag";
        conditionalButton.addEventListener("click", () => {
            this.addConditional(conditionalButton);
        });

        const conditionalContainer = document.createElement("div");
        conditionalContainer.className = "conditional-container";

        header.append(label, removeButton);
        question.append(titleInput, typeSelect, answersContainer, conditionalButton, conditionalContainer);
        container.appendChild(question);

        this.saveToLocalStorage();
    }

    private createQuestionTypeSelect(): HTMLSelectElement {
        const select = document.createElement("select");
        select.className = "form-select form-select-sm mb-3";

        [
            { value: "", text: "Kies type vraag...", disabled: true, selected: true },
            { value: "single", text: "Enkelkeuze" },
            { value: "multiple", text: "Meerkeuze" },
            { value: "range", text: "Range" },
            { value: "open", text: "Open vraag" }
        ].forEach(optionData => {
            const option = document.createElement("option");
            option.value = optionData.value;
            option.textContent = optionData.text;
            option.disabled = optionData.disabled ?? false;
            option.selected = optionData.selected ?? false;
            select.appendChild(option);
        });

        select.addEventListener("change", () => {
            this.changeType(select);
        });

        return select;
    }

    private removeQuestion(button: HTMLElement): void {
        if (this.questionCount <= 1) {
            window.alert("Er moet minstens 1 vraag zijn");
            return;
        }

        const question = button.closest(".question");

        if (!question) {
            return;
        }

        question.remove();
        this.questionCount--;

        this.updateCounter();
        this.saveToLocalStorage();
    }

    private changeType(select: HTMLSelectElement): void {
        const question = select.closest(".question") as HTMLElement | null;
        const container = question?.querySelector(".answers") as HTMLElement | null;
        const questionType = select.value as QuestionType;

        if (!question || !container) {
            return;
        }

        container.innerHTML = "";

        if (questionType === "single" || questionType === "multiple") {
            this.createChoiceAnswers(container);
        }

        if (questionType === "range") {
            this.createRangeInputs(container);
        }

        question.querySelectorAll<HTMLElement>(".conditional-block").forEach(conditional => {
            this.updateConditionalTriggerType(question, conditional);

            const triggerSelect = conditional.querySelector(".conditional-trigger") as HTMLSelectElement | null;

            if (triggerSelect) {
                this.populateConditionalTriggerOptions(question, triggerSelect);
            }
        });

        this.saveToLocalStorage();
    }

    private createChoiceAnswers(container: HTMLElement): void {
        const wrapper = document.createElement("div");
        wrapper.className = "answers-list d-flex flex-column gap-2";

        this.addAnswer(wrapper);
        this.addAnswer(wrapper);

        const button = document.createElement("button");
        button.type = "button";
        button.textContent = "+ Antwoord";
        button.className = "btn btn-link btn-sm mt-2 p-0 text-decoration-none";
        button.addEventListener("click", () => {
            this.addAnswer(wrapper);
        });

        container.append(wrapper, button);
    }

    private createRangeInputs(container: HTMLElement): void {
        const wrapper = document.createElement("div");
        wrapper.className = "d-flex gap-2";

        const minInput = document.createElement("input");
        minInput.type = "number";
        minInput.placeholder = "Min";
        minInput.className = "form-control form-control-sm";

        const maxInput = document.createElement("input");
        maxInput.type = "number";
        maxInput.placeholder = "Max";
        maxInput.className = "form-control form-control-sm";

        [minInput, maxInput].forEach(input => {
            input.addEventListener("input", () => {
                const question = input.closest(".question") as HTMLElement | null;

                if (!question) {
                    return;
                }

                question.querySelectorAll<HTMLSelectElement>(".conditional-trigger").forEach(triggerSelect => {
                    this.populateConditionalTriggerOptions(question, triggerSelect);
                });
            });
        });

        wrapper.append(minInput, maxInput);
        container.appendChild(wrapper);
    }

    private createInitialSurvey(): void {
        this.isLoading = true;
        this.addSection();

        const firstSection = document.querySelector(".section");
        const addQuestionButton = firstSection?.querySelector(".add-question-btn") as HTMLElement | null;

        if (addQuestionButton) {
            this.addQuestion(addQuestionButton);
        }

        this.isLoading = false;
        this.saveToLocalStorage();
    }

    private addAnswer(container: HTMLElement): void {
        const wrapper = document.createElement("div");
        wrapper.className = "d-flex align-items-center gap-2";

        const input = document.createElement("input");
        input.placeholder = "Antwoord...";
        input.className = "form-control form-control-sm";

        input.addEventListener("input", () => {
            const question = input.closest(".question") as HTMLElement | null;

            if (!question) {
                return;
            }

            question.querySelectorAll<HTMLSelectElement>(".conditional-trigger").forEach(triggerSelect => {
                const currentValue = triggerSelect.value;
                this.populateConditionalTriggerOptions(question, triggerSelect);
                triggerSelect.value = currentValue;
            });
        });

        const removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.textContent = "🗑";
        removeButton.className = "btn btn-outline-danger btn-sm flex-shrink-0";
        removeButton.addEventListener("click", () => {
            if (container.children.length <= 2) {
                window.alert("Minstens 2 antwoorden vereist");
                return;
            }

            wrapper.remove();
            this.saveToLocalStorage();
        });

        wrapper.append(input, removeButton);
        container.appendChild(wrapper);

        this.saveToLocalStorage();
    }

    private addConditional(button: HTMLElement): void {
        const question = button.closest(".question") as HTMLElement | null;
        const container = question?.querySelector(".conditional-container");

        if (!container || !question) {
            return;
        }

        const conditional = document.createElement("div");
        conditional.className = "conditional-block border rounded p-3 mt-3 bg-light";

        const header = document.createElement("div");
        header.className = "d-flex justify-content-between align-items-center mb-2";

        const title = document.createElement("span");
        title.className = "text-primary small fw-semibold text-uppercase";
        title.textContent = "Conditionele vraag";

        const removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.className = "btn btn-outline-danger btn-sm";
        removeButton.title = "Verwijder conditionele vraag";
        removeButton.textContent = "🗑";
        removeButton.addEventListener("click", () => {
            this.removeConditional(removeButton);
        });

        const triggerTypeContainer = document.createElement("div");
        triggerTypeContainer.className = "conditional-trigger-type-container mb-2";

        const triggerSelect = document.createElement("select");
        triggerSelect.className = "conditional-trigger form-select form-select-sm mb-2";

        const conditionalInput = document.createElement("input");
        conditionalInput.className = "conditional-input form-control form-control-sm";
        conditionalInput.placeholder = "Conditionele vraag...";

        header.append(title, removeButton);
        conditional.append(header, triggerTypeContainer, triggerSelect, conditionalInput);
        container.appendChild(conditional);

        this.updateConditionalTriggerType(question, conditional);
        this.populateConditionalTriggerOptions(question, triggerSelect);
        this.saveToLocalStorage();
    }

    private populateConditionalTriggerOptions(question: HTMLElement, triggerSelect: HTMLSelectElement): void {
        triggerSelect.innerHTML = "";

        const typeSelect = question.querySelector("select") as HTMLSelectElement | null;
        const questionType = typeSelect?.value as QuestionType;

        if (questionType === "range") {
            this.populateRangeTriggerOptions(question, triggerSelect);
            return;
        }

        this.populateAnswerTriggerOptions(question, triggerSelect);
    }

    private populateRangeTriggerOptions(question: HTMLElement, triggerSelect: HTMLSelectElement): void {
        const minInput = question.querySelector('input[placeholder="Min"]') as HTMLInputElement | null;
        const maxInput = question.querySelector('input[placeholder="Max"]') as HTMLInputElement | null;

        const min = Number(minInput?.value || 1);
        const max = Number(maxInput?.value || 5);

        if (!Number.isFinite(min) || !Number.isFinite(max) || min > max) {
            this.appendOption(triggerSelect, "", "Vul eerst een geldige range in");
            return;
        }

        for (let value = min; value <= max; value++) {
            this.appendOption(triggerSelect, value.toString(), value.toString());
        }
    }

    private populateAnswerTriggerOptions(question: HTMLElement, triggerSelect: HTMLSelectElement): void {
        const answerInputs = question.querySelectorAll<HTMLInputElement>(".answers-list input");

        answerInputs.forEach(input => {
            const value = input.value.trim();

            if (value) {
                this.appendOption(triggerSelect, value, value);
            }
        });

        if (triggerSelect.options.length === 0) {
            this.appendOption(triggerSelect, "", "Vul eerst antwoordopties in");
        }
    }

    private appendOption(select: HTMLSelectElement, value: string, text: string): void {
        const option = document.createElement("option");
        option.value = value;
        option.textContent = text;
        select.appendChild(option);
    }

    private removeConditional(button: HTMLElement): void {
        const conditionalBlock = button.closest(".conditional-block");

        if (!conditionalBlock) {
            return;
        }

        conditionalBlock.remove();
        this.saveToLocalStorage();
    }

    private getSurveyData(): SectionData[] {
        return Array.from(document.querySelectorAll<HTMLElement>(".section")).map(section => {
            const title = section.querySelector<HTMLInputElement>(".section-title")?.value ?? "";

            const questions = Array.from(section.querySelectorAll<HTMLElement>(".question")).map(question => {
                return this.getQuestionData(question);
            });

            return { title, questions };
        });
    }

    private getQuestionData(question: HTMLElement): QuestionData {
        const title = question.querySelector<HTMLInputElement>(".question-title")?.value ?? "";
        const type = question.querySelector<HTMLSelectElement>("select")?.value as QuestionType;
        const answers = Array.from(question.querySelectorAll<HTMLInputElement>(".answers-list input"))
            .map(input => input.value);

        const min = question.querySelector<HTMLInputElement>('input[placeholder="Min"]')?.value ?? "";
        const max = question.querySelector<HTMLInputElement>('input[placeholder="Max"]')?.value ?? "";
        const conditionals = this.getConditionals(question, type);

        return { title, type, answers, min, max, conditionals };
    }

    private getConditionals(question: HTMLElement, type: QuestionType): ConditionalData[] {
        return Array.from(question.querySelectorAll<HTMLElement>(".conditional-container > .conditional-block"))
            .map(conditional => {
                const trigger = conditional.querySelector<HTMLSelectElement>(".conditional-trigger")?.value ?? "";
                const questionText = conditional.querySelector<HTMLInputElement>(".conditional-input")?.value ?? "";

                return {
                    trigger,
                    triggerType: this.getTriggerType(conditional, type),
                    ai: false,
                    question: questionText
                };
            });
    }

    private getTriggerType(conditional: HTMLElement, type: QuestionType): string {
        if (type === "single" || type === "multiple") {
            return "Equals";
        }

        if (type === "open") {
            return "Contains";
        }

        return conditional.querySelector<HTMLSelectElement>(".conditional-trigger-type")?.value ?? "Contains";
    }

    private saveToLocalStorage(): void {
        if (this.isLoading) {
            return;
        }

        sessionStorage.setItem("surveyDraft", JSON.stringify(this.getSurveyData()));
    }

    private loadFromLocalStorage(sections: SectionData[]): void {
        const container = document.getElementById("sections");

        if (!container) {
            return;
        }

        this.isLoading = true;
        container.innerHTML = "";
        this.questionCount = 0;
        this.sectionCount = 0;

        sections.forEach(sectionData => {
            this.loadSection(sectionData);
        });

        this.isLoading = false;
        this.updateCounter();
        this.saveToLocalStorage();
    }

    private loadSection(sectionData: SectionData): void {
        this.addSection();

        const sections = Array.from(document.querySelectorAll<HTMLElement>(".section"));
        const section = sections[sections.length - 1];
        
        if (!section) {
            return;
        }

        const sectionTitle = section.querySelector<HTMLInputElement>(".section-title");

        if (sectionTitle) {
            sectionTitle.value = sectionData.title ?? "";
        }

        sectionData.questions.forEach(questionData => {
            this.loadQuestion(section, questionData);
        });
    }

    private loadQuestion(section: HTMLElement, questionData: QuestionData): void {
        const addQuestionButton = section.querySelector<HTMLElement>(".add-question-btn");

        if (!addQuestionButton) {
            return;
        }

        this.addQuestion(addQuestionButton);

        const questions = Array.from(section.querySelectorAll<HTMLElement>(".question"));
        const question = questions[questions.length - 1];
        
        if (!question) {
            return;
        }

        const titleInput = question.querySelector<HTMLInputElement>(".question-title");
        const typeSelect = question.querySelector<HTMLSelectElement>("select");

        if (titleInput) {
            titleInput.value = questionData.title ?? "";
        }

        if (typeSelect) {
            typeSelect.value = questionData.type ?? "";
            this.changeType(typeSelect);
        }

        this.loadQuestionAnswers(question, questionData);
        this.loadConditionals(question, questionData.conditionals ?? []);
    }

    private loadQuestionAnswers(question: HTMLElement, questionData: QuestionData): void {
        if (questionData.type === "single" || questionData.type === "multiple") {
            const answersList = question.querySelector<HTMLElement>(".answers-list");

            if (!answersList) {
                return;
            }

            answersList.innerHTML = "";

            questionData.answers.forEach(answer => {
                this.addAnswer(answersList);

                const inputs = Array.from(answersList.querySelectorAll<HTMLInputElement>("input"));
                const input = inputs[inputs.length - 1];

                if (input) {
                    input.value = answer;
                }
            });
        }

        if (questionData.type === "range") {
            const minInput = question.querySelector<HTMLInputElement>('input[placeholder="Min"]');
            const maxInput = question.querySelector<HTMLInputElement>('input[placeholder="Max"]');

            if (minInput) {
                minInput.value = questionData.min ?? "";
            }

            if (maxInput) {
                maxInput.value = questionData.max ?? "";
            }
        }
    }

    private loadConditionals(question: HTMLElement, conditionals: ConditionalData[]): void {
        conditionals.forEach(conditionalData => {
            const conditionalButton = question.querySelector<HTMLElement>(".add-conditional-btn");

            if (!conditionalButton) {
                return;
            }

            this.addConditional(conditionalButton);

            const conditionals = Array.from(question.querySelectorAll<HTMLElement>(".conditional-container > .conditional-block"));
            const conditional = conditionals[conditionals.length - 1];

            if (!conditional) {
                return;
            }

            const triggerSelect = conditional.querySelector<HTMLSelectElement>(".conditional-trigger");
            const triggerTypeSelect = conditional.querySelector<HTMLSelectElement>(".conditional-trigger-type");
            const conditionalInput = conditional.querySelector<HTMLInputElement>(".conditional-input");

            if (triggerSelect) {
                triggerSelect.value = conditionalData.trigger ?? "";
            }

            if (triggerTypeSelect) {
                triggerTypeSelect.value = conditionalData.triggerType ?? "Contains";
            }

            if (conditionalInput) {
                conditionalInput.value = conditionalData.question ?? "";
            }
        });
    }

    private updateConditionalTriggerType(question: HTMLElement, conditional: HTMLElement): void {
        const container = conditional.querySelector<HTMLElement>(".conditional-trigger-type-container");
        const questionType = question.querySelector<HTMLSelectElement>("select")?.value as QuestionType;

        if (!container) {
            return;
        }

        container.innerHTML = "";

        if (questionType !== "range") {
            return;
        }

        const select = document.createElement("select");
        select.className = "conditional-trigger-type form-select form-select-sm";

        [
            { value: "Equals", text: "Gelijk aan" },
            { value: "GreaterOrEqual", text: "Groter dan of gelijk aan" },
            { value: "LessOrEqual", text: "Kleiner dan of gelijk aan" }
        ].forEach(optionData => {
            this.appendOption(select, optionData.value, optionData.text);
        });

        select.addEventListener("change", () => {
            this.saveToLocalStorage();
        });

        container.appendChild(select);
    }

    private async generateSurveyWithAi(): Promise<void> {
        const promptInput = document.getElementById("aiPrompt") as HTMLTextAreaElement | null;
        const questionAmountInput = document.getElementById("questionAmount") as HTMLInputElement | null;
        const messageBox = document.getElementById("surveyAiMessage") as HTMLSpanElement | null;
        const errorBox = document.getElementById("surveyAiError") as HTMLDivElement | null;
        const form = document.getElementById("surveyForm") as HTMLFormElement | null;

        if (!promptInput || !form) {
            return;
        }

        const description = promptInput.value.trim();
        const questionAmount = Number(questionAmountInput?.value ?? 5);
        const aiUrl = form.dataset.aiUrl;

        if (!aiUrl) {
            window.alert("AI-url ontbreekt.");
            return;
        }

        this.hideError(errorBox);

        if (!description) {
            window.alert("Geef eerst een beschrijving in.");
            return;
        }

        if (description.length < 50) {
            window.alert("Beschrijving moet minstens 50 tekens bevatten.");
            return;
        }

        if (messageBox) {
            messageBox.textContent = "AI maakt je vragenlijst...";
        }

        const response = await fetch(aiUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": DomUtils.getAntiForgeryToken()
            },
            body: JSON.stringify({ description, questionAmount })
        });

        const data = await this.readGenerateSurveyResponse(response);

        if (!data) {
            this.showError(errorBox, "Server gaf geen geldige JSON terug.");
            this.clearMessage(messageBox);
            return;
        }

        if (!response.ok || !data.ok || !data.survey?.sections) {
            this.showError(errorBox, data.message ?? "AI kon geen vragenlijst genereren.");
            this.clearMessage(messageBox);
            return;
        }

        this.loadFromLocalStorage(data.survey.sections);
        sessionStorage.setItem("surveyDraft", JSON.stringify(data.survey.sections));

        if (messageBox) {
            messageBox.textContent = "Vragenlijst gegenereerd.";
        }
    }

    private async readGenerateSurveyResponse(response: Response): Promise<GenerateSurveyResponse | null> {
        try {
            return await response.json() as GenerateSurveyResponse;
        } catch {
            return null;
        }
    }

    private hideError(errorBox: HTMLElement | null): void {
        if (!errorBox) {
            return;
        }

        errorBox.textContent = "";
        errorBox.classList.add("d-none");
    }

    private showError(errorBox: HTMLElement | null, message: string): void {
        if (!errorBox) {
            window.alert(message);
            return;
        }

        errorBox.textContent = message;
        errorBox.classList.remove("d-none");
    }

    private clearMessage(messageBox: HTMLElement | null): void {
        if (messageBox) {
            messageBox.textContent = "";
        }
    }
}

document.addEventListener("DOMContentLoaded", () => {
    new SurveyBuilder().init();
});