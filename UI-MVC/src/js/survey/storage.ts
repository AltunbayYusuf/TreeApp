import {SectionData, QuestionData, ConditionalData} from '../helpers/types';

export class SurveyStorage {
    private static readonly KEY = "surveyDraft";

    static save(sections: SectionData[]): void {
        sessionStorage.setItem(this.KEY, JSON.stringify(sections));
    }

    static load(): SectionData[] | null {
        const data = sessionStorage.getItem(this.KEY);
        return data ? JSON.parse(data) : null;
    }

    static extractFromDOM(): SectionData[] {
        const sections: SectionData[] = [];
        document.querySelectorAll(".section").forEach(section => {
            const title = (section.querySelector(".section-title") as HTMLInputElement | null)?.value ?? "";
            const questions: QuestionData[] = [];

            section.querySelectorAll(".question").forEach(question => {
                const qTitle = (question.querySelector(".question-title") as HTMLInputElement | null)?.value ?? "";
                const type = (question.querySelector("select") as HTMLSelectElement | null)?.value ?? "";
                const answers: string[] = [];
                question.querySelectorAll(".answers-list input").forEach(answer => {
                    answers.push((answer as HTMLInputElement).value);
                });
                const min = (question.querySelector('input[placeholder="Min"]') as HTMLInputElement | null)?.value ?? "";
                const max = (question.querySelector('input[placeholder="Max"]') as HTMLInputElement | null)?.value ?? "";

                const conditionals: ConditionalData[] = [];
                question.querySelectorAll(".conditional-container > div.conditional-block").forEach(c => {
                    conditionals.push({
                        trigger: (c.querySelector("input") as HTMLInputElement | null)?.value ?? "",
                        ai: (c.querySelector("input[type='checkbox']") as HTMLInputElement | null)?.checked ?? false,
                        question: (c.querySelector(".conditional-input") as HTMLInputElement | null)?.value ?? ""
                    });
                });
                questions.push({title: qTitle, type, answers, min, max, conditionals});
            });
            sections.push({title, questions});
        });
        return sections;
    }
}