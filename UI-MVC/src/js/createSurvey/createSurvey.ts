// createsurvey/createSurvey.ts

type ConditionalData = {
    trigger: string;
    ai: boolean;
    question: string;
};

type QuestionData = {
    title: string;
    type: string;
    answers: string[];
    min: string;
    max: string;
    conditionals: ConditionalData[];
};

type SectionData = {
    title: string;
    questions: QuestionData[];
};

export class SurveyBuilder {
    private questionCount: number = 0;
    private readonly maxQuestions: number = 20;
    private sectionCount: number = 0;
    private isLoading: boolean = false;
    private saveTimeout?: number;

    init(): void {
        this.updateCounter();
        this.bindWindowMethods();

        document.addEventListener("input", this.handleInputDebounce.bind(this));

        const data = sessionStorage.getItem("surveyDraft");
        if (!data) {
            this.createInitialSurvey();
            return;
        }

        const parsed = JSON.parse(data) as SectionData[];
        const hasQuestions = parsed.some(s => s.questions?.length > 0);

        if (hasQuestions) {
            this.loadFromLocalStorage(parsed);
        } else {
            sessionStorage.removeItem("surveyDraft");
            this.createInitialSurvey();
        }
    }

    private bindWindowMethods(): void {
        // Zorg dat de inline HTML onclick/onchange attributes blijven werken
        (window as any).addSection = this.addSection.bind(this);
        (window as any).addQuestion = this.addQuestion.bind(this);
        (window as any).removeQuestion = this.removeQuestion.bind(this);
        (window as any).removeSection = this.removeSection.bind(this);
        (window as any).changeType = this.changeType.bind(this);
        (window as any).addConditional = this.addConditional.bind(this);
        (window as any).removeConditional = this.removeConditional.bind(this); // NIEUW
        (window as any).toggleAI = this.toggleAI.bind(this);
    }

    private handleInputDebounce(): void {
        if (this.saveTimeout !== undefined) {
            clearTimeout(this.saveTimeout);
        }

        this.saveTimeout = window.setTimeout(() => {
            this.saveToLocalStorage();
        }, 300);
    }

    private updateCounter(): void {
        const counter = document.getElementById("questionCounter");
        if (counter) {
            counter.innerText = `${this.questionCount} / ${this.maxQuestions} vragen`;
        }
    }

    addSection(): void {
        const container = document.getElementById("sections");
        if (!container) return;

        this.sectionCount++;

        const section = document.createElement("div");
        section.className = "section bg-white p-5 rounded-xl shadow-sm border border-slate-200";
        section.innerHTML = `
            <div class="flex justify-between items-center gap-3 mb-3">
                <input type="text"
                       class="section-title w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 placeholder:text-slate-400 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100"
                       placeholder="Sectie ${this.sectionCount} titel..." />

                <button type="button"
                        onclick="removeSection(this)"
                        class="inline-flex h-11 w-11 shrink-0 items-center justify-center rounded-lg border border-red-200 bg-white text-red-500 hover:bg-red-50 transition">
                    🗑
                </button>
            </div>

            <div class="questions space-y-3"></div>

            <button type="button"
                    onclick="addQuestion(this)"
                    class="add-question-btn mt-4 rounded-lg bg-indigo-600 px-4 py-2 text-sm text-white hover:bg-indigo-700 transition">
                + Vraag toevoegen
            </button>
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
        question.className = "question rounded-lg border border-slate-200 bg-white p-4";
        question.innerHTML = `
            <div class="flex justify-between items-center mb-2">
                <span class="text-sm text-slate-500">Vraag</span>
                <button type="button"
                        onclick="removeQuestion(this)"
                        class="inline-flex h-11 w-11 items-center justify-center rounded-lg border border-red-200 bg-white text-red-500 hover:bg-red-50 transition">
                    🗑
                </button>
            </div>

            <input type="text"
                   class="question-title mb-3 w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 placeholder:text-slate-400 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100"
                   placeholder="Vraag titel..." />

            <select onchange="changeType(this)"
                    class="mb-3 w-full rounded-lg border border-slate-300 bg-gray-100 px-4 py-3 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100">
                <option value="" disabled selected>Kies type vraag...</option>
                <option value="single">Enkelkeuze</option>
                <option value="multiple">Meerkeuze</option>
                <option value="range">Range</option>
                <option value="open">Open vraag</option>
            </select>

            <div class="answers space-y-2"></div>

            <button type="button"
                    onclick="addConditional(this)"
                    class="add-conditional-btn mt-3 text-sm text-indigo-600 hover:underline">
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
            wrapper.className = "answers-list space-y-2";

            this.addAnswer(wrapper);
            this.addAnswer(wrapper);

            const button = document.createElement("button");
            button.type = "button";
            button.innerText = "+ Antwoord";
            button.className = "mt-2 text-sm text-indigo-600 hover:underline";
            button.onclick = () => this.addAnswer(wrapper);

            container.appendChild(wrapper);
            container.appendChild(button);
        }

