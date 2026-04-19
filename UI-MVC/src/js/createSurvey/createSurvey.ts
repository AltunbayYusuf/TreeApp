let questionCount = 0;
const maxQuestions = 20;
let sectionCount = 0;
let isLoading = false;

document.addEventListener("DOMContentLoaded", () => {
    updateCounter();

    (window as any).addSection = addSection;
    (window as any).addQuestion = addQuestion;
    (window as any).removeQuestion = removeQuestion;
    (window as any).removeSection = removeSection;
    (window as any).changeType = changeType;
    (window as any).addConditional = addConditional;
    (window as any).toggleAI = toggleAI;

    loadFromLocalStorage();
});
let saveTimeout: any;

document.addEventListener("input", () => {
    clearTimeout(saveTimeout);
    saveTimeout = setTimeout(() => {
        saveToLocalStorage();
    }, 300);
});

//  COUNTER 
function updateCounter() {
    const counter = document.getElementById("questionCounter");
    if (counter) {
        counter.innerText = `${questionCount} / ${maxQuestions} vragen`;
    }
}

//  SECTION 

function addSection() {
    const container = document.getElementById("sections");
    if (!container) return;

    sectionCount++;

    const section = document.createElement("div");
    section.className = "section bg-white p-5 rounded-xl shadow-sm border border-slate-200";

    section.innerHTML = `
        <div class="flex justify-between items-center mb-3">
            <input type="text"
                   class="section-title text-lg font-semibold text-slate-800 w-full"
                   placeholder="Sectie ${sectionCount} titel..." />

           <button onclick="removeSection(this)" class="text-red-500 ml-3 font-bold">
    x
</button>
        </div>

        <div class="questions space-y-3"></div>

        <button onclick="addQuestion(this)"
                class="mt-4 bg-indigo-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-indigo-700">
            + Vraag toevoegen
        </button>
    `;

    container.appendChild(section);
    saveToLocalStorage()
}

function removeSection(btn: HTMLElement) {
    const section = btn.closest(".section");
    if (!section) return;

    const questions = section.querySelectorAll(".question").length;
    questionCount -= questions;

    section.remove();
    updateCounter();
    saveToLocalStorage()
}

//  QUESTION 

function addQuestion(btn: HTMLElement) {

    if (questionCount >= maxQuestions) {
        alert("Max 20 vragen toegestaan");
        return;
    }

    // 🔥 BELANGRIJK: juiste sectie zoeken
    const section = btn.closest(".section");
    const container = section?.querySelector(".questions");

    if (!container) return;

    questionCount++;
    updateCounter();

    const questionDiv = document.createElement("div");
    questionDiv.className = "question border border-slate-200 p-4 rounded-lg bg-white";

    questionDiv.innerHTML = `
        <div class="flex justify-between items-center mb-2">
            <span class="text-sm text-slate-500">Vraag</span>
        <button type="button" onclick="removeQuestion(this)" class="text-red-500 font-bold">
    x
</button>
        </div>

        <input type="text"
               class="question-title w-full mb-3 rounded-md border border-slate-300 px-3 py-2 text-sm"
               placeholder="Vraag titel..." />

        <select onchange="changeType(this)"
                class="mb-3 rounded-md border border-slate-300 px-3 py-2 text-sm w-full">
            <option value="" disabled selected>Kies type vraag...</option>
            <option value="single">Enkelkeuze</option>
            <option value="multiple">Meerkeuze</option>
            <option value="range">Range</option>
            <option value="open">Open vraag</option>
        </select>

        <div class="answers space-y-2"></div>

        <button type="button"
                onclick="addConditional(this)"
                class="mt-3 text-sm text-indigo-600 hover:underline">
            + Conditionele vraag
        </button>

        <div class="conditional-container"></div>
    `;

    container.appendChild(questionDiv);
    saveToLocalStorage()
}

function removeQuestion(btn: HTMLElement) {
    const question = btn.closest(".question");
    if (!question) return;

    question.remove();
    questionCount--;
    updateCounter();
    saveToLocalStorage()
}

//  TYPE 

function changeType(select: HTMLSelectElement) {
    const container = select.parentElement?.querySelector(".answers");
    if (!container) return;

    container.innerHTML = "";
    const type = select.value;

    if (type === "single" || type === "multiple") {
        const wrapper = document.createElement("div");
        wrapper.className = "answers-list space-y-2";

        for (let i = 0; i < 2; i++) addAnswer(wrapper);

        const btn = document.createElement("button");
        btn.type = "button";
        btn.innerText = "+ Antwoord";
        btn.className = "text-sm text-indigo-600 hover:underline mt-2";
        btn.onclick = () => addAnswer(wrapper);

        container.appendChild(wrapper);
        container.appendChild(btn);
    }
    if (type === "range") {
        container.innerHTML = `
            <div class="flex gap-3">
                <input type="number" placeholder="Min"
                       class="w-full border px-2 py-1 rounded" />
                <input type="number" placeholder="Max"
                       class="w-full border px-2 py-1 rounded" />
            </div>
        `;
    }
    saveToLocalStorage()
}

