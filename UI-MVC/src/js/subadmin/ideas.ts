// subadmin/ideas.ts
import { DomUtils } from '../helpers/utils';

type SortOrder = 'newest' | 'oldest';
type SimilarityFilter = 'all' | 'common' | 'unique';

interface ReactionDto {
    id: number;
    text: string;
    emoji: string;
    status: string;
}

interface IdeaDto {
    id: number;
    title: string;
    text: string;
    status: string;
    topic: string;
    project: string;
    projectId: number;
    reactions: ReactionDto[];
}

export class SubAdminIdeas {
    private subplatformId: number = 0;
    private allIdeas: IdeaDto[] = [];
    private sortOrder: SortOrder = 'newest';
    private similarityFilter: SimilarityFilter = 'all';

    init(): void {
        const mount = document.getElementById('subplatformIdData');
        if (!mount) return;

        this.subplatformId = Number(mount.dataset.id ?? 0);
        if (!this.subplatformId) return;

        document.getElementById('ideeen-tab')?.addEventListener('shown.bs.tab', this.handleTabOpen.bind(this));
        document.getElementById('projectFilter')?.addEventListener('change', this.handleProjectChange.bind(this));

        document.querySelectorAll<HTMLButtonElement>('[data-similarity]').forEach(btn => {
            btn.addEventListener('click', this.handleSimilarityClick.bind(this));
        });

        document.querySelectorAll<HTMLButtonElement>('[data-sort]').forEach(btn => {
            btn.addEventListener('click', this.handleSortClick.bind(this));
        });
    }

    private handleTabOpen(): void {
        const projectSelect = document.getElementById('projectFilter') as HTMLSelectElement | null;
        this.fetchAndRender(projectSelect?.value ? Number(projectSelect.value) : null);
    }

    private handleProjectChange(e: Event): void {
        const select = e.currentTarget as HTMLSelectElement;
        const projectId = select.value ? Number(select.value) : null;

        this.similarityFilter = 'all';
        this.updateSimilarityButtons();
        this.toggleSimilarityGroup(projectId !== null);

        this.fetchAndRender(projectId);
    }

    private handleSimilarityClick(e: MouseEvent): void {
        const btn = e.currentTarget as HTMLButtonElement;
        this.similarityFilter = (btn.dataset.similarity ?? 'all') as SimilarityFilter;
        this.updateSimilarityButtons();
        this.renderRows();
    }

    private handleSortClick(e: MouseEvent): void {
        const btn = e.currentTarget as HTMLButtonElement;
        this.sortOrder = (btn.dataset.sort ?? 'newest') as SortOrder;
        this.updateSortButtons();
        this.renderRows();
    }