        if (select.value === "range") {
            container.innerHTML = `
                <div class="flex gap-3">
                    <input type="number" placeholder="Min"
                           class="w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100" />
                    <input type="number" placeholder="Max"
                           class="w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100" />
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

        if (addQuestionBtn) {
            this.addQuestion(addQuestionBtn);
        }

        this.isLoading = false;
        this.saveToLocalStorage();
    }

    private addAnswer(container: Element): void {
        const wrapper = document.createElement("div");
        wrapper.className = "flex items-center gap-2";

        const input = document.createElement("input");
        input.placeholder = "Antwoord...";
        input.className = "w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100";

        const removeBtn = document.createElement("button");
        removeBtn.type = "button";
        removeBtn.innerText = "🗑";
        removeBtn.className = "inline-flex h-11 w-11 items-center justify-center rounded-lg border border-red-200 bg-white text-red-500 hover:bg-red-50 transition";

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
        if (!container) return;

        const conditional = document.createElement("div");
        // We voegen een extra class "conditional-block" toe om hem makkelijk te vinden bij het verwijderen
        conditional.className = "conditional-block mt-3 rounded-lg border bg-indigo-50 p-3";

        // Let op de nieuwe header-div bovenaan de template
        conditional.innerHTML = `
            <div class="flex justify-between items-center mb-2">
                <span class="text-xs font-semibold uppercase tracking-wide text-indigo-500">Conditionele Vraag</span>
                <button type="button"
                        onclick="removeConditional(this)"
                        class="inline-flex h-8 w-8 items-center justify-center rounded-md border border-red-200 bg-white text-red-500 hover:bg-red-50 transition"
                        title="Verwijder conditionele vraag">
                    🗑
                </button>
            </div>

            <input placeholder="Trigger antwoord"
                   class="mb-2 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm" />

            <label class="mb-2 flex gap-2 text-sm">
                <input type="checkbox" onchange="toggleAI(this)" />
                AI laten genereren
            </label>

            <input class="conditional-input w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
                   placeholder="Conditionele vraag..." />
        `;

        container.appendChild(conditional);
        this.saveToLocalStorage();
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
        if (checkbox.checked) {
            input.value = "";
        }

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
                    const trigger = (c.querySelector("input") as HTMLInputElement | null)?.value ?? "";
                    const ai = (c.querySelector("input[type='checkbox']") as HTMLInputElement | null)?.checked ?? false;
                    const conditionalQuestion = (c.querySelector(".conditional-input") as HTMLInputElement | null)?.value ?? "";

                    conditionals.push({ trigger, ai, question: conditionalQuestion });
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
            if (sectionTitle) {
                sectionTitle.value = sectionData.title ?? "";
            }

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
                            if (input) {
                                input.value = answer;
                            }
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

                    const triggerInput = conditional.querySelector("input") as HTMLInputElement | null;
                    const aiCheckbox = conditional.querySelector("input[type='checkbox']") as HTMLInputElement | null;
                    const conditionalInput = conditional.querySelector(".conditional-input") as HTMLInputElement | null;

                    if (triggerInput) triggerInput.value = cond.trigger ?? "";
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
}

// Initialisatie wanneer het document geladen is
document.addEventListener("DOMContentLoaded", () => {
    new SurveyBuilder().init();
});