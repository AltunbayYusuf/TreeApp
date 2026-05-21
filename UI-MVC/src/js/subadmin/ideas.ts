// subadmin/ideas.ts
import {DomUtils} from '../helpers/utils';

type SortOrder = 'newest' | 'oldest';
type SimilarityFilter = 'all' | 'similar' | 'different' | 'broad';

interface ReactionDto {
    id: number;
    text: string;
    emoji: string;
    status: string;
}

interface EmojiReactionSummary {
    emoji: string;
    count: number;
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

interface IdeaSelectionGroupDto {
    title: string;
    reason: string;
    ideaIds: number[];
}

interface IdeaSelectionDto {
    mode: SimilarityFilter;
    groups: IdeaSelectionGroupDto[];
}

export class SubAdminIdeas {
    private subplatformId: number = 0;
    private allIdeas: IdeaDto[] = [];
    private sortOrder: SortOrder = 'newest';
    private similarityFilter: SimilarityFilter = 'all';
    private keywordFilter: string = '';
    private emailSort: 'none' | 'hasEmail' | 'noEmail' = 'none';
    private statusSort: 'none' | 'Accepted' | 'InReview' | 'Rejected' = 'none';
    private ideaToDelete: IdeaDto | null = null;
    private reactionToDelete: { ideaId: number; reactionId: number } | null = null;

    private selectedProjectId: number | null = null;
    private aiSelection: IdeaSelectionDto | null = null;

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

        const keywordInput = document.getElementById('keywordFilter') as HTMLInputElement;
        if (keywordInput) {
            keywordInput.addEventListener('input', this.handleKeywordChange.bind(this));
        }

        const clearBtn = document.getElementById('clearKeywordBtn');
        if (clearBtn) {
            clearBtn.addEventListener('click', () => {
                if (keywordInput) {
                    keywordInput.value = '';
                    this.keywordFilter = '';
                    this.renderRows();
                }
            });
        }

        document.getElementById('thStatus')?.addEventListener('click', this.handleStatusSortClick.bind(this));
        document.getElementById('thEmail')?.addEventListener('click', this.handleEmailSortClick.bind(this));

        this.buildDetailModal();
        this.bindDeleteModal();
        this.bindDeleteReactionModal();

        this.fetchAndRender(null);
    }

    private handleEmailSortClick(): void {
        this.statusSort = 'none';
        if (this.emailSort === 'none' || this.emailSort === 'noEmail') {
            this.emailSort = 'hasEmail';
        } else {
            this.emailSort = 'noEmail';
        }
        this.updateSortIcons();
        this.renderRows();
    }

    private handleStatusSortClick(): void {
        this.emailSort = 'none';
        if (this.statusSort === 'none' || this.statusSort === 'Rejected') {
            this.statusSort = 'Accepted';
        } else if (this.statusSort === 'Accepted') {
            this.statusSort = 'InReview';
        } else if (this.statusSort === 'InReview') {
            this.statusSort = 'Rejected';
        }
        this.updateSortIcons();
        this.renderRows();
    }

    private updateSortIcons(): void {
        const iconEmail = document.getElementById('iconEmail');
        if (iconEmail) {
            iconEmail.className = this.emailSort === 'none' ? 'text-body-secondary small d-flex align-items-center' : 'text-primary small d-flex align-items-center';
        }
        const iconStatus = document.getElementById('iconStatus');
        if (iconStatus) {
            iconStatus.className = this.statusSort === 'none' ? 'text-body-secondary small d-flex align-items-center' : 'text-primary small d-flex align-items-center';
        }
    }

    private handleProjectChange(e: Event): void {
        const select = e.currentTarget as HTMLSelectElement;
        const projectId = select.value ? Number(select.value) : null;

        this.selectedProjectId = projectId;
        this.similarityFilter = 'all';
        this.aiSelection = null;

        this.updateSimilarityButtons();
        this.toggleSimilarityGroup(projectId !== null);

        this.fetchAndRender(projectId);
    }

