let questionCount = 0;
const maxQuestions = 20;
let sectionCount = 0;
let isLoading = false;
let saveTimeout: number | undefined;

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

document.addEventListener("DOMContentLoaded", () => {
    updateCounter();

    (window as any).addSection = addSection;
    (window as any).addQuestion = addQuestion;
    (window as any).removeQuestion = removeQuestion;
    (window as any).removeSection = removeSection;
    (window as any).changeType = changeType;
    (window as any).addConditional = addConditional;
    (window as any).toggleAI = toggleAI;

    const data = sessionStorage.getItem("surveyDraft");

    if (!data) {
        createInitialSurvey();
        return;
    }

    const parsed = JSON.parse(data) as SectionData[];
    const hasQuestions = parsed.some(s => s.questions?.length > 0);

    if (hasQuestions) {
        loadFromLocalStorage();
    } else {
        sessionStorage.removeItem("surveyDraft");
        createInitialSurvey();
    }
});

document.addEventListener("input", () => {
    if (saveTimeout !== undefined) {
        clearTimeout(saveTimeout);
    }

    saveTimeout = window.setTimeout(() => {
        saveToLocalStorage();
    }, 300);
});

function updateCounter(): void {
    const counter = document.getElementById("questionCounter");
    if (counter) {
        counter.innerText = `${questionCount} / ${maxQuestions} vragen`;
    }
}

function addSection(): void {
    const container = document.getElementById("sections");
    if (!container) return;

    sectionCount++;

    const section = document.createElement("div");
    section.className = "section bg-white p-5 rounded-xl shadow-sm border border-slate-200";
    section.innerHTML = `
        <div class="flex justify-between items-center gap-3 mb-3">
            <input type="text"
                   class="section-title w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 placeholder:text-slate-400 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100"
                   placeholder="Sectie ${sectionCount} titel..." />

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
    saveToLocalStorage();
}

function removeSection(btn: HTMLElement): void {
    const sections = document.querySelectorAll(".section");
    if (sections.length <= 1) {
        alert("Er moet minstens 1 sectie zijn");
        return;
    }

    const section = btn.closest(".section");
    if (!section) return;

    questionCount -= section.querySelectorAll(".question").length;
    section.remove();

    updateCounter();
    saveToLocalStorage();
}

function addQuestion(btn: HTMLElement): void {
    if (questionCount >= maxQuestions) {
        alert("Max 20 vragen toegestaan");
        return;
    }

    const section = btn.closest(".section");
    const container = section?.querySelector(".questions");
    if (!container) return;

    questionCount++;
    updateCounter();

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
    saveToLocalStorage();
}

function removeQuestion(btn: HTMLElement): void {
    if (questionCount <= 1) {
        alert("Er moet minstens 1 vraag zijn");
        return;
    }

    const question = btn.closest(".question");
    if (!question) return;

    question.remove();
    questionCount--;

    updateCounter();
    saveToLocalStorage();
}

function changeType(select: HTMLSelectElement): void {
    const container = select.parentElement?.querySelector(".answers") as HTMLElement | null;
    if (!container) return;

    container.innerHTML = "";

    if (select.value === "single" || select.value === "multiple") {
        const wrapper = document.createElement("div");
        wrapper.className = "answers-list space-y-2";

        addAnswer(wrapper);
        addAnswer(wrapper);

        const button = document.createElement("button");
        button.type = "button";
        button.innerText = "+ Antwoord";
        button.className = "mt-2 text-sm text-indigo-600 hover:underline";
        button.onclick = () => addAnswer(wrapper);

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

    saveToLocalStorage();
}

function createInitialSurvey(): void {
    isLoading = true;

    addSection();

    const firstSection = document.querySelector(".section");
    const addQuestionBtn = firstSection?.querySelector(".add-question-btn") as HTMLElement | null;

    if (addQuestionBtn) {
        addQuestion(addQuestionBtn);
    }

    isLoading = false;
    saveToLocalStorage();
}

function addAnswer(container: Element): void {
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
        saveToLocalStorage();
    };

    wrapper.appendChild(input);
    wrapper.appendChild(removeBtn);
    container.appendChild(wrapper);

    saveToLocalStorage();
}

function addConditional(btn: HTMLElement): void {
    const container = btn.parentElement?.querySelector(".conditional-container");
    if (!container) return;

    const conditional = document.createElement("div");
    conditional.className = "mt-3 rounded-lg border bg-indigo-50 p-3";
    conditional.innerHTML = `
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
    saveToLocalStorage();
}

function toggleAI(checkbox: HTMLInputElement): void {
    const wrapper = checkbox.closest("div");
    const input = wrapper?.querySelector(".conditional-input") as HTMLInputElement | null;
    if (!input) return;

    input.disabled = checkbox.checked;
    if (checkbox.checked) {
        input.value = "";
    }

    saveToLocalStorage();
}

function getSurveyData(): SectionData[] {
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
            question.querySelectorAll(".conditional-container > div").forEach(c => {
                const trigger = (c.querySelector("input") as HTMLInputElement | null)?.value ?? "";
                const ai = (c.querySelector("input[type='checkbox']") as HTMLInputElement | null)?.checked ?? false;
                const conditionalQuestion = (c.querySelector(".conditional-input") as HTMLInputElement | null)?.value ?? "";

                conditionals.push({
                    trigger,
                    ai,
                    question: conditionalQuestion
                });
            });

            questions.push({
                title: qTitle,
                type,
                answers,
                min,
                max,
                conditionals
            });
        });

        sections.push({ title, questions });
    });

    return sections;
}

function saveToLocalStorage(): void {
    if (isLoading) return;
    sessionStorage.setItem("surveyDraft", JSON.stringify(getSurveyData()));
}

function loadFromLocalStorage(): void {
    const data = sessionStorage.getItem("surveyDraft");
    if (!data) return;

    const container = document.getElementById("sections");
    if (!container) return;

    isLoading = true;
    container.innerHTML = "";
    questionCount = 0;
    sectionCount = 0;

    const parsed = JSON.parse(data) as SectionData[];

    parsed.forEach(sectionData => {
        addSection();

        const sections = document.querySelectorAll(".section");
        const section = sections[sections.length - 1] as HTMLElement;

        const sectionTitle = section.querySelector(".section-title") as HTMLInputElement | null;
        if (sectionTitle) {
            sectionTitle.value = sectionData.title ?? "";
        }

        sectionData.questions.forEach(q => {
            const addQuestionBtn = section.querySelector(".add-question-btn") as HTMLElement | null;
            if (!addQuestionBtn) return;

            addQuestion(addQuestionBtn);

            const questions = section.querySelectorAll(".question");
            const question = questions[questions.length - 1] as HTMLElement;

            const titleInput = question.querySelector(".question-title") as HTMLInputElement | null;
            const typeSelect = question.querySelector("select") as HTMLSelectElement | null;

            if (titleInput) {
                titleInput.value = q.title ?? "";
            }

            if (typeSelect) {
                typeSelect.value = q.type ?? "";
                changeType(typeSelect);
            }

            if (q.type === "single" || q.type === "multiple") {
                const answersList = question.querySelector(".answers-list") as HTMLElement | null;
                if (answersList) {
                    answersList.innerHTML = "";
                    q.answers.forEach(answer => {
                        addAnswer(answersList);
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

                addConditional(conditionalBtn);

                const conditionals = question.querySelectorAll(".conditional-container > div");
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

    isLoading = false;
    updateCounter();
}