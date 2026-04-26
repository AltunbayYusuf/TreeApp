export class SurveyTemplates {
    static createSection(sectionCount: number): HTMLDivElement {
        const div = document.createElement("div");
        div.className = "section bg-white p-5 rounded-xl shadow-sm border border-slate-200";
        div.innerHTML = `
            <div class="flex justify-between items-center gap-3 mb-3">
                <input type="text" class="section-title w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 focus:ring-2 focus:ring-indigo-100" placeholder="Sectie ${sectionCount} titel..." />
                <button type="button" onclick="removeSection(this)" class="text-red-500 hover:bg-red-50 p-2 rounded-lg transition">🗑</button>
            </div>
            <div class="questions space-y-3"></div>
            <button type="button" onclick="addQuestion(this)" class="mt-4 bg-indigo-600 text-white px-4 py-2 rounded-lg text-sm">+ Vraag toevoegen</button>
        `;
        return div;
    }

    static createQuestion(): HTMLDivElement {
        const div = document.createElement("div");
        div.className = "question rounded-lg border border-slate-200 bg-white p-4";
        div.innerHTML = `
            <div class="flex justify-between items-center mb-2">
                <span class="text-sm text-slate-500">Vraag</span>
                <button type="button" onclick="removeQuestion(this)" class="text-red-500">🗑</button>
            </div>
            <input type="text" class="question-title mb-3 w-full border rounded-lg p-3 text-sm" placeholder="Vraag titel..." />
            <select onchange="changeType(this)" class="mb-3 w-full border bg-gray-50 rounded-lg p-3 text-sm">
                <option value="" disabled selected>Kies type...</option>
                <option value="single">Enkelkeuze</option>
                <option value="multiple">Meerkeuze</option>
                <option value="range">Range</option>
                <option value="open">Open vraag</option>
            </select>
            <div class="answers space-y-2"></div>
            <button type="button" onclick="addConditional(this)" class="mt-3 text-sm text-indigo-600">+ Conditionele vraag</button>
            <div class="conditional-container"></div>
        `;
        return div;
    }

    static createAnswer(): HTMLDivElement {
        const div = document.createElement("div");
        div.className = "flex items-center gap-2 answer-row";
        div.innerHTML = `
            <input placeholder="Antwoord..." class="w-full border rounded-lg p-3 text-sm" />
            <button type="button" onclick="removeAnswer(this)" class="text-red-500">🗑</button>
        `;
        return div;
    }

    static createConditional(): HTMLDivElement {
        const div = document.createElement("div");
        div.className = "conditional-block mt-3 rounded-lg border bg-indigo-50 p-3";
        div.innerHTML = `
            <div class="flex justify-between items-center mb-2">
                <span class="text-xs font-bold text-indigo-500 uppercase">Conditioneel</span>
                <button type="button" onclick="removeConditional(this)" class="text-red-500 text-sm">🗑</button>
            </div>
            <input placeholder="Trigger antwoord" class="mb-2 w-full border rounded-lg p-2 text-sm" />
            <label class="flex gap-2 text-sm mb-2"><input type="checkbox" onchange="toggleAI(this)" /> AI genereren</label>
            <input class="conditional-input w-full border rounded-lg p-2 text-sm" placeholder="Vraag..." />
        `;
        return div;
    }
}