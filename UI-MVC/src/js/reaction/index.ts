document.addEventListener("DOMContentLoaded", () => {

    const emojiButtons = document.querySelectorAll<HTMLButtonElement>(".reaction-emoji-btn");

    emojiButtons.forEach(button => {
        button.addEventListener("click", function (e: MouseEvent) {

            const ideaId = this.dataset.ideaId;
            const emoji = this.dataset.emoji;

            if (!ideaId || !emoji) return;

            const hiddenInput = document.getElementById(`emoji-${ideaId}`) as HTMLInputElement | null;
            const buttonsForIdea = document.querySelectorAll<HTMLButtonElement>(`.reaction-emoji-btn[data-idea-id='${ideaId}']`);

            if (!hiddenInput) return;

            if (hiddenInput.value === emoji) {
                hiddenInput.value = "";
                this.classList.remove("selected");
                return;
            }

            buttonsForIdea.forEach(btn => btn.classList.remove("selected"));
            this.classList.add("selected");
            hiddenInput.value = emoji;

            e.preventDefault();
        });
    });


    const reactionForms = document.querySelectorAll<HTMLFormElement>(".reaction-form");

    reactionForms.forEach(form => {
        form.addEventListener("submit", (e: SubmitEvent) => {

            const textArea = form.querySelector<HTMLTextAreaElement>("textarea[name='text']");
            const emojiInput = form.querySelector<HTMLInputElement>("input[name='emoji']");

            const text = textArea?.value ?? "";
            const emoji = emojiInput?.value ?? "";

            if (text === "" && emoji === "") {
                e.preventDefault();
                alert("Je reactie is leeg.");
            }
            else if (!confirm("Wil je dit idee zeker verzenden?")) {
                e.preventDefault();
            }
        });
    });


    reactionForms.forEach(form => {
        form.addEventListener("submit", (e: SubmitEvent) => {

            const textArea = form.querySelector<HTMLTextAreaElement>("textarea[name='text']");
            const emojiInput = form.querySelector<HTMLInputElement>("input[name='emoji']");

            const text = textArea?.value ?? "";
            const emoji = emojiInput?.value ?? "";

            if (text === "" && emoji === "") {
                e.preventDefault();
                alert("Je reactie is leeg.");
            }
        });
    });

});