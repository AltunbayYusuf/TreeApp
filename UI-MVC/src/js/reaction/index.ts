function escapeHtmll(str: string): string {
    return (str || "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

interface ReactionApiResponse {
    ok?: boolean;
    message?: string;
    warning?: string;
    explanation?: string;
    suggestedText?: string;
    isToxic?: boolean;
    saved?: boolean;
    added?: boolean;
    aiUnavailable?: boolean;
    count?: number;
}

document.addEventListener("DOMContentLoaded", () => {
    const emojiButtons = document.querySelectorAll<HTMLButtonElement>(".reaction-emoji-btn");

    emojiButtons.forEach((button) => {
        button.addEventListener("click", async function () {
            const ideaId = this.dataset.ideaId;
            const emoji = this.dataset.emoji ?? "";

            if (!ideaId) return;

            const hiddenInput = document.getElementById(`emoji-${ideaId}`) as HTMLInputElement | null;

            if (!hiddenInput) return;

            const countElement = this.querySelector<HTMLElement>(`.reaction-count[data-count-for="${ideaId}-${emoji}"]`);

            try {
                const response = await fetch("/api/reactions/toggle-emoji", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "Accept": "application/json"
                    },
                    body: JSON.stringify({
                        ideaId: parseInt(ideaId, 10),
                        emoji
                    })
                });

                const data: ReactionApiResponse = await response.json();
                if (!response.ok || !data.ok) return;

                const isSelected = !!data.added;
                this.classList.toggle("selected", isSelected);
                this.classList.toggle("btn-primary", isSelected);
                this.classList.toggle("btn-outline-secondary", !isSelected);
                hiddenInput.value = isSelected ? emoji : "";

                if (countElement && typeof data.count === "number") {
                    countElement.textContent = data.count.toString();
                }
            } catch (error) {
                console.error("Fout bij togglen van emoji:", error);
            }
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
            const reactionsList = document.getElementById(`reactions-list-${ideaId}`) as HTMLUListElement | null;
            const noReactionsMessage = document.getElementById(`no-reactions-${ideaId}`) as HTMLElement | null;

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
                                ${escapeHtmll(data.message ?? "Er ging iets mis.")}
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
                                <strong>${escapeHtmll(data.warning ?? "AI: je reactie bevat mogelijk toxische inhoud.")}</strong>
                                ${data.explanation ? `<div class="mt-2"><em>${escapeHtmll(data.explanation)}</em></div>` : ""}
                                ${data.suggestedText ? `<div class="mt-2"><strong>Alternatief:</strong><br>${escapeHtmll(data.suggestedText)}</div>` : ""}
                                <div class="mt-3 d-flex gap-2 flex-wrap">
                                    <button type="button" class="btn btn-outline-primary btn-sm use-alternative-btn">
                                        Alternatief gebruiken
                                    </button>
                                    <button type="button" class="btn btn-outline-danger btn-sm force-submit-btn">
                                        Toch versturen
                                    </button>
                                </div>
                            </div>
                        `;

                        const forceBtn = resultBox.querySelector(".force-submit-btn") as HTMLButtonElement | null;
                        const useAlternativeBtn = resultBox.querySelector(".use-alternative-btn") as HTMLButtonElement | null;

                        forceBtn?.addEventListener("click", async () => {
                            const forceResponse = await fetch("/api/reactions/force", {
                                method: "POST",
                                headers: {
                                    "Content-Type": "application/json"
                                },
                                body: JSON.stringify({
                                    ideaId: parseInt(ideaId, 10),
                                    emoji: emoji,
                                    text: text
                                })
                            });

                            const forceData: ReactionApiResponse = await forceResponse.json();

                            if (!forceResponse.ok) {
                                if (resultBox) {
                                    resultBox.style.display = "block";
                                    resultBox.innerHTML = `
                                        <div class="alert alert-danger mb-0">
                                            ${escapeHtmll(forceData.message ?? "Fout bij toch versturen.")}
                                        </div>
                                    `;
                                }
                                return;
                            }

                            if (resultBox) {
                                resultBox.style.display = "block";
                                resultBox.innerHTML = `
                                    <div class="alert alert-info mb-0">
                                        ${escapeHtmll(forceData.message ?? "Reactie doorgestuurd voor moderatie.")}
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
                        });

                        useAlternativeBtn?.addEventListener("click", () => {
                            textArea.value = data.suggestedText || "";
                            resultBox.style.display = "none";
                            resultBox.innerHTML = "";
                            textArea.focus();
                        });
                    }
                    return;
                }

                if (data.aiUnavailable) {
                    if (resultBox) {
                        resultBox.style.display = "block";
                        resultBox.innerHTML = `
                            <div class="alert alert-info mb-0">
                                ${escapeHtmll(data.message ?? "Reactie opgeslagen en doorgestuurd voor moderatie omdat AI tijdelijk niet beschikbaar was.")}
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

                    return;
                }

                if (data.saved) {
                    if (resultBox) {
                        resultBox.style.display = "block";
                        resultBox.innerHTML = `
                            <div class="alert alert-success mb-0">
                                ${escapeHtmll(data.message ?? "Reactie succesvol toegevoegd.")}
                            </div>
                        `;
                    }

                    if (reactionsList) {
                        const li = document.createElement("li");

                        let html = "";

                        if (emoji) {
                            html += `<span>${escapeHtmll(emoji)} </span>`;
                        }

                        if (text) {
                            html += `<span>${escapeHtmll(text)}</span>`;
                        }

                        li.innerHTML = html;
                        reactionsList.prepend(li);
                        reactionsList.style.display = "block";
                    }

                    if (noReactionsMessage) {
                        noReactionsMessage.style.display = "none";
                        noReactionsMessage.remove();
                    }

                    textArea.value = "";
                    emojiInput.value = "";

                    emojiButtonsForIdea.forEach((btn) => {
                        btn.classList.remove("selected");
                        btn.classList.remove("btn-primary");
                        btn.classList.add("btn-outline-secondary");
                    });
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