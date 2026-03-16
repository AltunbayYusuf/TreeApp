const button = document.getElementById("verzendbtn") as HTMLButtonElement | null;

if (button) {
    button.addEventListener("click", (e: MouseEvent) => {
        e.preventDefault();

        const formData = new URLSearchParams();
        const questions = document.querySelectorAll<HTMLElement>(".survey-question");
        let allFilled: boolean = true;

        questions.forEach((block, index) => {
            const questionId = block.getAttribute("data-question-id");
            const type = block.getAttribute("data-type");

            let resultValue: string = "";
            let questionAnswered: boolean = false;

            if (type === "SingleChoice") {
                const selected = block.querySelector<HTMLInputElement>('input[type="radio"]:checked');
                if (selected) {
                    resultValue = selected.value;
                    questionAnswered = true;
                }
            }
            else if (type === "MultipleChoice") {
                const selected = Array.from(
                    block.querySelectorAll<HTMLInputElement>('input[type="checkbox"]:checked')
                );

                if (selected.length > 0) {
                    resultValue = selected.map(cb => cb.value).join(", ");
                    questionAnswered = true;
                }
            }
            else if (type === "OpenQuestion") {
                const textarea = block.querySelector<HTMLTextAreaElement>(".question-text");

                if (textarea) {
                    resultValue = textarea.value.trim();
                    if (resultValue !== "") {
                        questionAnswered = true;
                    }
                }
            }
            else if (type === "Range") {
                const selected = block.querySelector<HTMLInputElement>('input[type="radio"]:checked');
                if (selected) {
                    resultValue = selected.value;
                    questionAnswered = true;
                }
            }

            if (!questionAnswered) {
                block.style.border = "2px solid red";
                allFilled = false;
            } else {
                block.style.border = "";
            }

            if (questionId) {
                formData.append(`answers[${index}].QuestionId`, questionId);
                formData.append(`answers[${index}].Value`, resultValue);
            }
        });

        if (!allFilled) {
            alert("Niet alle vragen zijn beantwoord!");
            return;
        }

        const params = new URLSearchParams(window.location.search);
        const projectId = params.get("projectId");

        const submitUrl = projectId
            ? `/Survey/Submit?projectId=${projectId}`
            : "/Survey/Submit";

        fetch(submitUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: formData.toString()
        })
            .then(response => {
                if (response.ok) return response.json();
                throw new Error("Netwerk response was niet ok");
            })
            .then((data: { redirectUrl?: string }) => {
                if (data.redirectUrl) {
                    window.location.href = data.redirectUrl;
                }
            })
            .catch(error => console.error("Fout bij verzenden:", error));
    });
}

document.querySelectorAll<HTMLButtonElement>(".range-box").forEach(btn => {
    btn.addEventListener("click", function () {
        const parent = this.closest(".d-flex");

        if (!parent) return;

        parent.querySelectorAll<HTMLElement>(".range-box").forEach(b => {
            b.classList.remove("active");
        });

        this.classList.add("active");
    });
});