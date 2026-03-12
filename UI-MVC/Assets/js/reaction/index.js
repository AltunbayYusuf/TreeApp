document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".reaction-emoji-btn").forEach(button => {
        button.addEventListener("click", function (e) {
            const ideaId = this.dataset.ideaId;
            const emoji = this.dataset.emoji;
            const hiddenInput = document.getElementById("emoji-" + ideaId);
            const buttonsForIdea = document.querySelectorAll(`.reaction-emoji-btn[data-idea-id='${ideaId}']`);

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

    document.querySelectorAll(".reaction-form").forEach(form => {
        form.addEventListener("submit", function (e) {

            const text = form.querySelector("textarea[name='text']").value;
            const emoji = form.querySelector("input[name='emoji']").value;

            if (text === "" && emoji === "") {
                e.preventDefault();
                alert("Je reactie is leeg.");
            }
            else if (!confirm("Wil je dit idee zeker verzenden?")) {
                e.preventDefault();
                return;
            }
        });
    });

    document.querySelectorAll(".reaction-form").forEach(form => {
        form.addEventListener("submit", function (e) {

            const text = form.querySelector("textarea[name='text']").value;
            const emoji = form.querySelector("input[name='emoji']").value;

            if (text === "" && emoji === "") {
                e.preventDefault();
                alert("Je reactie is leeg.");
            }
        });
    });
});