    private async handleSimilarityClick(e: MouseEvent): Promise<void> {
        const btn = e.currentTarget as HTMLButtonElement;
        const mode = (btn.dataset.similarity ?? 'all') as SimilarityFilter;

        this.similarityFilter = mode;
        this.updateSimilarityButtons();

        if (mode === 'all') {
            this.aiSelection = null;
            this.renderRows();
            return;
        }

        if (!this.selectedProjectId) {
            this.showError('Kies eerst een project.');
            return;
        }

        await this.fetchAiSelection(this.selectedProjectId, mode);
    }

    private handleSortClick(e: MouseEvent): void {
        const btn = e.currentTarget as HTMLButtonElement;
        this.sortOrder = (btn.dataset.sort ?? 'newest') as SortOrder;
        this.updateSortButtons();
        this.renderRows();
    }

    private handleKeywordChange(e: Event): void {
        const input = e.currentTarget as HTMLInputElement;
        this.keywordFilter = input.value.trim().toLowerCase();
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

    private async fetchAiSelection(projectId: number, mode: SimilarityFilter): Promise<void> {
        this.showLoading(true);

        try {
            const response = await fetch('/api/subadmin-projects/idea-selection', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': DomUtils.getAntiForgeryToken()
                },
                body: JSON.stringify({
                    projectId,
                    selectionMode: mode
                })
            });

            if (!response.ok) throw new Error(`HTTP ${response.status}`);

            const data = await response.json();

            if (!data.ok) {
                this.showError(data.message ?? 'AI-selectie kon niet opgehaald worden.');
                return;
            }

            this.aiSelection = data.selection as IdeaSelectionDto;
            this.renderRows();
        } catch (error) {
            console.error('Fout bij AI-selectie:', error);
            this.showError('Kon AI-selectie niet ophalen.');
        } finally {
            this.showLoading(false);
        }
    }

    private getFilteredAndSortedIdeas(): IdeaDto[] {
        let ideas = this.allIdeas;

        if (this.keywordFilter) {
            ideas = ideas.filter(i =>
                (i.title && i.title.toLowerCase().includes(this.keywordFilter)) ||
                (i.text && i.text.toLowerCase().includes(this.keywordFilter)) ||
                (i.topic && i.topic.toLowerCase().includes(this.keywordFilter)) ||
                (i.project && i.project.toLowerCase().includes(this.keywordFilter))
            );
        }

        if (this.sortOrder === 'oldest') {
            ideas = [...ideas].reverse();
        } else {
            ideas = [...ideas];
        }

        if (this.emailSort === 'hasEmail') {
            ideas.sort((a, b) => {
                const aHas = a.userEmail && a.userEmail.trim() !== '' ? 1 : 0;
                const bHas = b.userEmail && b.userEmail.trim() !== '' ? 1 : 0;
                return bHas - aHas;
            });
        } else if (this.emailSort === 'noEmail') {
            ideas.sort((a, b) => {
                const aHas = a.userEmail && a.userEmail.trim() !== '' ? 1 : 0;
                const bHas = b.userEmail && b.userEmail.trim() !== '' ? 1 : 0;
                return aHas - bHas;
            });
        }

        if (this.statusSort !== 'none') {
            ideas.sort((a, b) => {
                const aMatch = a.status === this.statusSort ? 1 : 0;
                const bMatch = b.status === this.statusSort ? 1 : 0;
                return bMatch - aMatch;
            });
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

        if (this.similarityFilter !== 'all' && this.aiSelection?.groups?.length) {
            this.renderAiGroups(tbody, ideas);
        } else {
            ideas.forEach(idea => {
                tbody.appendChild(this.buildIdeaRow(idea));
                tbody.appendChild(this.buildReactionsRow(idea));
            });
        }

        this.attachReactionDeleteHandlers();
    }

    private renderAiGroups(tbody: HTMLElement, ideas: IdeaDto[]): void {
        const renderedIdeaIds = new Set<number>();

        this.aiSelection?.groups.forEach(group => {
            const groupIdeas = group.ideaIds
                .map(id => ideas.find(i => i.id === id))
                .filter((idea): idea is IdeaDto => idea !== undefined);

            if (groupIdeas.length === 0) return;

            const header = document.createElement('tr');
            header.className = 'idea-row bg-body-tertiary border-bottom border-secondary';
            header.innerHTML = `
            <td colspan="8" class="p-3 bg-transparent">
                <div class="fw-bold text-body-emphasis">${DomUtils.escapeHtml(group.title)}</div>
                <div class="text-body-secondary small">${DomUtils.escapeHtml(group.reason)}</div>
            </td>
        `;
            tbody.appendChild(header);

            groupIdeas.forEach(idea => {
                renderedIdeaIds.add(idea.id);
                tbody.appendChild(this.buildIdeaRow(idea));
                tbody.appendChild(this.buildReactionsRow(idea));
            });
        });

        const remainingIdeas = ideas.filter(i => !renderedIdeaIds.has(i.id));

        remainingIdeas.forEach(idea => {
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
        const email = idea.userEmail?.trim() ?? '';
        const hasEmail = email !== '';

        tr.innerHTML = `
            <td class="p-3 fw-semibold text-body-emphasis bg-transparent" style="max-width:160px">
                <span class="d-block text-truncate">${DomUtils.escapeHtml(idea.title)}</span>
            </td>
            <td class="p-3 text-body-secondary bg-transparent d-none d-md-table-cell" style="max-width:260px">
                <span class="d-block text-truncate">${DomUtils.escapeHtml(idea.text)}</span>
            </td>
            <td class="p-3 text-body-emphasis bg-transparent d-none d-md-table-cell">${DomUtils.escapeHtml(idea.topic)}</td>
            <td class="p-3 text-body-emphasis bg-transparent">${DomUtils.escapeHtml(idea.project)}</td>
            <td class="p-3 bg-transparent">
                <span class="px-2 py-1 border border-secondary rounded-1 small ${statusCfg.bg} ${statusCfg.text}">
                    ${DomUtils.escapeHtml(statusCfg.label)}
                </span>
            </td>
            <td class="p-3 bg-transparent d-none d-md-table-cell">
                ${hasEmail
            ? `<span class="small text-success">✔️</span>`
            : `<span class="small text-body-secondary">❌</span>`}
            </td>
            <td class="p-3 bg-transparent d-none d-md-table-cell">
                <button type="button"
                        class="btn btn-sm btn-outline-secondary toggle-reactions-btn text-body-emphasis"
                        data-idea-id="${idea.id}"
                        ${reactionCount === 0 ? 'disabled' : ''}>
                    ${reactionCount === 0
            ? 'Geen reacties'
            : `${reactionCount} reactie${reactionCount === 1 ? '' : 's'} tonen`}
                </button>
            </td>
            <td class="p-3 text-end bg-transparent d-none d-md-table-cell">
                <button type="button" class="btn btn-sm btn-outline-danger delete-idea-btn" data-idea-id="${idea.id}">🗑️</button>
            </td>
        `;

        tr.addEventListener('click', (e) => {
            const target = e.target as HTMLElement;
            if (target.closest('.toggle-reactions-btn, .delete-idea-btn')) return;
            this.openDetailModal(idea);
        });

        tr.querySelector<HTMLButtonElement>('.toggle-reactions-btn')
            ?.addEventListener('click', this.handleToggleReactions.bind(this));
        tr.querySelector<HTMLButtonElement>('.delete-idea-btn')
            ?.addEventListener('click', (e) => this.openDeleteModal(e, idea));

        return tr;
    }

    private buildReactionsRow(idea: IdeaDto): HTMLTableRowElement {
        const tr = document.createElement('tr');
        tr.className = 'idea-row reactions-row';
        tr.id = `reactions-row-${idea.id}`;
        tr.style.display = 'none';

        const reactions = idea.reactions ?? [];
        const emojiSummary = this.getEmojiReactionSummary(reactions);
        const emojiSummaryMarkup = emojiSummary.length === 0
            ? ''
            : `
                <div class="d-flex flex-wrap gap-2 mb-2">
                    ${emojiSummary.map(item => `
                        <span class="badge rounded-pill bg-transparent text-body-emphasis border border-secondary">
                            <span>${DomUtils.escapeHtml(item.emoji)}</span>
                            <span class="ms-1">${item.count}</span>
                        </span>
                    `).join('')}
                </div>`;

        const textReactions = reactions.filter(reaction => reaction.text);

        const reactionItems = textReactions.length === 0
            ? '<li class="text-body-secondary small">Geen reacties.</li>'
            : textReactions.map(r => {
                const parts: string[] = [];
                if (r.emoji) parts.push(`<span>${DomUtils.escapeHtml(r.emoji)}</span>`);
                if (r.text) parts.push(`<span>${DomUtils.escapeHtml(r.text)}</span>`);
                const statusCfg = this.getStatusConfig(r.status);
                return `
                    <li class="d-flex align-items-center gap-2 py-1 border-bottom border-secondary text-body-emphasis">
                        <span class="flex-grow-1">${parts.join(' ')}</span>
                        <span class="badge ${statusCfg.bg} ${statusCfg.text} border border-secondary small">
                            ${DomUtils.escapeHtml(statusCfg.label)}
                        </span>
                        <button type="button" class="btn btn-sm btn-outline-danger delete-reaction-btn" data-idea-id="${idea.id}" data-reaction-id="${r.id}">🗑️</button>
                    </li>`;
            }).join('');

        tr.innerHTML = `
            <td colspan="8" class="p-0 bg-transparent">
                <div class="px-4 py-2 bg-body-tertiary border-bottom border-secondary">
                    <p class="small fw-semibold text-body-secondary mb-1">Reacties</p>
                    ${emojiSummaryMarkup}
                    <ul class="list-unstyled mb-0">${reactionItems}</ul>
                </div>
            </td>
        `;

        return tr;
    }

    private getEmojiReactionSummary(reactions: ReactionDto[]): EmojiReactionSummary[] {
        const counts = new Map<string, number>();

        reactions
            .filter(reaction => reaction.emoji && !reaction.text)
            .forEach(reaction => {
                counts.set(reaction.emoji, (counts.get(reaction.emoji) ?? 0) + 1);
            });

        return Array.from(counts.entries())
            .map(([emoji, count]) => ({emoji, count}))
            .sort((a, b) => b.count - a.count || a.emoji.localeCompare(b.emoji));
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

    private bindDeleteModal(): void {
        const modal = document.getElementById('deleteIdeaModal');
        if (!modal) return;

        modal.addEventListener('click', (e) => {
            if (e.target === modal) this.closeDeleteModal();
        });
        document.getElementById('cancelDeleteIdeaBtn')?.addEventListener('click', () => this.closeDeleteModal());
        modal.querySelector('.btn-close')?.addEventListener('click', () => this.closeDeleteModal());
        document.getElementById('confirmDeleteIdeaBtn')?.addEventListener('click', this.handleDeleteIdea.bind(this));
    }

    private bindDeleteReactionModal(): void {
        const modal = document.getElementById('deleteReactionModal');
        if (!modal) return;

        modal.addEventListener('click', (e) => {
            if (e.target === modal) this.closeDeleteReactionModal();
        });
        document.getElementById('cancelDeleteReactionBtn')?.addEventListener('click', () => this.closeDeleteReactionModal());
        modal.querySelector('.btn-close')?.addEventListener('click', () => this.closeDeleteReactionModal());
        document.getElementById('confirmDeleteReactionBtn')?.addEventListener('click', this.handleDeleteReaction.bind(this));
    }

    private openDeleteModal(e: MouseEvent, idea: IdeaDto): void {
        e.preventDefault();
        this.ideaToDelete = idea;

        const title = document.getElementById('deleteIdeaTitle');
        if (title) title.textContent = idea.title || 'Zonder titel';

        DomUtils.openModal('deleteIdeaModal');
    }

    private closeDeleteModal(): void {
        DomUtils.closeModal('deleteIdeaModal');
        this.ideaToDelete = null;
    }

    private openDeleteReactionModal(ideaId: number, reactionId: number): void {
        this.reactionToDelete = {ideaId, reactionId};
        DomUtils.openModal('deleteReactionModal');
    }

    private closeDeleteReactionModal(): void {
        DomUtils.closeModal('deleteReactionModal');
        this.reactionToDelete = null;
    }

    private async handleDeleteIdea(): Promise<void> {
        const ideaId = this.ideaToDelete?.id;
        if (!ideaId) return;

        const response = await fetch(`/api/ideas/${ideaId}`, {method: 'DELETE'});
        if (!response.ok) {
            this.showError('Kon idee niet verwijderen.');
            return;
        }

        this.allIdeas = this.allIdeas.filter(i => i.id !== ideaId);
        this.closeDeleteModal();
        this.renderRows();
    }

    private async handleDeleteReaction(): Promise<void> {
        const reactionId = this.reactionToDelete?.reactionId;
        const ideaId = this.reactionToDelete?.ideaId;
        if (!reactionId || !ideaId) return;

        const response = await fetch(`/api/reactions/${reactionId}`, {method: 'DELETE'});
        if (!response.ok) {
            this.showError('Kon reactie niet verwijderen.');
            return;
        }

        const idea = this.allIdeas.find(i => i.id === ideaId);
        if (idea) {
            idea.reactions = idea.reactions.filter(r => r.id !== reactionId);
        }

        this.closeDeleteReactionModal();
        this.renderRows();
    }

    private attachReactionDeleteHandlers(): void {
        document.querySelectorAll<HTMLButtonElement>('.delete-reaction-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                const reactionId = Number(btn.dataset.reactionId);
                const ideaId = Number(btn.dataset.ideaId);
                if (!reactionId || !ideaId) return;
                this.openDeleteReactionModal(ideaId, reactionId);
            });
        });
    }

    private buildDetailModal(): void {
        if (document.getElementById('ideaDetailModal')) return;

        const el = document.createElement('div');
        el.innerHTML = `
            <div class="modal fade" id="ideaDetailModal" tabindex="-1" aria-hidden="true" style="display:none">
                <div class="modal-dialog modal-lg modal-fullscreen-md-down modal-dialog-centered modal-dialog-scrollable">
                    <div class="modal-content border-secondary" style="background-color: #0f172a; color: #f8fafc;">
                        <div class="modal-header border-bottom border-secondary pb-3">
                            <h5 class="modal-title fw-bold text-white" id="ideaDetailTitle"></h5>
                            <button type="button" class="btn-close btn-close-white" aria-label="Sluiten"></button>
                        </div>
                        <div class="modal-body" id="ideaDetailBody"></div>
                    </div>
                </div>
            </div>`;
        const modal = el.firstElementChild as HTMLElement;
        modal.addEventListener('click', (e) => {
            if (e.target === modal) this.closeDetailModal();
        });
        modal.querySelector('.btn-close')?.addEventListener('click', () => this.closeDetailModal());
        document.body.appendChild(modal);
    }

    private openDetailModal(idea: IdeaDto): void {
        const title = document.getElementById('ideaDetailTitle');
        const body = document.getElementById('ideaDetailBody');
        if (!title || !body) return;

        const statusCfg = this.getStatusConfig(idea.status);
        const email = idea.userEmail?.trim() ?? '';

        title.textContent = idea.title || 'Zonder titel';

        // bg-transparent toegevoegd aan BEIDE kanten van de email-rij!
        const emailRow = email
            ? `<tr>
                   <th class="text-body-secondary fw-normal bg-transparent" style="width:140px">Email</th>
                   <td class="bg-transparent"><a href="mailto:${DomUtils.escapeHtml(email)}" class="text-info">${DomUtils.escapeHtml(email)}</a></td>
               </tr>`
            : `<tr>
                   <th class="text-body-secondary fw-normal bg-transparent" style="width:140px">Email</th>
                   <td class="text-body-secondary bg-transparent">Niet opgegeven</td>
               </tr>`;

        body.innerHTML = `
            <div class="card shadow-sm bg-transparent border border-secondary mb-4">
                <div class="card-body">
                    <table class="table table-borderless table-sm mb-0">
                        <tbody>
                            <tr>
                                <th class="text-body-secondary fw-normal bg-transparent" style="width:140px">Project</th>
                                <td class="fw-semibold text-white bg-transparent">${DomUtils.escapeHtml(idea.project)}</td>
                            </tr>
                            <tr>
                                <th class="text-body-secondary fw-normal bg-transparent">Topic</th>
                                <td class="fw-semibold text-white bg-transparent">${DomUtils.escapeHtml(idea.topic)}</td>
                            </tr>
                            <tr>
                                <th class="text-body-secondary fw-normal bg-transparent">Status</th>
                                <td class="bg-transparent"><span class="px-2 py-1 border border-secondary rounded-1 small ${statusCfg.bg} ${statusCfg.text} fw-semibold">${DomUtils.escapeHtml(statusCfg.label)}</span></td>
                            </tr>
                            ${emailRow}
                        </tbody>
                    </table>
                </div>
            </div>

            <div class="card shadow-sm bg-transparent border border-secondary">
                <div class="card-body">
                    <h6 class="fw-bold mb-3 text-body-secondary text-uppercase" style="font-size: 0.85rem; letter-spacing: 0.5px;">Inhoud</h6>
                    <p class="mb-0 text-white" style="white-space:pre-wrap; font-size: 1.05rem;">${DomUtils.escapeHtml(idea.text)}</p>
                </div>
            </div>

            <div class="mt-4 text-end">
                <button type="button" class="btn btn-outline-danger delete-from-modal-btn">Idee verwijderen 🗑️</button>
            </div>
        `;

        const modalEl = document.getElementById('ideaDetailModal');
        if (!modalEl) return;

        modalEl.querySelector('.delete-from-modal-btn')?.addEventListener('click', (e) => {
            this.closeDetailModal();
            this.openDeleteModal(e as MouseEvent, idea);
        });

        DomUtils.openModal('ideaDetailModal');
    }

    private closeDetailModal(): void {
        DomUtils.closeModal('ideaDetailModal');
    }


    private updateSimilarityButtons(): void {
        document.querySelectorAll<HTMLButtonElement>('[data-similarity]').forEach(btn => {
            const isActive = btn.dataset.similarity === this.similarityFilter;
            btn.classList.toggle('btn-dark', isActive);
            btn.classList.toggle('btn-outline-secondary', !isActive);
            if (!isActive) {
                btn.classList.add('text-body-emphasis');
            } else {
                btn.classList.remove('text-body-emphasis');
            }
        });
    }

    private updateSortButtons(): void {
        document.querySelectorAll<HTMLButtonElement>('[data-sort]').forEach(btn => {
            const isActive = btn.dataset.sort === this.sortOrder;
            btn.classList.toggle('btn-dark', isActive);
            btn.classList.toggle('btn-outline-secondary', !isActive);
            if (!isActive) {
                btn.classList.add('text-body-emphasis');
            } else {
                btn.classList.remove('text-body-emphasis');
            }
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
        tr.innerHTML = `<td colspan="8" class="text-center bg-transparent text-danger p-4">${DomUtils.escapeHtml(msg)}</td>`;
        tbody.appendChild(tr);
    }

    private getStatusConfig(status: string): { bg: string; text: string; label: string } {
        const configs: Record<string, { bg: string; text: string; label: string }> = {
            Accepted: {bg: 'bg-success bg-opacity-10', text: 'text-success', label: 'Goedgekeurd'},
            InReview: {bg: 'bg-warning bg-opacity-10', text: 'text-warning', label: 'In review'},
            Rejected: {bg: 'bg-danger  bg-opacity-10', text: 'text-danger', label: 'Afgewezen'},
        };
        return configs[status] ?? {bg: '', text: 'text-body-secondary', label: status};
    }
}

document.addEventListener('DOMContentLoaded', () => new SubAdminIdeas().init());