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
    userEmail: string | null;
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

        document.getElementById('projectFilter')?.addEventListener('change', this.handleProjectChange.bind(this));

        document.querySelectorAll<HTMLButtonElement>('[data-similarity]').forEach(btn => {
            btn.addEventListener('click', this.handleSimilarityClick.bind(this));
        });

        document.querySelectorAll<HTMLButtonElement>('[data-sort]').forEach(btn => {
            btn.addEventListener('click', this.handleSortClick.bind(this));
        });

        this.buildDetailModal();

        this.fetchAndRender(null);
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
            console.error('Fout bij ophalen ideeen:', error);
            this.showError('Kon ideeen niet ophalen. Probeer opnieuw.');
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
        tr.style.cursor = 'pointer';

        const statusCfg = this.getStatusConfig(idea.status);
        const reactionCount = idea.reactions?.length ?? 0;
        const hasEmail = !!idea.userEmail;

        tr.innerHTML = `
            <td class="p-3 fw-semibold" style="max-width:160px">
                <span class="d-block text-truncate">${DomUtils.escapeHtml(idea.title)}</span>
            </td>
            <td class="p-3 text-muted" style="max-width:260px">
                <span class="d-block text-truncate">${DomUtils.escapeHtml(idea.text)}</span>
            </td>
            <td class="p-3">${DomUtils.escapeHtml(idea.topic)}</td>
            <td class="p-3">${DomUtils.escapeHtml(idea.project)}</td>
            <td class="p-3">
                <span class="px-2 py-1 border rounded-1 small ${statusCfg.bg} ${statusCfg.text}">
                    ${DomUtils.escapeHtml(statusCfg.label)}
                </span>
            </td>
            <td class="p-3">
                ${hasEmail
            ? `<span class="small text-success" title="${DomUtils.escapeHtml(idea.userEmail!)}">
                           <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" class="me-1" viewBox="0 0 16 16">
                               <path d="M0 4a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2zm2-1a1 1 0 0 0-1 1v.217l7 4.2 7-4.2V4a1 1 0 0 0-1-1zm13 2.383-4.708 2.825L15 11.105zm-.034 6.876-5.64-3.471L8 9.583l-1.326-.795-5.64 3.47A1 1 0 0 0 2 13h12a1 1 0 0 0 .966-.741M1 11.105l4.708-2.897L1 5.383z"/>
                           </svg>${DomUtils.escapeHtml(idea.userEmail!)}</span>`
            : `<span class="small text-muted">Geen email</span>`}
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

        tr.addEventListener('click', (e) => {
            const target = e.target as HTMLElement;
            if (target.closest('.toggle-reactions-btn')) return;
            this.openDetailModal(idea);
        });

        tr.querySelector<HTMLButtonElement>('.toggle-reactions-btn')
            ?.addEventListener('click', this.handleToggleReactions.bind(this));

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
                if (r.text)  parts.push(`<span>${DomUtils.escapeHtml(r.text)}</span>`);
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
            <td colspan="7" class="p-0">
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
    

    private buildDetailModal(): void {
        if (document.getElementById('ideaDetailModal')) return;

        const el = document.createElement('div');
        el.innerHTML = `
            <div class="modal fade" id="ideaDetailModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="ideaDetailTitle"></h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Sluiten"></button>
                        </div>
                        <div class="modal-body" id="ideaDetailBody"></div>
                    </div>
                </div>
            </div>`;
        document.body.appendChild(el.firstElementChild!);
    }

    private openDetailModal(idea: IdeaDto): void {
        const title = document.getElementById('ideaDetailTitle');
        const body  = document.getElementById('ideaDetailBody');
        if (!title || !body) return;

        const statusCfg = this.getStatusConfig(idea.status);

        title.textContent = idea.title || 'Zonder titel';

        const emailRow = idea.userEmail
            ? `<tr>
                   <th class="text-muted fw-normal" style="width:140px">Email</th>
                   <td><a href="mailto:${DomUtils.escapeHtml(idea.userEmail)}">${DomUtils.escapeHtml(idea.userEmail)}</a></td>
               </tr>`
            : `<tr>
                   <th class="text-muted fw-normal" style="width:140px">Email</th>
                   <td class="text-muted">Niet opgegeven</td>
               </tr>`;

        const reactions = idea.reactions ?? [];
        const reactionsHtml = reactions.length === 0
            ? '<p class="text-muted small mb-0">Geen reacties.</p>'
            : `<ul class="list-unstyled mb-0">
                ${reactions.map(r => {
                const parts: string[] = [];
                if (r.emoji) parts.push(DomUtils.escapeHtml(r.emoji));
                if (r.text)  parts.push(DomUtils.escapeHtml(r.text));
                const cfg = this.getStatusConfig(r.status);
                return `<li class="d-flex align-items-start gap-2 py-2 border-bottom">
                                <span class="flex-grow-1">${parts.join(' ')}</span>
                                <span class="badge ${cfg.bg} ${cfg.text} border">${DomUtils.escapeHtml(cfg.label)}</span>
                            </li>`;
            }).join('')}
               </ul>`;

        body.innerHTML = `
            <table class="table table-borderless table-sm mb-4">
                <tbody>
                    <tr>
                        <th class="text-muted fw-normal" style="width:140px">Project</th>
                        <td>${DomUtils.escapeHtml(idea.project)}</td>
                    </tr>
                    <tr>
                        <th class="text-muted fw-normal">Topic</th>
                        <td>${DomUtils.escapeHtml(idea.topic)}</td>
                    </tr>
                    <tr>
                        <th class="text-muted fw-normal">Status</th>
                        <td><span class="px-2 py-1 border rounded-1 small ${statusCfg.bg} ${statusCfg.text}">${DomUtils.escapeHtml(statusCfg.label)}</span></td>
                    </tr>
                    ${emailRow}
                </tbody>
            </table>

            <h6 class="fw-semibold mb-2">Inhoud</h6>
            <p class="mb-4" style="white-space:pre-wrap">${DomUtils.escapeHtml(idea.text)}</p>

            <h6 class="fw-semibold mb-2">Reacties <span class="text-muted fw-normal">(${reactions.length})</span></h6>
            ${reactionsHtml}
        `;

        const modalEl = document.getElementById('ideaDetailModal')!;
        const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modal.show();
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
        if (counter) counter.textContent = `${count} idee${count === 1 ? '' : 'en'}`;
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
        tr.innerHTML = `<td colspan="7" class="text-center text-danger p-4">${DomUtils.escapeHtml(msg)}</td>`;
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