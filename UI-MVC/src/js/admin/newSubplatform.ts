type SubplatformPayload = {
    companyName: string;
    slug: string;
    adminEmail: string;
};

class SubplatformBuilder {
    private triggerBtn!: HTMLButtonElement;
    private modalEl!: HTMLElement;
    private successModalEl!: HTMLElement; 
    private form!: HTMLFormElement;

    private companyInput!: HTMLInputElement;
    private adminInput!: HTMLInputElement;

    private modal: any = null;
    private successModal: any = null;
    private readonly BACKDROP_ID = "modal-backdrop-fallback";

    init(): void {
        this.triggerBtn = document.getElementById("newSubplatform") as HTMLButtonElement;
        this.modalEl = document.getElementById("createSubplatformModal") as HTMLElement;
        this.successModalEl = document.getElementById("successModal") as HTMLElement;
        this.form = document.getElementById("createSubplatformForm") as HTMLFormElement;

        this.companyInput = document.getElementById("companyName") as HTMLInputElement;
        this.adminInput = document.getElementById("adminEmail") as HTMLInputElement;

        if (!this.triggerBtn || !this.modalEl || !this.successModalEl || !this.form) {
            console.error("Een of meerdere HTML elementen voor het subplatform ontbreken!");
            return;
        }

        this.initializeModal();
        this.bindEvents();
    }

    private initializeModal(): void {
        const bootstrapInstance = (window as any).bootstrap;

        if (bootstrapInstance?.Modal) {
            this.modal = new bootstrapInstance.Modal(this.modalEl);
            this.successModal = new bootstrapInstance.Modal(this.successModalEl);
        }

        this.modalEl.querySelectorAll("[data-bs-dismiss='modal']").forEach((el) => {
            el.addEventListener("click", () => this.hideModal());
        });

        document.getElementById("reloadPageBtn")?.addEventListener("click", () => {
            window.location.reload();
        });
    }

    private bindEvents(): void {
        this.triggerBtn.addEventListener("click", () => {
            this.resetForm();
            this.showModal();

            setTimeout(() => {
                this.companyInput.focus();
            }, 250);
        });

        this.form.addEventListener("submit", async (e) => {
            e.preventDefault();
            if (!this.validate()) return;
            await this.createSubplatform();
        });
    }

    private showModal(): void {
        if (this.modal) {
            this.modal.show();
            return;
        }

        this.modalEl.style.display = "block";
        setTimeout(() => this.modalEl.classList.add("show"), 10);
        this.addBackdrop();
    }

    private hideModal(): void {
        if (this.modal) {
            this.modal.hide();
            return;
        }

        this.modalEl.classList.remove("show");
        setTimeout(() => {
            this.modalEl.style.display = "none";
        }, 150); 
        
        this.removeBackdrop();
    }

    private showSuccessModal(): void {
        if (this.successModal) {
            this.successModal.show();
            return;
        }

        this.successModalEl.style.display = "block";
        setTimeout(() => this.successModalEl.classList.add("show"), 10);
        this.addBackdrop();
    }

    private addBackdrop(): void {
        if (!document.getElementById(this.BACKDROP_ID)) {
            const backdrop = document.createElement("div");
            backdrop.id = this.BACKDROP_ID;
            backdrop.className = "modal-backdrop fade show";
            document.body.appendChild(backdrop);
        }
        document.body.classList.add("modal-open");
        document.body.style.overflow = "hidden";
    }

    private removeBackdrop(): void {
        const backdrop = document.getElementById(this.BACKDROP_ID);
        if (backdrop) backdrop.remove();
        document.body.classList.remove("modal-open");
        document.body.style.overflow = "";
    }

    private resetForm(): void {
        this.form.reset();
        [this.companyInput, this.adminInput].forEach((input) => {
            input.classList.remove("is-invalid", "is-valid");
        });
    }

    private validate(): boolean {
        let valid = true;

        if (!this.companyInput.value.trim()) {
            this.companyInput.classList.add("is-invalid");
            valid = false;
        } else {
            this.companyInput.classList.remove("is-invalid");
            this.companyInput.classList.add("is-valid");
        }

        if (!this.isEmail(this.adminInput.value)) {
            this.adminInput.classList.add("is-invalid");
            valid = false;
        } else {
            this.adminInput.classList.remove("is-invalid");
            this.adminInput.classList.add("is-valid");
        }

        return valid;
    }

    private async createSubplatform(): Promise<void> {
        const payload: SubplatformPayload = {
            companyName: this.companyInput.value.trim(),
            slug: this.generateSlug(this.companyInput.value.trim()),
            adminEmail: this.adminInput.value.trim()
        };

        try {
            const token = (document.querySelector('input[name="__RequestVerificationToken"]') as HTMLInputElement)?.value;

            const response = await fetch("/Platform/CreateSubPlatform", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json",
                    "RequestVerificationToken": token || ""
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                let msg = "Onbekende fout";
                try {
                    msg = await response.text();
                } catch {}
                throw new Error(msg);
            }

            const data = await response.json();
            const password = data?.password;

            this.hideModal();

            setTimeout(() => {
                const emailSpan = document.getElementById("successEmail");
                const passwordSpan = document.getElementById("successPassword");

                if (emailSpan) emailSpan.textContent = payload.adminEmail;
                if (passwordSpan) passwordSpan.textContent = password || "Geen wachtwoord ontvangen van de server";

                this.showSuccessModal();
            }, 300);

        } catch (error) {
            console.error(error);
            alert("Fout bij aanmaken van subplatform: " + (error as Error).message);
        }
    }

    private generateSlug(name: string): string {
        return (name ?? "")
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .toLowerCase()
            .trim()
            .replace(/[^a-z0-9]+/g, "-")
            .replace(/(^-|-$)/g, "")
            .substring(0, 50);
    }

    private isEmail(value: string): boolean {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(value.trim());
    }
}

document.addEventListener("DOMContentLoaded", () => {
    console.log("newSubplatform script loaded");
    new SubplatformBuilder().init();
});