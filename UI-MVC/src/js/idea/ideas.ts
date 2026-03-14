function toonMeer(): void {
    const ideas = document.querySelectorAll<HTMLElement>(".idea-item");
    const showMoreBtn = document.getElementById("show-more-btn") as HTMLElement | null;

    let visibleCount: number = 2;

    function updateIdeas(): void {
        ideas.forEach((idea, index) => {
            idea.style.display = index < visibleCount ? "block" : "none";
        });

        if (showMoreBtn && visibleCount >= ideas.length) {
            showMoreBtn.style.display = "none";
        }
    }

    if (showMoreBtn) {
        showMoreBtn.addEventListener("click", () => {
            visibleCount += 3;
            updateIdeas();
        });
    }

    updateIdeas();
}

document.addEventListener("DOMContentLoaded", toonMeer);