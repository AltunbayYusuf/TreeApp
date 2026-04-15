let questionCount = 0;
const maxQuestions = 20;

//   INIT
document.addEventListener("DOMContentLoaded", () => {
    updateCounter();

    (window as any).addSection = addSection;
    (window as any).addQuestion = addQuestion;
    (window as any).changeType = changeType;
    (window as any).addConditional = addConditional;
});

//   UI LOGIC

function updateCounter() {
    const counter = document.getElementById("questionCounter");
    if (counter) {
        counter.innerText = `${questionCount} / ${maxQuestions} vragen`;
    }
}

function addSection() {
    const container = document.getElementById("sections");
    if (!container) return;

    const section = document.createElement("div");
    section.className = "bg-white p-5 rounded-xl shadow-sm border border-slate-200";

    section.innerHTML = `
        <h4 class="text-lg font-semibold text-slate-800 mb-3">Sectie</h4>
        <div class="questions space-y-3"></div>
    `;

    container.appendChild(section);
}

function addQuestion() {
    if (questionCount >= maxQuestions) {
        alert("Max 20 vragen toegestaan");
        return;
    }

    const section = document.querySelector(".section:last-child .questions")
        || document.querySelector("#sections .questions:last-child");

    if (!section) {
        alert("Maak eerst een sectie");
        return;
    }

    questionCount++;
    updateCounter();

    const questionDiv = document.createElement("div");
    questionDiv.className = "border border-slate-200 p-4 rounded-lg bg-white";

    questionDiv.innerHTML = `
        <input type="text"
               class="question-title w-full mb-3 rounded-md border border-slate-300 px-3 py-2 text-sm"
               placeholder="Vraag titel..." />

        <select onchange="changeType(this)"
                class="mb-3 rounded-md border border-slate-300 px-3 py-2 text-sm w-full">
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

    section.appendChild(questionDiv);
}

function changeType(select: HTMLSelectElement) {
    const container = select.parentElement?.querySelector(".answers");
    if (!container) return;

    container.innerHTML = "";

    const type = select.value;

    if (type === "single" || type === "multiple") {
        for (let i = 0; i < 2; i++) {
            addAnswer(container);
        }

        const btn = document.createElement("button");
        btn.type = "button";
        btn.innerText = "+ Antwoord";
        btn.className = "text-sm text-indigo-600 hover:underline mt-2";
        btn.onclick = () => addAnswer(container);

        container.appendChild(btn);
    }

    if (type === "range") {
        container.innerHTML = `
            <div class="flex gap-3">
                <div>
                    <label class="text-xs text-slate-500">Min</label>
                    <input type="number" min="3" max="10" value="3"
                           class="w-full rounded-md border border-slate-300 px-2 py-1 text-sm" />
                </div>
                <div>
                    <label class="text-xs text-slate-500">Max</label>
                    <input type="number" min="3" max="10" value="5"
                           class="w-full rounded-md border border-slate-300 px-2 py-1 text-sm" />
                </div>
            </div>
        `;
    }
}

function addAnswer(container: Element) {
    const input = document.createElement("input");
    input.placeholder = "Antwoord...";
    input.className = "w-full rounded-md border border-slate-300 px-3 py-2 text-sm";
    container.appendChild(input);
}

function addConditional(btn: HTMLElement) {
    const parent = btn.parentElement;
    if (!parent) return;

    const container = parent.querySelector(".conditional-container");
    if (!container) return;

    const div = document.createElement("div");
    div.className = "bg-indigo-50 p-3 mt-3 rounded-lg border border-indigo-200";

    div.innerHTML = `
        <label class="text-xs text-slate-600">Trigger antwoord:</label>
        <input type="text"
               class="w-full mb-2 rounded-md border border-slate-300 px-2 py-1 text-sm"
               placeholder="bv. Slecht" />

        <label class="text-xs text-slate-600">Vraag type:</label>
        <select class="w-full mb-2 rounded-md border border-slate-300 px-2 py-1 text-sm">
            <option>AI laten genereren</option>
            <option>Zelf schrijven</option>
        </select>

        <input type="text"
               class="w-full rounded-md border border-slate-300 px-2 py-1 text-sm"
               placeholder="Conditionele vraag..." />
    `;

    container.appendChild(div);
}

//   DATA COLLECTIE


function collectSurveyData() {
    const sections: any[] = [];

    document.querySelectorAll("#sections > div").forEach((sectionEl, sIndex) => {

        const section: any = {
            title: `Sectie ${sIndex + 1}`,
            order: sIndex,
            questions: []
        };

        sectionEl.querySelectorAll(".question, .border").forEach((qEl) => {

            const titleInput = qEl.querySelector(".question-title") as HTMLInputElement;
            const select = qEl.querySelector("select") as HTMLSelectElement;

            if (!titleInput || !select) return;

            const question: any = {
                description: titleInput.value,
                questionType: mapType(select.value),
                options: [],
                rangeMin: null,
                rangeMax: null
            };

            if (select.value === "single" || select.value === "multiple") {
                qEl.querySelectorAll(".answers input").forEach(i => {
                    const val = (i as HTMLInputElement).value;
                    if (val.trim() !== "") {
                        question.options.push(val);
                    }
                });
            }

            if (select.value === "range") {
                const inputs = qEl.querySelectorAll(".answers input");

                const min = parseInt((inputs[0] as HTMLInputElement).value);
                const max = parseInt((inputs[1] as HTMLInputElement).value);

                question.rangeMin = min;
                question.rangeMax = max;
            }

            section.questions.push(question);
        });

        sections.push(section);
    });

    return sections;
}

function mapType(type: string) {
    switch (type) {
        case "single":
            return 0;
        case "multiple":
            return 1;
        case "range":
            return 2;
        case "open":
            return 3;
        default:
            return 3;
    }
}

// export

(window as any).collectSurveyData = collectSurveyData;