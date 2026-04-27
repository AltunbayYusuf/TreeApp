// survey/index.ts
import {DomUtils} from '../helpers/utils';

export class SurveySubmitter {
    init(): void {
        document.getElementById("verzendbtn")?.addEventListener("click", this.handleSubmit.bind(this));

        document.querySelectorAll<HTMLButtonElement>(".range-box").forEach(btn => {
            btn.addEventListener("click", this.handleRangeClick);
        });
    }

    private handleRangeClick(e: MouseEvent): void {
        const btn = e.currentTarget as HTMLButtonElement;
        const parent = btn.closest(".d-flex");
        if (!parent) return;

        parent.querySelectorAll<HTMLElement>(".range-box").forEach(b => b.classList.remove("active"));
        btn.classList.add("active");
    }

    private handleSubmit(e: MouseEvent): void {
        e.preventDefault();

        const formData = new URLSearchParams();
        const questions = document.querySelectorAll<HTMLElement>(".survey-question");
        let allFilled: boolean = true;

        questions.forEach((block, index) => {
            const questionId = block.getAttribute("data-question-id");
            const type = block.getAttribute("data-type");
            const {value, answered} = this.extractAnswer(block, type);

            if (!answered) {
                block.style.border = "2px solid red";
                allFilled = false;
            } else {
                block.style.border = "";
            }

            if (questionId) {
                formData.append(`answers[${index}].QuestionId`, questionId);
                formData.append(`answers[${index}].Value`, value);
            }
        });

        if (!allFilled) {
            alert("Niet alle vragen zijn beantwoord!");
            return;
        }

        const submitUrl = DomUtils.getProjectRedirectUrl("Survey/Submit");

        fetch(submitUrl, {
            method: "POST",
            headers: {"Content-Type": "application/x-www-form-urlencoded"},
            body: formData.toString()
        })
            .then(response => {
                if (response.ok) return response.json();
                throw new Error("Netwerk response was niet ok");
            })
            .then((data: { redirectUrl?: string }) => {
                if (data.redirectUrl) window.location.href = data.redirectUrl;
            })
            .catch(error => console.error("Fout bij verzenden:", error));
    }

    private extractAnswer(block: HTMLElement, type: string | null): { value: string, answered: boolean } {
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
}

document.addEventListener("DOMContentLoaded", () => new SurveySubmitter().init());