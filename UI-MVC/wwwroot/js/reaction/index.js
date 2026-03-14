document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".reaction-emoji-btn").forEach(button => {
        button.addEventListener("click", function (e) {
            const ideaId = this.dataset.ideaId;
            const emoji = this.textContent.trim();
            const hiddenInput = document.getElementById("emoji-" + ideaId);
            const buttonsForIdea = document.querySelectorAll(`.reaction-emoji-btn[data-idea-id='${ideaId}']`);

            if (!hiddenInput) return;

            if (hiddenInput.value === emoji) {
                this.classList.remove("selected");
                this.classList.remove("btn-primary");
                this.classList.add("btn-outline-secondary");
                console.log("emoji verwijderd:", hiddenInput.value);
                return;
            }

            buttonsForIdea.forEach(btn => {
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

    document.querySelectorAll(".reaction-form").forEach(form => {
        form.addEventListener("submit", async function (e) {
            e.preventDefault();

            const ideaId = form.dataset.ideaId;
            const text = form.querySelector("textarea[name='text']").value.trim();
            const emoji = form.querySelector("input[name='emoji']").value.trim();
            const resultBox = document.getElementById("reaction-result-" + ideaId);
            const emojiButtonsForIdea = document.querySelectorAll(`.reaction-emoji-btn[data-idea-id='${ideaId}']`);

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

            const response = await fetch("/api/reactions", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
                body: JSON.stringify({
                    ideaId: parseInt(ideaId),
                    emoji: emoji,
                    text: text
                })
            });

            const data = await response.json();

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
                            </div>
                        </div>
                    `;

                    const useAlternativeBtn = resultBox.querySelector(".use-alternative-btn");
                    if (useAlternativeBtn) {
                        useAlternativeBtn.addEventListener("click", function () {
                            form.querySelector("textarea[name='text']").value = data.suggestedText || "";
                            resultBox.style.display = "none";
                            resultBox.innerHTML = "";
                            form.querySelector("textarea[name='text']").focus();
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

                form.querySelector("textarea[name='text']").value = "";
                form.querySelector("input[name='emoji']").value = "";

                emojiButtonsForIdea.forEach(btn => {
                    btn.classList.remove("selected");
                    btn.classList.remove("btn-primary");
                    btn.classList.add("btn-outline-secondary");
                });

                setTimeout(() => {
                    location.reload();
                }, 700);
            }
        });
    });
});