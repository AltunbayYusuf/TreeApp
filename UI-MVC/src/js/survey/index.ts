// survey/index.ts
import {DomUtils} from "../helpers/utils";

export class SurveySubmitter {
    init(): void {
        // start timer zodra survey opent
        sessionStorage.setItem(
            "surveyStartTime",
            Date.now().toString()
        );

        document.getElementById("verzendbtn")?.addEventListener(
            "click",
            this.handleSubmit.bind(this)
        );

        document.querySelectorAll<HTMLButtonElement>(".range-box").forEach(btn => {
            btn.addEventListener("click", (e) => this.handleRangeClick(e));
        });

        document.querySelectorAll<HTMLInputElement | HTMLTextAreaElement>(
            ".survey-question input, .survey-question textarea"
        ).forEach(input => {
            input.addEventListener("change", () => this.updateConditionalQuestions());
            input.addEventListener("input", () => this.updateConditionalQuestions());
        });

        this.updateConditionalQuestions();
    }

    private handleRangeClick(e: MouseEvent): void {
        const rangeBox = e.currentTarget as HTMLElement;
        const parent = rangeBox.closest(".d-flex");
        if (!parent) return;

        parent.querySelectorAll<HTMLElement>(".range-box").forEach(box => {
            box.classList.remove("active");
            box.classList.remove("btn-primary");
            box.classList.add("btn-outline-primary");
        });

        rangeBox.classList.add("active");
        rangeBox.classList.remove("btn-outline-primary");
        rangeBox.classList.add("btn-primary");

        const input = rangeBox.querySelector<HTMLInputElement>('input[type="radio"]');
        if (input) {
            input.checked = true;
            input.dispatchEvent(new Event("change", {bubbles: true}));
        }

        this.updateConditionalQuestions();
    }

    private handleSubmit(e: MouseEvent): void {
        e.preventDefault();

        const startTime = Number(
            sessionStorage.getItem("surveyStartTime")
        );

        const durationInSeconds = Math.floor(
            (Date.now() - startTime) / 1000
        );

        const answers: any[] = [];

        document.querySelectorAll<HTMLElement>(".survey-question").forEach(block => {
            if (block.classList.contains("d-none")) return;

            const questionId = block.dataset.questionId;
            const type = block.dataset.type ?? null;

            const result = this.extractAnswer(block, type);

            if (questionId && result.answered) {
                answers.push({
                    questionId: Number(questionId),
                    value: result.value
                });
            }
        });

        const projectId =
            Number(
                new URLSearchParams(window.location.search)
                    .get("projectId")
            ) ||
            Number(
                (document.querySelector("#surveyForm") as HTMLFormElement)
                    ?.dataset.projectId
            );

        const payload = {
            projectId,
            durationInSeconds,
            answers
        };

        const submitUrl = DomUtils.getProjectRedirectUrl("Survey/Submit");

        fetch(submitUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        })
            .then(r => r.json())
            .then(data => {
                if (data.redirectUrl) {
                    sessionStorage.removeItem("surveyStartTime");
                    window.location.href = data.redirectUrl;
                }
            });
    }

    private extractAnswer(block: HTMLElement, type: string | null): { value: string; answered: boolean } {
        let value = "";
        let answered = false;

        if (type === "SingleChoice" || type === "Range") {
            const selected = block.querySelector<HTMLInputElement>('input[type="radio"]:checked');
            if (selected) {
                value = selected.value;
                answered = true;
            }
        } else if (type === "MultipleChoice") {
            const selected = Array.from(block.querySelectorAll<HTMLInputElement>('input[type="checkbox"]:checked'));
            if (selected.length > 0) {
                value = selected.map(cb => cb.value).join(", ");
                answered = true;
            }
        } else if (type === "OpenQuestion") {
            const textarea = block.querySelector<HTMLTextAreaElement>(".question-text");
            if (textarea && textarea.value.trim() !== "") {
                value = textarea.value.trim();
                answered = true;
            }
        }

        return {value, answered};
    }

    private updateConditionalQuestions(): void {
        const allQuestions = Array.from(document.querySelectorAll<HTMLElement>(".survey-question"));
        const conditionalQuestions = allQuestions.filter(q => q.classList.contains("conditional-question"));

        conditionalQuestions.forEach(conditionalBlock => {
            const parentQuestionId = conditionalBlock.dataset.parentQuestionId;
            const triggerType = conditionalBlock.dataset.triggerType ?? "";
            const triggerValue = conditionalBlock.dataset.triggerValue ?? "";

            if (!parentQuestionId) return;

            const parentBlock = allQuestions.find(q => q.dataset.questionId === parentQuestionId);
            if (!parentBlock) return;

            const parentType = parentBlock.dataset.type ?? null;
            const { value: parentAnswer } = this.extractAnswer(parentBlock, parentType);

            const shouldShow = this.triggerMatches(parentAnswer, triggerType, triggerValue);

            if (shouldShow) {
                conditionalBlock.classList.remove("d-none");
            } else {
                conditionalBlock.classList.add("d-none");
                conditionalBlock.style.border = "";
                this.clearAnswer(conditionalBlock);
            }
        });
    }

    private triggerMatches(parentAnswer: string, triggerType: string, triggerValue: string): boolean {
        if (!parentAnswer || !triggerValue) return false;

        const answer = parentAnswer.toLowerCase().trim();
        const trigger = triggerValue.toLowerCase().trim();

        switch (triggerType) {
            case "Equals":
                return answer === trigger;

            case "Contains":
                return answer.includes(trigger);

            case "GreaterOrEqual": {
                const answerNumber = Number(parentAnswer);
                const triggerNumber = Number(triggerValue);
                return !Number.isNaN(answerNumber) &&
                    !Number.isNaN(triggerNumber) &&
                    answerNumber >= triggerNumber;
            }

            case "LessOrEqual": {
                const answerNumber = Number(parentAnswer);
                const triggerNumber = Number(triggerValue);
                return !Number.isNaN(answerNumber) &&
                    !Number.isNaN(triggerNumber) &&
                    answerNumber <= triggerNumber;
            }

            default:
                return false;
        }
    }

    private clearAnswer(block: HTMLElement): void {
        block.querySelectorAll<HTMLInputElement>('input[type="radio"], input[type="checkbox"]').forEach(input => {
            input.checked = false;
        });

        block.querySelectorAll<HTMLElement>(".range-box").forEach(btn => {
            btn.classList.remove("active");
        });

        const textarea = block.querySelector<HTMLTextAreaElement>(".question-text");
        if (textarea) {
            textarea.value = "";
        }
    }
}

document.addEventListener("DOMContentLoaded", () => new SurveySubmitter().init());