    private async fetchAndRender(projectId: number | null): Promise<void> {
        this.showLoading(true);

        try {
            let url = `/api/ideas?subplatformId=${this.subplatformId}`;
            if (projectId !== null) url += `&projectId=${projectId}`;

            const response = await fetch(url);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);

            this.allIdeas = await response.json() as IdeaDto[];
            this.renderRows();
        } catch (error) {
            console.error('Fout bij ophalen ideeën:', error);
            this.showError('Kon ideeën niet ophalen. Probeer opnieuw.');
        } finally {
            this.showLoading(false);
        }
    }

    private getFilteredAndSortedIdeas(): IdeaDto[] {
        let ideas = this.allIdeas;

        if (this.sortOrder === 'oldest') {
            ideas = [...ideas].reverse();
        }

        return ideas;
    }

    private renderRows(): void {
        const tbody = document.getElementById('ideasTableBody');
        if (!tbody) return;

        document.querySelectorAll<HTMLTableRowElement>('tr.idea-row').forEach(r => r.remove());

        const ideas = this.getFilteredAndSortedIdeas();
        this.updateCounter(ideas.length);

        const emptyRow = document.getElementById('ideasEmptyRow');
        if (ideas.length === 0) {
            if (emptyRow) emptyRow.style.display = '';
            return;
        }
        if (emptyRow) emptyRow.style.display = 'none';

        ideas.forEach(idea => {
            tbody.appendChild(this.buildIdeaRow(idea));
            tbody.appendChild(this.buildReactionsRow(idea));
        });
    }

    private buildIdeaRow(idea: IdeaDto): HTMLTableRowElement {
        const tr = document.createElement('tr');
        tr.className = 'idea-row align-middle';

        const statusCfg = this.getStatusConfig(idea.status);
        const reactionCount = idea.reactions?.length ?? 0;

        tr.innerHTML = `
            <td class="p-3 fw-semibold" style="max-width:180px">
                ${DomUtils.escapeHtml(idea.title)}
            </td>
            <td class="p-3 text-muted" style="max-width:280px">
                <span class="d-block text-truncate" title="${DomUtils.escapeHtml(idea.text)}">
                    ${DomUtils.escapeHtml(idea.text)}
                </span>
            </td>
            <td class="p-3">${DomUtils.escapeHtml(idea.topic)}</td>
            <td class="p-3">${DomUtils.escapeHtml(idea.project)}</td>
            <td class="p-3">
                <span class="px-2 py-1 border rounded-1 small ${statusCfg.bg} ${statusCfg.text}">
                    ${DomUtils.escapeHtml(statusCfg.label)}
                </span>
            </td>
            <td class="p-3">
                <button type="button"
                        class="btn btn-sm btn-outline-secondary toggle-reactions-btn"
                        data-idea-id="${idea.id}"
                        ${reactionCount === 0 ? 'disabled' : ''}>
                    ${reactionCount === 0
            ? 'Geen reacties'
            : `${reactionCount} reactie${reactionCount === 1 ? '' : 's'} tonen`}
                </button>
            </td>
        `;

        tr.querySelector<HTMLButtonElement>('.toggle-reactions-btn')?.addEventListener('click', this.handleToggleReactions.bind(this));

        return tr;
    }

    private buildReactionsRow(idea: IdeaDto): HTMLTableRowElement {
        const tr = document.createElement('tr');
        tr.className = 'idea-row reactions-row';
        tr.id = `reactions-row-${idea.id}`;
        tr.style.display = 'none';

        const reactions = idea.reactions ?? [];

        const reactionItems = reactions.length === 0
            ? '<li class="text-muted small">Geen reacties.</li>'
            : reactions.map(r => {
                const parts: string[] = [];
                if (r.emoji) parts.push(`<span>${DomUtils.escapeHtml(r.emoji)}</span>`);
                if (r.text) parts.push(`<span>${DomUtils.escapeHtml(r.text)}</span>`);
                const statusCfg = this.getStatusConfig(r.status);
                return `
                    <li class="d-flex align-items-center gap-2 py-1 border-bottom">
                        <span class="flex-grow-1">${parts.join(' ')}</span>
                        <span class="badge ${statusCfg.bg} ${statusCfg.text} border small">
                            ${DomUtils.escapeHtml(statusCfg.label)}
                        </span>
                    </li>`;
            }).join('');

        tr.innerHTML = `
            <td colspan="6" class="p-0">
                <div class="px-4 py-2 bg-light border-bottom">
                    <p class="small fw-semibold text-muted mb-1">Reacties</p>
                    <ul class="list-unstyled mb-0">${reactionItems}</ul>
                </div>
            </td>
        `;

        return tr;
    }

    private handleToggleReactions(e: MouseEvent): void {
        const btn = e.currentTarget as HTMLButtonElement;
        const ideaId = btn.dataset.ideaId;
        if (!ideaId) return;

        const reactionsRow = document.getElementById(`reactions-row-${ideaId}`);
        if (!reactionsRow) return;

        const isHidden = reactionsRow.style.display === 'none';
        reactionsRow.style.display = isHidden ? '' : 'none';

        const idea = this.allIdeas.find(i => i.id === Number(ideaId));
        const count = idea?.reactions?.length ?? 0;
        btn.textContent = isHidden
            ? `${count} reactie${count === 1 ? '' : 's'} verbergen`
            : `${count} reactie${count === 1 ? '' : 's'} tonen`;
    }

    private updateSimilarityButtons(): void {
        document.querySelectorAll<HTMLButtonElement>('[data-similarity]').forEach(btn => {
            const isActive = btn.dataset.similarity === this.similarityFilter;
            btn.classList.toggle('btn-dark', isActive);
            btn.classList.toggle('btn-outline-secondary', !isActive);
        });
    }

    private updateSortButtons(): void {
        document.querySelectorAll<HTMLButtonElement>('[data-sort]').forEach(btn => {
            const isActive = btn.dataset.sort === this.sortOrder;
            btn.classList.toggle('btn-dark', isActive);
            btn.classList.toggle('btn-outline-secondary', !isActive);
        });
    }

    private toggleSimilarityGroup(visible: boolean): void {
        const group = document.getElementById('similarityFilterGroup');
        if (group) group.style.display = visible ? 'flex' : 'none';
    }

    private updateCounter(count: number): void {
        const counter = document.getElementById('ideasCount');
        if (counter) counter.textContent = `${count} idee${count === 1 ? '' : 'ën'}`;
    }

    private showLoading(show: boolean): void {
        const row = document.getElementById('ideasLoadingRow');
        if (row) row.style.display = show ? '' : 'none';
    }

    private showError(msg: string): void {
        const tbody = document.getElementById('ideasTableBody');
        if (!tbody) return;

        document.querySelectorAll<HTMLTableRowElement>('tr.idea-row').forEach(r => r.remove());

        const tr = document.createElement('tr');
        tr.className = 'idea-row';
        tr.innerHTML = `<td colspan="6" class="text-center text-danger p-4">${DomUtils.escapeHtml(msg)}</td>`;
        tbody.appendChild(tr);
    }

    private getStatusConfig(status: string): { bg: string; text: string; label: string } {
        const configs: Record<string, { bg: string; text: string; label: string }> = {
            Accepted: { bg: 'bg-success bg-opacity-10', text: 'text-success', label: 'Goedgekeurd' },
            InReview:  { bg: 'bg-warning bg-opacity-10', text: 'text-warning', label: 'In review'   },
            Rejected:  { bg: 'bg-danger  bg-opacity-10', text: 'text-danger',  label: 'Afgewezen'   },
        };
        return configs[status] ?? { bg: '', text: 'text-secondary', label: status };
    }
}

document.addEventListener('DOMContentLoaded', () => new SubAdminIdeas().init());