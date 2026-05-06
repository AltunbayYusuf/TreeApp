type SubplatformPayload = {
    companyName: string;
    slug: string;
    adminEmail: string;
};

class SubplatformBuilder {
    private triggerBtn!: HTMLButtonElement;
    private modalEl!: HTMLElement;
    private form!: HTMLFormElement;

    private companyInput!: HTMLInputElement;
    private adminInput!: HTMLInputElement;

    private modal: any = null;
    private readonly BACKDROP_ID = "modal-backdrop-fallback";

    init(): void {
        this.triggerBtn = document.getElementById("newSubplatform") as HTMLButtonElement;
        this.modalEl = document.getElementById("createSubplatformModal") as HTMLElement;
        this.form = document.getElementById("createSubplatformForm") as HTMLFormElement;

        this.companyInput = document.getElementById("companyName") as HTMLInputElement;
        this.adminInput = document.getElementById("adminEmail") as HTMLInputElement;

        if (!this.triggerBtn || !this.modalEl || !this.form || !this.companyInput || !this.adminInput) {
            console.error("Subplatform elements niet gevonden");
            return;
        }

        this.initializeModal();
        this.bindEvents();
    }

    private initializeModal(): void {
        const bootstrapInstance = (window as any).bootstrap;

        if (bootstrapInstance?.Modal) {
            this.modal = new bootstrapInstance.Modal(this.modalEl);
        }

        this.modalEl
            .querySelectorAll("[data-bs-dismiss='modal']")
            .forEach((el) => {
                el.addEventListener("click", () => this.hideModal());
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
        this.modalEl.classList.add("show");
        this.modalEl.removeAttribute("aria-hidden");

        if (!document.getElementById(this.BACKDROP_ID)) {
            const backdrop = document.createElement("div");
            backdrop.id = this.BACKDROP_ID;
            backdrop.className = "modal-backdrop fade show";
            document.body.appendChild(backdrop);
        }

        document.body.classList.add("modal-open");
        document.body.style.overflow = "hidden";
    }

    private hideModal(): void {
        if (this.modal) {
            this.modal.hide();
            return;
        }

        this.modalEl.style.display = "none";
        this.modalEl.classList.remove("show");
        this.modalEl.setAttribute("aria-hidden", "true");

        const backdrop = document.getElementById(this.BACKDROP_ID);

        if (backdrop) {
            backdrop.remove();
        }

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
                    "RequestVerificationToken": token 
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                let msg = "Onbekende fout";
                try {
                    msg = await response.text();
                } catch {
                }
                throw new Error(msg);
            }

            const data = await response.json();
            const password = data?.password;

            this.hideModal();

            if (password) {
                alert(
                    `Subplatform succesvol aangemaakt!\n\n` +
                    `Het wachtwoord voor ${payload.adminEmail} is:\n\n${password}\n\n` +
                    `Kopieer dit, je ziet dit hierna niet meer.`
                );
            } else {
                alert(
                    `Subplatform succesvol aangemaakt!\n\n` +
                    `Opmerking: geen wachtwoord ontvangen van de server.`
                );
            }

            setTimeout(() => window.location.reload(), 200);

        } catch (error) {
            console.error(error);
            alert("Fout bij aanmaken van subplatform");
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