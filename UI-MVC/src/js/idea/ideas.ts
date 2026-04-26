// ideas/ideas.ts
export class IdeaViewer {
    private visibleCount: number = 2;

    init(): void {
        document.getElementById("show-more-btn")?.addEventListener("click", this.handleShowMore.bind(this));
        this.updateIdeas();
    }

    private handleShowMore(): void {
        this.visibleCount += 3;
        this.updateIdeas();
    }

    private updateIdeas(): void {
        const ideas = document.querySelectorAll<HTMLElement>(".idea-item");
        const showMoreBtn = document.getElementById("show-more-btn") as HTMLElement | null;

        ideas.forEach((idea, index) => {
            idea.style.display = index < this.visibleCount ? "block" : "none";
        });

        if (showMoreBtn && this.visibleCount >= ideas.length) {
            showMoreBtn.style.display = "none";
        }
    }
}

document.addEventListener("DOMContentLoaded", () => new IdeaViewer().init());