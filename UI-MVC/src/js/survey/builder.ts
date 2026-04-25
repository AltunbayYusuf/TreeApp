import {SurveyStorage} from './storage.ts';
import {SurveyTemplates} from './templates.ts';
import {SectionData} from '../helpers/types.ts';

export class SurveyBuilder {
    private questionCount = 0;
    private readonly maxQuestions = 20;
    private sectionCount = 0;
    private isLoading = false;
    private saveTimeout?: number;

    init(): void {
        this.exposeToWindow();
        document.addEventListener("input", () => this.handleInputDebounce());

        const data = SurveyStorage.load();
        if (data && data.length > 0) {
            this.loadFromLocalStorage(data);
        } else {
            this.createInitialSurvey();
        }
        this.updateCounter();
    }

    private exposeToWindow(): void {
        const w = window as any;
        w.addSection = () => this.addSection();
        w.removeSection = (btn: HTMLElement) => this.removeSection(btn);
        w.addQuestion = (btn: HTMLElement) => this.addQuestion(btn);
        w.removeQuestion = (btn: HTMLElement) => this.removeQuestion(btn);
        w.changeType = (sel: HTMLSelectElement) => this.changeType(sel);
        w.addConditional = (btn: HTMLElement) => this.addConditional(btn);
        w.removeConditional = (btn: HTMLElement) => this.removeConditional(btn);
        w.removeAnswer = (btn: HTMLElement) => this.removeAnswer(btn);
        w.toggleAI = (cb: HTMLInputElement) => this.toggleAI(cb);
    }

    private handleInputDebounce(): void {
        clearTimeout(this.saveTimeout);
        this.saveTimeout = window.setTimeout(() => this.saveToLocalStorage(), 300);
    }

    private updateCounter(): void {
        const counter = document.getElementById("questionCounter");
        if (counter) counter.innerText = `${this.questionCount} / ${this.maxQuestions} vragen`;
    }

    private saveToLocalStorage(): void {
        if (this.isLoading) return;
        SurveyStorage.save(SurveyStorage.extractFromDOM());
    }

    addSection(): void {
        this.sectionCount++;
        document.getElementById("sections")?.appendChild(SurveyTemplates.createSection(this.sectionCount));
        this.saveToLocalStorage();
    }

    removeSection(btn: HTMLElement): void {
        const section = btn.closest(".section");
        if (document.querySelectorAll(".section").length <= 1) return alert("Minimaal 1 sectie!");
        this.questionCount -= section?.querySelectorAll(".question").length ?? 0;
        section?.remove();
        this.updateCounter();
        this.saveToLocalStorage();
    }

    addQuestion(btn: HTMLElement): void {
        if (this.questionCount >= this.maxQuestions) return alert("Maximum bereikt!");
        this.questionCount++;
        btn.closest(".section")?.querySelector(".questions")?.appendChild(SurveyTemplates.createQuestion());
        this.updateCounter();
        this.saveToLocalStorage();
    }

    removeQuestion(btn: HTMLElement): void {
        if (this.questionCount <= 1) return alert("Minimaal 1 vraag!");
        btn.closest(".question")?.remove();
        this.questionCount--;
        this.updateCounter();
        this.saveToLocalStorage();
    }

    changeType(select: HTMLSelectElement): void {
        const container = select.parentElement?.querySelector(".answers") as HTMLElement;
        container.innerHTML = "";
        if (["single", "multiple"].includes(select.value)) {
            const list = document.createElement("div");
            list.className = "answers-list space-y-2";
            list.appendChild(SurveyTemplates.createAnswer());
            list.appendChild(SurveyTemplates.createAnswer());
            container.appendChild(list);

            const addBtn = document.createElement("button");
            addBtn.className = "text-indigo-600 text-sm mt-2";
            addBtn.innerText = "+ Antwoord";
            addBtn.onclick = () => list.appendChild(SurveyTemplates.createAnswer());
            container.appendChild(addBtn);
        }
        this.saveToLocalStorage();
    }

    removeAnswer(btn: HTMLElement): void {
        const list = btn.closest(".answers-list");
        if (list && list.querySelectorAll(".answer-row").length <= 2) return alert("Minimaal 2 antwoorden!");
        btn.closest(".answer-row")?.remove();
        this.saveToLocalStorage();
    }

    addConditional(btn: HTMLElement): void {
        btn.parentElement?.querySelector(".conditional-container")?.appendChild(SurveyTemplates.createConditional());
        this.saveToLocalStorage();
    }

    removeConditional(btn: HTMLElement): void {
        btn.closest(".conditional-block")?.remove();
        this.saveToLocalStorage();
    }

    toggleAI(cb: HTMLInputElement): void {
        const input = cb.closest(".conditional-block")?.querySelector(".conditional-input") as HTMLInputElement;
        input.disabled = cb.checked;
        if (cb.checked) input.value = "";
        this.saveToLocalStorage();
    }

    private createInitialSurvey(): void {
        this.isLoading = true;
        this.addSection();
        this.addQuestion(document.querySelector(".add-question-btn") as HTMLElement);
        this.isLoading = false;
    }

    private loadFromLocalStorage(data: SectionData[]): void {
        this.isLoading = true;
        const container = document.getElementById("sections")!;
        container.innerHTML = "";
        this.questionCount = 0;

        data.forEach(sData => {
            this.addSection();
            const section = container.lastElementChild as HTMLElement;
            (section.querySelector(".section-title") as HTMLInputElement).value = sData.title;

            sData.questions.forEach(q => {
                this.addQuestion(section.querySelector(".add-question-btn") as HTMLElement);
                const qEl = section.querySelector(".questions")?.lastElementChild as HTMLElement;
                (qEl.querySelector(".question-title") as HTMLInputElement).value = q.title;
                const sel = qEl.querySelector("select")!;
                sel.value = q.type;
                this.changeType(sel as HTMLSelectElement);

                if (["single", "multiple"].includes(q.type)) {
                    const list = qEl.querySelector(".answers-list")!;
                    list.innerHTML = "";
                    q.answers.forEach(a => {
                        const row = SurveyTemplates.createAnswer();
                        (row.querySelector("input")!).value = a;
                        list.appendChild(row);
                    });
                }
                // ... laad overige velden zoals conditionals ...
            });
        });
        this.isLoading = false;
    }
}

document.addEventListener("DOMContentLoaded", () => new SurveyBuilder().init());