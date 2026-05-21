import { DomUtils } from "../helpers/utils";
import type { ConditionalData, QuestionData, SectionData } from "../helpers/types";

export class SurveyBuilder {
    private questionCount: number = 0;
    private readonly maxQuestions: number = 20;
    private sectionCount: number = 0;
    private isLoading: boolean = false;
    private saveTimeout?: number;

    init(): void {
        this.updateCounter();
        this.bindWindowMethods();

        document.getElementById("generateSurveyWithAiBtn")?.addEventListener("click", async () => {
            await this.generateSurveyWithAi();
        });

        const form = document.getElementById("surveyForm") as HTMLFormElement | null;
        form?.addEventListener("submit", async (event) => {
            event.preventDefault();
            await this.handleSave(form);
        });

        document.addEventListener("input", this.handleInputDebounce.bind(this));

        const initialDataElement = document.getElementById("initialSurveyData");
        if (initialDataElement?.textContent?.trim()) {
            const serverData = JSON.parse(initialDataElement.textContent) as SectionData[];
            if (serverData.some(s => s.questions?.length > 0)) {
                sessionStorage.setItem("surveyDraft", JSON.stringify(serverData));
                this.loadFromLocalStorage(serverData);
                return;
            }
        }

        const data = sessionStorage.getItem("surveyDraft");
        if (!data) {
            this.createInitialSurvey();
            return;
        }

        const parsed = JSON.parse(data) as SectionData[];
        if (parsed.some(s => s.questions?.length > 0)) {
            this.loadFromLocalStorage(parsed);
        } else {
            sessionStorage.removeItem("surveyDraft");
            this.createInitialSurvey();
        }
    }