function addAnswer(container: Element) {
    const wrapper = document.createElement("div");
    wrapper.className = "flex items-center gap-2";

    const input = document.createElement("input");
    input.placeholder = "Antwoord...";
    input.className = "w-full border px-3 py-2 rounded";

    const removeBtn = document.createElement("button");
    removeBtn.type = "button";
    removeBtn.innerText = "X";
    removeBtn.className = "text-red-500 font-bold";
    removeBtn.onclick = () => {
        if (container.children.length <= 2) {
            alert("Minstens 2 antwoord vereist");
            return;
        }
        wrapper.remove();
    };
    wrapper.appendChild(input);
    wrapper.appendChild(removeBtn);

    container.appendChild(wrapper);
    saveToLocalStorage()
}

//  CONDITIONAL 

function addConditional(btn: HTMLElement) {
    const container = btn.parentElement?.querySelector(".conditional-container");
    if (!container) return;

    const div = document.createElement("div");
    div.className = "bg-indigo-50 p-3 mt-3 rounded-lg border";

    div.innerHTML = `
        <input placeholder="Trigger antwoord" class="w-full mb-2 border px-2 py-1 rounded"/>

        <label class="flex gap-2 text-sm mb-2">
            <input type="checkbox" onchange="toggleAI(this)" />
            AI laten genereren
        </label>

        <input class="conditional-input w-full border px-2 py-1 rounded"
               placeholder="Conditionele vraag..." />
    `;

    container.appendChild(div);
}

function toggleAI(checkbox: HTMLInputElement) {
    const wrapper = checkbox.closest("div");
    if (!wrapper) return;

    const input = wrapper.querySelector(".conditional-input") as HTMLInputElement;

    if (checkbox.checked) {
        input.disabled = true;
        input.value = "";
    } else {
        input.disabled = false;
    }
}


function getSurveyData() {
    const sections: any[] = [];

    document.querySelectorAll(".section").forEach(section => {
        const title = (section.querySelector(".section-title") as HTMLInputElement)?.value;

        const questions: any[] = [];

        section.querySelectorAll(".question").forEach(q => {
            const qTitle = (q.querySelector(".question-title") as HTMLInputElement)?.value;
            const type = (q.querySelector("select") as HTMLSelectElement)?.value;

            // answers
            const answers: string[] = [];
            q.querySelectorAll(".answers input").forEach(a => {
                answers.push((a as HTMLInputElement).value);
            });

            // range
            const min = (q.querySelector('input[placeholder="Min"]') as HTMLInputElement)?.value;
            const max = (q.querySelector('input[placeholder="Max"]') as HTMLInputElement)?.value;

            // conditionals
            const conditionals: any[] = [];
            q.querySelectorAll(".conditional-container > div").forEach(c => {
                const trigger = (c.querySelector("input") as HTMLInputElement)?.value;
                const ai = (c.querySelector("input[type='checkbox']") as HTMLInputElement)?.checked;
                const question = (c.querySelector(".conditional-input") as HTMLInputElement)?.value;

                conditionals.push({trigger, ai, question});
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

        sections.push({title, questions});
    });

    return sections;
}

function saveToLocalStorage() {
    if (isLoading) return;

    const data = getSurveyData();
    sessionStorage.setItem("surveyDraft", JSON.stringify(data));
}

function loadFromLocalStorage() {
    const data = sessionStorage.getItem("surveyDraft");
    if (!data) return;

    isLoading = true;

    const parsed = JSON.parse(data);

    parsed.forEach((sectionData: any) => {
        addSection();

        const sections = document.querySelectorAll(".section");
        const lastSection = sections[sections.length - 1];

        (lastSection.querySelector(".section-title") as HTMLInputElement).value = sectionData.title;

        sectionData.questions.forEach((q: any) => {
            const btn = lastSection.querySelector(".mt-4") as HTMLElement;
            addQuestion(btn as HTMLElement);

            const questions = lastSection.querySelectorAll(".question");
            const lastQ = questions[questions.length - 1];

            (lastQ.querySelector(".question-title") as HTMLInputElement).value = q.title;
            (lastQ.querySelector("select") as HTMLSelectElement).value = q.type;

            changeType(lastQ.querySelector("select") as HTMLSelectElement); // 🔥 belangrijk
        });
    });

    isLoading = false;

}