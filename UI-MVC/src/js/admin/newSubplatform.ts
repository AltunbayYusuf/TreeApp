type SubplatformPayload = {
    companyName: string;
    slug: string;
    adminEmail: string;
};

type CreateSubplatformResponse = {
    password?: string;
};

type BootstrapModalConstructor = {
    new (element: HTMLElement): BootstrapModal;
};

type BootstrapModal = {
    show: () => void;
    hide: () => void;
};

type BootstrapWindow = Window & {
    bootstrap?: {
        Modal?: BootstrapModalConstructor;
    };
};

class SubplatformBuilder {
    private readonly backdropId = "modal-backdrop-fallback";

    private triggerButton: HTMLButtonElement | null = null;
    private modalElement: HTMLElement | null = null;
    private successModalElement: HTMLElement | null = null;
    private form: HTMLFormElement | null = null;
    private companyInput: HTMLInputElement | null = null;
    private adminInput: HTMLInputElement | null = null;
    private submitButton: HTMLButtonElement | null = null;

    private modal: BootstrapModal | null = null;
    private successModal: BootstrapModal | null = null;

    init(): void {
        this.readElements();

        if (!this.hasRequiredElements()) {
            return;
        }

        this.initializeModal();
        this.bindEvents();
    }

    private readElements(): void {
        this.triggerButton = document.getElementById("newSubplatform") as HTMLButtonElement | null;
        this.modalElement = document.getElementById("createSubplatformModal");
        this.successModalElement = document.getElementById("successModal");
        this.form = document.getElementById("createSubplatformForm") as HTMLFormElement | null;
        this.companyInput = document.getElementById("companyName") as HTMLInputElement | null;
        this.adminInput = document.getElementById("adminEmail") as HTMLInputElement | null;
        this.submitButton = document.getElementById("submitCreateSubplatform") as HTMLButtonElement | null;
    }

    private hasRequiredElements(): boolean {
        return Boolean(
            this.triggerButton &&
            this.modalElement &&
            this.successModalElement &&
            this.form &&
            this.companyInput &&
            this.adminInput
        );
    }

    private initializeModal(): void {
        if (!this.modalElement || !this.successModalElement) {
            return;
        }

        const bootstrapModal = (window as BootstrapWindow).bootstrap?.Modal;

        if (bootstrapModal) {
            this.modal = new bootstrapModal(this.modalElement);
            this.successModal = new bootstrapModal(this.successModalElement);
        }

        this.modalElement
            .querySelectorAll("[data-bs-dismiss='modal']")
            .forEach((element) => {
                element.addEventListener("click", () => this.hideModal());
            });

        document.getElementById("reloadPageBtn")?.addEventListener("click", () => {
            window.location.reload();
        });
    }

    private bindEvents(): void {
        this.triggerButton?.addEventListener("click", () => {
            this.resetForm();
            this.showModal();

            window.setTimeout(() => {
                this.companyInput?.focus();
            }, 250);
        });

        this.form?.addEventListener("submit", async (event) => {
            event.preventDefault();

            if (!this.validate()) {
                return;
            }

            await this.createSubplatform();
        });
    }

    private showModal(): void {
        if (!this.modalElement) {
            return;
        }

        if (this.modal) {
            this.modal.show();
            return;
        }

        this.modalElement.style.display = "block";

        window.setTimeout(() => {
            this.modalElement?.classList.add("show");
        }, 10);

        this.addBackdrop();
    }

    private hideModal(): void {
        if (!this.modalElement) {
            return;
        }

        if (this.modal) {
            this.modal.hide();
            return;
        }

        this.modalElement.classList.remove("show");

        window.setTimeout(() => {
            if (this.modalElement) {
                this.modalElement.style.display = "none";
            }
        }, 150);

        this.removeBackdrop();
    }

    private showSuccessModal(): void {
        if (!this.successModalElement) {
            return;
        }

        if (this.successModal) {
            this.successModal.show();
            return;
        }

        this.successModalElement.style.display = "block";

        window.setTimeout(() => {
            this.successModalElement?.classList.add("show");
        }, 10);

        this.addBackdrop();
    }