    private async handleSave(form: HTMLFormElement): Promise<void> {
        const url = form.dataset.saveUrl;
        const errorBox = document.getElementById("surveyError") as HTMLDivElement | null;
        if (!url) return;

        if (errorBox) { errorBox.textContent = ""; errorBox.classList.add("d-none"); }

        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": DomUtils.getAntiForgeryToken()
            },
            body: JSON.stringify({ sections: this.getSurveyData() })
        });

        const result = await response.json();

        if (!response.ok || !result.ok) {
            if (errorBox) {
                errorBox.textContent = result.message ?? "Er is iets fout gegaan.";
                errorBox.classList.remove("d-none");
            } else {
                alert(result.message ?? "Er is iets fout gegaan.");
            }
            return;
        }

        window.location.href = result.redirectUrl;
    }

    private bindWindowMethods(): void {
        // Zorg dat de inline HTML onclick/onchange attributes blijven werken
        (window as any).addSection = this.addSection.bind(this);
        (window as any).addQuestion = this.addQuestion.bind(this);
        (window as any).removeQuestion = this.removeQuestion.bind(this);
        (window as any).removeSection = this.removeSection.bind(this);
        (window as any).changeType = this.changeType.bind(this);
        (window as any).addConditional = this.addConditional.bind(this);
        (window as any).removeConditional = this.removeConditional.bind(this);
        (window as any).toggleAI = this.toggleAI.bind(this);
    }

    private handleInputDebounce(): void {
        if (this.saveTimeout !== undefined) clearTimeout(this.saveTimeout);
        this.saveTimeout = window.setTimeout(() => this.saveToLocalStorage(), 300);
    }

    private updateCounter(): void {
        const counter = document.getElementById("questionCounter");
        if (counter) counter.innerText = `${this.questionCount} / ${this.maxQuestions} vragen`;
    }

    addSection(): void {
        const container = document.getElementById("sections");
        if (!container) return;

        this.sectionCount++;

        const section = document.createElement("div");
        section.className = "section card mb-3";
        section.innerHTML = `
            <div class="card-body">
                <div class="d-flex justify-content-between align-items-center gap-3 mb-3">
                    <input type="text"
                           class="section-title form-control form-control-sm"
                           placeholder="Sectie ${this.sectionCount} titel..." />
                    <button type="button"
                            onclick="removeSection(this)"
                            class="btn btn-outline-danger btn-sm flex-shrink-0">
                        🗑
                    </button>
                </div>
                <div class="questions d-flex flex-column gap-3"></div>
                <button type="button"
                        onclick="addQuestion(this)"
                        class="add-question-btn btn btn-outline-primary btn-sm mt-3">
                    + Vraag toevoegen
                </button>
            </div>
        `;

        container.appendChild(section);
        this.saveToLocalStorage();
    }

    removeSection(btn: HTMLElement): void {
        const sections = document.querySelectorAll(".section");
        if (sections.length <= 1) {
            alert("Er moet minstens 1 sectie zijn");
            return;
        }

        const section = btn.closest(".section");
        if (!section) return;

        this.questionCount -= section.querySelectorAll(".question").length;
        section.remove();

        this.updateCounter();
        this.saveToLocalStorage();
    }

    addQuestion(btn: HTMLElement): void {
        if (this.questionCount >= this.maxQuestions) {
            alert(`Max ${this.maxQuestions} vragen toegestaan`);
            return;
        }

        const section = btn.closest(".section");
        const container = section?.querySelector(".questions");
        if (!container) return;

        this.questionCount++;
        this.updateCounter();

        const question = document.createElement("div");
        question.className = "question border rounded p-3";
        question.innerHTML = `
            <div class="d-flex justify-content-between align-items-center mb-2">
                <span class="text-muted small">Vraag</span>
                <button type="button"
                        onclick="removeQuestion(this)"
                        class="btn btn-outline-danger btn-sm">
                    🗑
                </button>
            </div>
            <input type="text"
                   class="question-title form-control form-control-sm mb-3"
                   placeholder="Vraag titel..." />
            <select onchange="changeType(this)"
                    class="form-select form-select-sm mb-3">
                <option value="" disabled selected>Kies type vraag...</option>
                <option value="single">Enkelkeuze</option>
                <option value="multiple">Meerkeuze</option>
                <option value="range">Range</option>
                <option value="open">Open vraag</option>
            </select>
            <div class="answers d-flex flex-column gap-2"></div>
            <button type="button"
                    onclick="addConditional(this)"
                    class="add-conditional-btn btn btn-link btn-sm mt-2 p-0 text-decoration-none">
                + Conditionele vraag
            </button>
            <div class="conditional-container"></div>
        `;

        container.appendChild(question);
        this.saveToLocalStorage();
    }

    removeQuestion(btn: HTMLElement): void {
        if (this.questionCount <= 1) {
            alert("Er moet minstens 1 vraag zijn");
            return;
        }

        const question = btn.closest(".question");
        if (!question) return;

        question.remove();
        this.questionCount--;

        this.updateCounter();
        this.saveToLocalStorage();
    }

    changeType(select: HTMLSelectElement): void {
        const container = select.parentElement?.querySelector(".answers") as HTMLElement | null;
        if (!container) return;

        container.innerHTML = "";

        if (select.value === "single" || select.value === "multiple") {
            const wrapper = document.createElement("div");
            wrapper.className = "answers-list d-flex flex-column gap-2";

            this.addAnswer(wrapper);
            this.addAnswer(wrapper);

            const button = document.createElement("button");
            button.type = "button";
            button.innerText = "+ Antwoord";
            button.className = "btn btn-link btn-sm mt-2 p-0 text-decoration-none";
            button.onclick = () => this.addAnswer(wrapper);

            container.appendChild(wrapper);
            container.appendChild(button);
        }

        if (select.value === "range") {
            container.innerHTML = `
                <div class="d-flex gap-2">
                    <input type="number" placeholder="Min" class="form-control form-control-sm" />
                    <input type="number" placeholder="Max" class="form-control form-control-sm" />
                </div>
            `;
        }

        this.saveToLocalStorage();
    }

    private createInitialSurvey(): void {
        this.isLoading = true;
        this.addSection();

        const firstSection = document.querySelector(".section");
        const addQuestionBtn = firstSection?.querySelector(".add-question-btn") as HTMLElement | null;
        if (addQuestionBtn) this.addQuestion(addQuestionBtn);

        this.isLoading = false;
        this.saveToLocalStorage();
    }

    private addAnswer(container: Element): void {
        const wrapper = document.createElement("div");
        wrapper.className = "d-flex align-items-center gap-2";

        const input = document.createElement("input");
        input.placeholder = "Antwoord...";
        input.className = "form-control form-control-sm";

        input.addEventListener("input", () => {
            const question = input.closest(".question") as HTMLElement | null;
            if (!question) return;

            question.querySelectorAll<HTMLSelectElement>(".conditional-trigger").forEach(triggerSelect => {
                const currentValue = triggerSelect.value;
                this.populateConditionalTriggerOptions(question, triggerSelect);
                triggerSelect.value = currentValue;
            });
        });

        const removeBtn = document.createElement("button");
        removeBtn.type = "button";
        removeBtn.innerText = "🗑";
        removeBtn.className = "btn btn-outline-danger btn-sm flex-shrink-0";
        removeBtn.onclick = () => {
            if (container.children.length <= 2) {
                alert("Minstens 2 antwoorden vereist");
                return;
            }
            wrapper.remove();
            this.saveToLocalStorage();
        };

        wrapper.appendChild(input);
        wrapper.appendChild(removeBtn);
        container.appendChild(wrapper);

        this.saveToLocalStorage();
    }

    addConditional(btn: HTMLElement): void {
        const container = btn.parentElement?.querySelector(".conditional-container");
        const question = btn.closest(".question") as HTMLElement | null;

        if (!container || !question) return;

        const conditional = document.createElement("div");
        conditional.className = "conditional-block border rounded p-3 mt-3 bg-light";
        conditional.innerHTML = `
        <div class="d-flex justify-content-between align-items-center mb-2">
            <span class="text-primary small fw-semibold text-uppercase">Conditionele Vraag</span>
            <button type="button"
                    onclick="removeConditional(this)"
                    class="btn btn-outline-danger btn-sm"
                    title="Verwijder conditionele vraag">
                🗑
            </button>
        </div>

        <div class="conditional-trigger-type-container mb-2"></div>

        <select class="conditional-trigger form-select form-select-sm mb-2"></select>

       

        <input class="conditional-input form-control form-control-sm"
               placeholder="Conditionele vraag..." />
    `;

        container.appendChild(conditional);
        this.updateConditionalTriggerType(question, conditional);

        const triggerSelect = conditional.querySelector(".conditional-trigger") as HTMLSelectElement | null;
        if (triggerSelect) {
            this.populateConditionalTriggerOptions(question, triggerSelect);
        }

        this.saveToLocalStorage();
    }

    private populateConditionalTriggerOptions(
        question: HTMLElement,
        triggerSelect: HTMLSelectElement
    ): void {
        triggerSelect.innerHTML = "";

        const typeSelect = question.querySelector("select") as HTMLSelectElement | null;
        const questionType = typeSelect?.value ?? "";

        if (questionType === "range") {
            const min = Number((question.querySelector('input[placeholder="Min"]') as HTMLInputElement | null)?.value ?? 1);
            const max = Number((question.querySelector('input[placeholder="Max"]') as HTMLInputElement | null)?.value ?? 5);

            for (let value = min; value <= max; value++) {
                const option = document.createElement("option");
                option.value = value.toString();
                option.textContent = value.toString();
                triggerSelect.appendChild(option);
            }

            return;
        }

        const answerInputs = question.querySelectorAll<HTMLInputElement>(".answers-list input");
        answerInputs.forEach(input => {
            if (!input.value.trim()) return;

            const option = document.createElement("option");
            option.value = input.value.trim();
            option.textContent = input.value.trim();

            triggerSelect.appendChild(option);
        });

        if (triggerSelect.options.length === 0) {
            const option = document.createElement("option");
            option.value = "";
            option.textContent = "Vul eerst antwoordopties in";
            triggerSelect.appendChild(option);
        }
    }

    removeConditional(btn: HTMLElement): void {
        const conditionalBlock = btn.closest(".conditional-block");
        if (!conditionalBlock) return;
        conditionalBlock.remove();
        this.saveToLocalStorage();
    }

    toggleAI(checkbox: HTMLInputElement): void {
        const wrapper = checkbox.closest("div");
        const input = wrapper?.querySelector(".conditional-input") as HTMLInputElement | null;
        if (!input) return;

        input.disabled = checkbox.checked;
        if (checkbox.checked) input.value = "";

        this.saveToLocalStorage();
    }

    private getSurveyData(): SectionData[] {
        const sections: SectionData[] = [];

        document.querySelectorAll(".section").forEach(section => {
            const title = (section.querySelector(".section-title") as HTMLInputElement | null)?.value ?? "";
            const questions: QuestionData[] = [];

            section.querySelectorAll(".question").forEach(question => {
                const qTitle = (question.querySelector(".question-title") as HTMLInputElement | null)?.value ?? "";
                const type = (question.querySelector("select") as HTMLSelectElement | null)?.value ?? "";

                const answers: string[] = [];
                question.querySelectorAll(".answers input").forEach(answer => {
                    answers.push((answer as HTMLInputElement).value);
                });

                const min = (question.querySelector('input[placeholder="Min"]') as HTMLInputElement | null)?.value ?? "";
                const max = (question.querySelector('input[placeholder="Max"]') as HTMLInputElement | null)?.value ?? "";

                const conditionals: ConditionalData[] = [];
                question.querySelectorAll(".conditional-container > div.conditional-block").forEach(c => {
                    const trigger = (c.querySelector(".conditional-trigger") as HTMLSelectElement | null)?.value ?? "";
                    let triggerType = (c.querySelector(".conditional-trigger-type") as HTMLSelectElement | null)?.value ?? "Contains";

                    if (type === "single" || type === "multiple") {
                        triggerType = "Equals";
                    }

                    if (type === "open") {
                        triggerType = "Contains";
                    }                    const ai = false;
                    const conditionalQuestion = (c.querySelector(".conditional-input") as HTMLInputElement | null)?.value ?? "";

                    conditionals.push({ trigger, triggerType, ai, question: conditionalQuestion });
                });

                questions.push({ title: qTitle, type, answers, min, max, conditionals });
            });

            sections.push({ title, questions });
        });

        return sections;
    }

    private saveToLocalStorage(): void {
        if (this.isLoading) return;
        sessionStorage.setItem("surveyDraft", JSON.stringify(this.getSurveyData()));
    }

    private loadFromLocalStorage(parsed: SectionData[]): void {
        const container = document.getElementById("sections");
        if (!container) return;

        this.isLoading = true;
        container.innerHTML = "";
        this.questionCount = 0;
        this.sectionCount = 0;

        parsed.forEach(sectionData => {
            this.addSection();

            const sections = document.querySelectorAll(".section");
            const section = sections[sections.length - 1] as HTMLElement;

            const sectionTitle = section.querySelector(".section-title") as HTMLInputElement | null;
            if (sectionTitle) sectionTitle.value = sectionData.title ?? "";

            sectionData.questions.forEach(q => {
                const addQuestionBtn = section.querySelector(".add-question-btn") as HTMLElement | null;
                if (!addQuestionBtn) return;

                this.addQuestion(addQuestionBtn);

                const questions = section.querySelectorAll(".question");
                const question = questions[questions.length - 1] as HTMLElement;

                const titleInput = question.querySelector(".question-title") as HTMLInputElement | null;
                const typeSelect = question.querySelector("select") as HTMLSelectElement | null;

                if (titleInput) titleInput.value = q.title ?? "";

                if (typeSelect) {
                    typeSelect.value = q.type ?? "";
                    this.changeType(typeSelect);
                }

                if (q.type === "single" || q.type === "multiple") {
                    const answersList = question.querySelector(".answers-list") as HTMLElement | null;
                    if (answersList) {
                        answersList.innerHTML = "";
                        q.answers.forEach(answer => {
                            this.addAnswer(answersList);
                            const inputs = answersList.querySelectorAll("input");
                            const input = inputs[inputs.length - 1] as HTMLInputElement | null;
                            if (input) input.value = answer;
                        });
                    }
                }

                if (q.type === "range") {
                    const minInput = question.querySelector('input[placeholder="Min"]') as HTMLInputElement | null;
                    const maxInput = question.querySelector('input[placeholder="Max"]') as HTMLInputElement | null;
                    if (minInput) minInput.value = q.min ?? "";
                    if (maxInput) maxInput.value = q.max ?? "";
                }

                q.conditionals.forEach(cond => {
                    const conditionalBtn = question.querySelector(".add-conditional-btn") as HTMLElement | null;
                    if (!conditionalBtn) return;

                    this.addConditional(conditionalBtn);

                    const conditionals = question.querySelectorAll(".conditional-container > div.conditional-block");
                    const conditional = conditionals[conditionals.length - 1] as HTMLElement | null;
                    if (!conditional) return;

                    const triggerSelect = conditional.querySelector(".conditional-trigger") as HTMLSelectElement | null;
                    const triggerTypeSelect = conditional.querySelector(".conditional-trigger-type") as HTMLSelectElement | null;
                    const aiCheckbox = conditional.querySelector("input[type='checkbox']") as HTMLInputElement | null;
                    const conditionalInput = conditional.querySelector(".conditional-input") as HTMLInputElement | null;

                    if (triggerSelect) triggerSelect.value = cond.trigger ?? "";
                    if (triggerTypeSelect) triggerTypeSelect.value = cond.triggerType ?? "Contains";
                    if (aiCheckbox) aiCheckbox.checked = cond.ai;
                    if (conditionalInput) {
                        conditionalInput.value = cond.question ?? "";
                        conditionalInput.disabled = cond.ai;
                    }
                });
            });
        });

        this.isLoading = false;
        this.updateCounter();
    }

    private updateConditionalTriggerType(question: HTMLElement, conditional: HTMLElement): void {
        const container = conditional.querySelector(".conditional-trigger-type-container") as HTMLElement | null;
        const typeSelect = question.querySelector("select") as HTMLSelectElement | null;
        const questionType = typeSelect?.value ?? "";

        if (!container) return;

        container.innerHTML = "";

        if (questionType !== "range") {
            return;
        }

        container.innerHTML = `
        <select class="conditional-trigger-type form-select form-select-sm">
            <option value="Equals">Gelijk aan</option>
            <option value="GreaterOrEqual">Groter dan of gelijk aan</option>
            <option value="LessOrEqual">Kleiner dan of gelijk aan</option>
        </select>
    `;
    }

    private async generateSurveyWithAi(): Promise<void> {
        const promptInput = document.getElementById("aiPrompt") as HTMLTextAreaElement | null;
        const questionAmountInput = document.getElementById("questionAmount") as HTMLInputElement | null;
        const messageBox = document.getElementById("surveyAiMessage") as HTMLSpanElement | null;
        const errorBox = document.getElementById("surveyAiError") as HTMLDivElement | null;
        const form = document.getElementById("surveyForm") as HTMLFormElement | null;

        if (!promptInput) return;

        const description = promptInput.value.trim();
        const questionAmount = Number(questionAmountInput?.value ?? 5);
        const aiUrl = form?.dataset.aiUrl;

        if (!aiUrl) { alert("AI-url ontbreekt."); return; }

        if (errorBox) { errorBox.textContent = ""; errorBox.classList.add("d-none"); }

        if (!description) { alert("Geef eerst een beschrijving in."); return; }
        if (description.length < 50) { alert("Beschrijving moet minstens 50 tekens bevatten."); return; }

        if (messageBox) messageBox.textContent = "AI maakt je vragenlijst...";

        const response = await fetch(aiUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": DomUtils.getAntiForgeryToken()
            },
            body: JSON.stringify({ description, questionAmount })
        });

        const responseText = await response.text();
        let data: any;

        try {
            data = JSON.parse(responseText);
        } catch {
            console.error("Server gaf geen JSON terug:", responseText);
            if (errorBox) { errorBox.textContent = responseText; errorBox.classList.remove("d-none"); }
            if (messageBox) messageBox.textContent = "";
            return;
        }

        if (!response.ok || !data.ok) {
            if (errorBox) { errorBox.textContent = data.message ?? "AI kon geen vragenlijst genereren."; errorBox.classList.remove("d-none"); }
            if (messageBox) messageBox.textContent = "";
            return;
        }

        this.loadFromLocalStorage(data.survey.sections);
        sessionStorage.setItem("surveyDraft", JSON.stringify(data.survey.sections));
        if (messageBox) messageBox.textContent = "Vragenlijst gegenereerd.";
    }
}

document.addEventListener("DOMContentLoaded", () => new SurveyBuilder().init());
