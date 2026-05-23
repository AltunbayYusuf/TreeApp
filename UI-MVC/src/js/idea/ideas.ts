// ideas/ideas.ts

type IdeaFilter = "all" | "mine" | "different";

interface IdeaSelectionGroup {
    title: string;
    reason: string;
    ideaIds: number[];
}

interface IdeaSelection {
    groups: IdeaSelectionGroup[];
}

export class IdeaViewer {
    private visibleCount: number = 2;
    private activeFilter: IdeaFilter = "all";
    private projectId: number = 0;
    private aiSelections = new Map<IdeaFilter, IdeaSelection>();

    init(): void {
        const pageData = document.getElementById("ideasPageData");
        this.projectId = Number(pageData?.dataset.projectId ?? 0);

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

    private async handleFilterChange(event: Event): Promise<void> {
        const button = event.currentTarget as HTMLElement | null;
        const nextFilter = button?.dataset.ideaFilter as IdeaFilter;

        if (!["all", "mine", "different"].includes(nextFilter)) {
            return;
        }

        this.activeFilter = nextFilter;
        this.visibleCount = 2;
        this.updateFilterButtons();

        if (nextFilter === "different" && !this.aiSelections.has(nextFilter)) {
            await this.fetchAiSelection(nextFilter);
        }

        this.updateIdeas();
    }

    private async fetchAiSelection(mode: IdeaFilter): Promise<void> {
        if (this.projectId <= 0 || mode !== "different") {
            return;
        }

        type IdeaSelectionResponse = {
            ok?: boolean;
            selection?: IdeaSelection;
        };

        try {
            const response = await fetch("/api/ideas/idea-selection", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
                body: JSON.stringify({
                    projectId: this.projectId,
                    selectionMode: mode
                })
            });

            if (!response.ok) {
                return;
            }

            const data = await response.json() as IdeaSelectionResponse;

            if (data.ok && data.selection) {
                this.aiSelections.set(mode, data.selection);
            }
        } catch {
            // AI-filter is optioneel. Als die faalt, blijven de gewone ideeën zichtbaar.
        }
    }

    private updateIdeas(): void {
        const ideas = Array.from(document.querySelectorAll<HTMLElement>(".idea-item"));
        const showMoreBtn = document.getElementById("show-more-btn") as HTMLElement | null;

        let filteredIdeas = ideas;

        if (this.activeFilter === "mine") {
            filteredIdeas = ideas.filter((idea) => idea.dataset.ownIdea === "true");
        }

        if (this.activeFilter === "different") {
            filteredIdeas = this.getAiOrderedIdeas(ideas, this.activeFilter);
        }

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

    private getAiOrderedIdeas(ideas: HTMLElement[], mode: IdeaFilter): HTMLElement[] {
        const selection = this.aiSelections.get(mode);

        if (!selection?.groups?.length) {
            return ideas;
        }

        const orderedIdeas: HTMLElement[] = [];
        const usedIdeaIds = new Set<number>();

        selection.groups.forEach((group) => {
            const ideaId = group.ideaIds[0];
            const idea = ideas.find((element) => Number(element.dataset.ideaId) === ideaId);

            if (idea && !usedIdeaIds.has(ideaId)) {
                orderedIdeas.push(idea);
                usedIdeaIds.add(ideaId);
            }
        });

        return orderedIdeas;
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