    private addBackdrop(): void {
        if (!document.getElementById(this.backdropId)) {
            const backdrop = document.createElement("div");
            backdrop.id = this.backdropId;
            backdrop.className = "modal-backdrop fade show";
            document.body.appendChild(backdrop);
        }

        document.body.classList.add("modal-open");
        document.body.style.overflow = "hidden";
    }

    private removeBackdrop(): void {
        document.getElementById(this.backdropId)?.remove();
        document.body.classList.remove("modal-open");
        document.body.style.overflow = "";
    }

    private resetForm(): void {
        this.form?.reset();

        [this.companyInput, this.adminInput].forEach((input) => {
            input?.classList.remove("is-invalid", "is-valid");
        });

        this.setSubmitLoading(false);
    }

    private validate(): boolean {
        const isCompanyNameValid = this.validateCompanyName();
        const isAdminEmailValid = this.validateAdminEmail();

        return isCompanyNameValid && isAdminEmailValid;
    }

    private validateCompanyName(): boolean {
        if (!this.companyInput) {
            return false;
        }

        const isValid = this.companyInput.value.trim().length > 0 &&
            this.companyInput.value.trim().length <= 50;

        this.setInputValidationState(this.companyInput, isValid);

        return isValid;
    }

    private validateAdminEmail(): boolean {
        if (!this.adminInput) {
            return false;
        }

        const isValid = this.isEmail(this.adminInput.value);

        this.setInputValidationState(this.adminInput, isValid);

        return isValid;
    }

    private setInputValidationState(input: HTMLInputElement, isValid: boolean): void {
        input.classList.toggle("is-valid", isValid);
        input.classList.toggle("is-invalid", !isValid);
    }

    private async createSubplatform(): Promise<void> {
        if (!this.companyInput || !this.adminInput) {
            return;
        }

        const companyName = this.companyInput.value.trim();

        const payload: SubplatformPayload = {
            companyName,
            slug: this.generateSlug(companyName),
            adminEmail: this.adminInput.value.trim()
        };

        this.setSubmitLoading(true);

        try {
            const response = await fetch("/Platform/CreateSubPlatform", {
                method: "POST",
                headers: this.createRequestHeaders(),
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                const message = await this.readErrorMessage(response);

                window.alert(`Fout bij aanmaken van subplatform: ${message}`);
                return;
            }

            const data = await response.json() as CreateSubplatformResponse;

            this.hideModal();

            window.setTimeout(() => {
                this.showSuccess(payload.adminEmail, data.password);
            }, 300);
        } catch {
            window.alert("Fout bij aanmaken van subplatform.");
        } finally {
            this.setSubmitLoading(false);
        }
    }

    private createRequestHeaders(): HeadersInit {
        return {
            "Content-Type": "application/json",
            "Accept": "application/json",
            "RequestVerificationToken": this.getAntiForgeryToken()
        };
    }

    private getAntiForgeryToken(): string {
        const tokenInput = document.querySelector<HTMLInputElement>('input[name="__RequestVerificationToken"]');

        return tokenInput?.value ?? "";
    }

    private async readErrorMessage(response: Response): Promise<string> {
        const message = await response.text();

        return message.trim().length > 0
            ? message
            : "Onbekende fout";
    }

    private showSuccess(email: string, password?: string): void {
        const emailElement = document.getElementById("successEmail");
        const passwordElement = document.getElementById("successPassword");

        if (emailElement) {
            emailElement.textContent = email;
        }

        if (passwordElement) {
            passwordElement.textContent = password && password.trim().length > 0
                ? password
                : "Geen wachtwoord ontvangen van de server";
        }

        this.showSuccessModal();
    }

    private setSubmitLoading(isLoading: boolean): void {
        if (!this.submitButton) {
            return;
        }

        this.submitButton.disabled = isLoading;
        this.submitButton.textContent = isLoading ? "Bezig..." : "Aanmaken";
    }

    private generateSlug(name: string): string {
        return name
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .toLowerCase()
            .trim()
            .replace(/[^a-z0-9]+/g, "-")
            .replace(/(^-|-$)/g, "")
            .substring(0, 50);
    }

    private isEmail(value: string): boolean {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim());
    }
    
}

document.addEventListener("DOMContentLoaded", () => {
    new SubplatformBuilder().init();
});