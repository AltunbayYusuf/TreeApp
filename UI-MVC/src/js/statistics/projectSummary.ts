class ProjectSummary {
    init(): void {
        this.initializeProjectSummary();
        this.initializeOpenQuestionSummaries();
    }

    private initializeProjectSummary(): void {
        const form = document.getElementById("projectSummaryForm") as HTMLFormElement | null;

        if (!form) {
            return;
        }

        form.addEventListener("submit", async (event) => {
            event.preventDefault();

            const button = document.getElementById("projectSummaryButton") as HTMLButtonElement | null;
            const loading = document.getElementById("projectSummaryLoading");
            const error = document.getElementById("projectSummaryError");
            const result = document.getElementById("projectSummaryResult");

            if (button) {
                button.disabled = true;
                button.textContent = "AI genereert...";
            }

            loading?.classList.remove("d-none");
            error?.classList.add("d-none");

            try {
                const response = await fetch(form.action, {
                    method: "POST",
                    body: new FormData(form)
                });

                const data = await response.json() as {
                    ok?: boolean;
                    summary?: string;
                };

                if (!response.ok || !data.ok || !data.summary) {
                    throw new Error();
                }

                if (result) {
                    result.classList.remove("text-muted");
                    result.textContent = data.summary;
                }
            } catch {
                error?.classList.remove("d-none");
            } finally {
                loading?.classList.add("d-none");

                if (button) {
                    button.disabled = false;
                    button.textContent = "Genereer algemene samenvatting";
                }
            }
        });
    }
    private initializeOpenQuestionSummaries(): void {
        const forms = document.querySelectorAll<HTMLFormElement>(".open-question-summary-form");

        forms.forEach(form => {
            form.addEventListener("submit", async (event) => {
                event.preventDefault();

                const card = form.closest(".card-body") ?? form.parentElement;

                const button = form.querySelector<HTMLButtonElement>(".open-question-summary-button");
                const loading = card?.querySelector(".open-question-loading");
                const error = card?.querySelector(".open-question-error");

                const summaryText = card?.querySelector(".open-question-summary-text");
                if (button) {
                    button.disabled = true;
                }

                loading?.classList.remove("d-none");
                error?.classList.add("d-none");

                try {
                    const response = await fetch(form.action, {
                        method: "POST",
                        body: new FormData(form)
                    });

                    const data = await response.json() as {
                        ok?: boolean;
                        summary?: string;
                    };

                    if (!response.ok || !data.ok || !data.summary) {
                        throw new Error();
                    }

                    if (summaryText) {
                        summaryText.textContent = data.summary;
                        summaryText.classList.remove("text-muted");
                    }

                    form.remove();
                }
                catch {
                    error?.classList.remove("d-none");
                }
                finally {
                    loading?.classList.add("d-none");

                    if (button) {
                        button.disabled = false;
                    }
                }
            });
        });
    }
}


document.addEventListener("DOMContentLoaded", () => {
    new ProjectSummary().init();
});