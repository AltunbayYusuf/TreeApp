document.addEventListener("DOMContentLoaded", () => {
    const emojiButtons = document.querySelectorAll<HTMLButtonElement>(".reaction-emoji-btn");

    emojiButtons.forEach((button) => {
        button.addEventListener("click", function () {
            const ideaId = this.dataset.ideaId;
            const emoji = this.textContent?.trim() ?? "";

            if (!ideaId) return;

            const hiddenInput = document.getElementById(`emoji-${ideaId}`) as HTMLInputElement | null;
            const buttonsForIdea = document.querySelectorAll<HTMLButtonElement>(
                `.reaction-emoji-btn[data-idea-id='${ideaId}']`
            );

            if (!hiddenInput) return;

            if (hiddenInput.value === emoji) {
                this.classList.remove("selected");
                this.classList.remove("btn-primary");
                this.classList.add("btn-outline-secondary");
                hiddenInput.value = "";
                console.log("emoji verwijderd:", emoji);
                return;
            }

            buttonsForIdea.forEach((btn) => {
                btn.classList.remove("selected");
                btn.classList.remove("btn-primary");
                btn.classList.add("btn-outline-secondary");
            });

            this.classList.add("selected");
            this.classList.remove("btn-outline-secondary");
            this.classList.add("btn-primary");
            hiddenInput.value = emoji;

            console.log("emoji gezet:", hiddenInput.value);
        });
    });

    const reactionForms = document.querySelectorAll<HTMLFormElement>(".reaction-form");

    reactionForms.forEach((form) => {
        form.addEventListener("submit", async (e: SubmitEvent) => {
            e.preventDefault();

            const ideaId = form.dataset.ideaId;
            if (!ideaId) return;

            const textArea = form.querySelector("textarea[name='text']") as HTMLTextAreaElement | null;
            const emojiInput = form.querySelector("input[name='emoji']") as HTMLInputElement | null;

            if (!textArea || !emojiInput) return;

            const text = textArea.value.trim();
            const emoji = emojiInput.value.trim();

            const resultBox = document.getElementById(`reaction-result-${ideaId}`) as HTMLDivElement | null;
            const emojiButtonsForIdea = document.querySelectorAll<HTMLButtonElement>(
                `.reaction-emoji-btn[data-idea-id='${ideaId}']`
            );

            if (resultBox) {
                resultBox.style.display = "none";
                resultBox.innerHTML = "";
            }

            if (text === "" && emoji === "") {
                if (resultBox) {
                    resultBox.style.display = "block";
                    resultBox.innerHTML = `
                        <div class="alert alert-danger mb-0">
                            Schrijf een reactie of kies een emoji.
                        </div>
                    `;
                }
                return;
            }

            if (!confirm("Wil je deze reactie zeker verzenden?")) {
                return;
            }

            try {
                const response = await fetch("/api/reactions", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "Accept": "application/json"
                    },
                    body: JSON.stringify({
                        ideaId: parseInt(ideaId, 10),
                        emoji: emoji,
                        text: text
                    })
                });

                const data: ReactionApiResponse = await response.json();

                if (!response.ok) {
                    if (resultBox) {
                        resultBox.style.display = "block";
                        resultBox.innerHTML = `
                            <div class="alert alert-danger mb-0">
                                ${data.message ?? "Er ging iets mis."}
                            </div>
                        `;
                    }
                    return;
                }

                if (data.isToxic) {
                    if (resultBox) {
                        resultBox.style.display = "block";
                        resultBox.innerHTML = `
                            <div class="alert alert-warning mb-0">
                                <strong>${data.warning ?? "AI: je reactie bevat mogelijk toxische inhoud."}</strong>
                                ${data.explanation ? `<div class="mt-2"><em>${data.explanation}</em></div>` : ""}
                                ${data.suggestedText ? `<div class="mt-2"><strong>Alternatief:</strong><br>${data.suggestedText}</div>` : ""}
                                <div class="mt-3 d-flex gap-2 flex-wrap">
                                    <button type="button" class="btn btn-outline-primary btn-sm use-alternative-btn">
                                        Alternatief gebruiken
                                    </button>
                                    <button type="button" id="force-submit-btn" class="btn btn-outline-danger btn-sm">
                                        Toch versturen
                                    </button>
                                </div>
                            </div>
                        `;

                        const useAlternativeBtn = resultBox.querySelector(".use-alternative-btn") as HTMLButtonElement | null;

                        if (useAlternativeBtn) {
                            useAlternativeBtn.addEventListener("click", () => {
                                textArea.value = data.suggestedText || "";
                                resultBox.style.display = "none";
                                resultBox.innerHTML = "";
                                textArea.focus();
                            });
                        }
                    }
                    return;
                }

                if (data.saved) {
                    if (resultBox) {
                        resultBox.style.display = "block";
                        resultBox.innerHTML = `
                            <div class="alert alert-success mb-0">
                                ${data.message ?? "Reactie succesvol toegevoegd."}
                            </div>
                        `;
                    }

                    textArea.value = "";
                    emojiInput.value = "";

                    emojiButtonsForIdea.forEach((btn) => {
                        btn.classList.remove("selected");
                        btn.classList.remove("btn-primary");
                        btn.classList.add("btn-outline-secondary");
                    });

                    setTimeout(() => {
                        location.reload();
                    }, 700);
                }
            } catch (error) {
                console.error("Fout bij verzenden van reactie:", error);

                if (resultBox) {
                    resultBox.style.display = "block";
                    resultBox.innerHTML = `
                        <div class="alert alert-danger mb-0">
                            Er ging iets mis bij het verzenden van je reactie.
                        </div>
                    `;
                }
            }
        });
    });
});

interface ReactionApiResponse {
    message?: string;
    warning?: string;
    explanation?: string;
    suggestedText?: string;
    isToxic?: boolean;
    saved?: boolean;
}