// ideas/ideas.ts
export class IdeaViewer {
    private visibleCount: number = 2;
    private activeFilter: "all" | "mine" = "all";

    init(): void {
        document.getElementById("show-more-btn")?.addEventListener("click", this.handleShowMore.bind(this));
        document.querySelectorAll<HTMLElement>("[data-idea-filter]").forEach((button) => {
            button.addEventListener("click", this.handleFilterChange.bind(this));
        });
        this.updateIdeas();
    }

    private handleShowMore(): void {
        this.visibleCount += 3;
        this.updateIdeas();
    }

    private handleFilterChange(event: Event): void {
        const button = event.currentTarget as HTMLElement | null;
        const nextFilter = button?.dataset.ideaFilter;

        if (nextFilter !== "all" && nextFilter !== "mine") {
            return;
        }

        this.activeFilter = nextFilter;
        this.visibleCount = 2;
        this.updateFilterButtons();
        this.updateIdeas();
    }

    private updateIdeas(): void {
        const ideas = Array.from(document.querySelectorAll<HTMLElement>(".idea-item"));
        const showMoreBtn = document.getElementById("show-more-btn") as HTMLElement | null;
        const filteredIdeas = ideas.filter((idea) => this.activeFilter === "all" || idea.dataset.ownIdea === "true");

        ideas.forEach((idea) => {
            idea.style.display = "none";
        });

        filteredIdeas.forEach((idea, index) => {
            idea.style.display = index < this.visibleCount ? "block" : "none";
        });

        if (showMoreBtn) {
            showMoreBtn.style.display = this.visibleCount < filteredIdeas.length ? "inline-block" : "none";
        }
    }

    private updateFilterButtons(): void {
        document.querySelectorAll<HTMLButtonElement>("[data-idea-filter]").forEach((button) => {
            const isActive = button.dataset.ideaFilter === this.activeFilter;
            button.classList.toggle("btn-dark", isActive);
            button.classList.toggle("btn-outline-secondary", !isActive);
        });
    }
}

document.addEventListener("DOMContentLoaded", () => new IdeaViewer().init());