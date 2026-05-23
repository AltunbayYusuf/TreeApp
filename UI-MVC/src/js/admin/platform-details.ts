type TotalAiCostResponse = {
    totalCost: number;
};

class PlatformDetailsPage {
    private readonly costElement = document.getElementById("total-ai-cost");
    private readonly refreshIntervalMilliseconds = 5000;

    init(): void {
        if (!this.costElement) {
            return;
        }

        void this.loadTotalAiCost();

        window.setInterval(() => {
            void this.loadTotalAiCost();
        }, this.refreshIntervalMilliseconds);
    }

    private async loadTotalAiCost(): Promise<void> {
        const subPlatformId = this.getSubPlatformId();

        if (subPlatformId === null) {
            return;
        }

        try {
            const response = await fetch(`/api/aiusage/total-cost?subPlatformId=${subPlatformId}`);

            if (!response.ok) {
                return;
            }

            const data = await response.json() as TotalAiCostResponse;
            this.updateCost(data.totalCost);
        } catch {
            // dit is een automatische refresh op de achtergrond.
        }
    }

    private getSubPlatformId(): number | null {
        if (!(this.costElement instanceof HTMLElement)) {
            return null;
        }

        const subPlatformId = Number(this.costElement.dataset.subplatformId);

        return Number.isFinite(subPlatformId) && subPlatformId > 0
            ? subPlatformId
            : null;
    }

    private updateCost(totalCost: number): void {
        if (!(this.costElement instanceof HTMLElement)) {
            return;
        }

        this.costElement.textContent = this.formatEuro(totalCost);
    }

    private formatEuro(value: number): string {
        return new Intl.NumberFormat("nl-BE", {
            style: "currency",
            currency: "EUR",
            minimumFractionDigits: 4,
            maximumFractionDigits: 4
        }).format(value);
    }
}

new PlatformDetailsPage().init();