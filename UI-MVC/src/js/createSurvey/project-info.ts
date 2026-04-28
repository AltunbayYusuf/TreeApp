type GenerateImageResponse = {
    ok: boolean;
    imageUrl?: string;
    message?: string;
};

type GenerateIntroResponse = {
    ok: boolean;
    introduction?: string;
    message?: string;
};

function getAntiForgeryToken(): string {
    const tokenInput = document.querySelector<HTMLInputElement>(
        "input[name='__RequestVerificationToken']"
    );

    return tokenInput?.value ?? "";
}

function getProjectInfoElements() {
    const form = document.getElementById("createProjectForm") as HTMLFormElement | null;
    const nameInput = document.getElementById("Name") as HTMLInputElement | null;
    const introductionInput = document.getElementById("Introduction") as HTMLTextAreaElement | null;

    const generateImageButton = document.getElementById("generateProjectImageButton") as HTMLButtonElement | null;
    const imagePreview = document.getElementById("projectImagePreview") as HTMLImageElement | null;
    const imageStatus = document.getElementById("projectImageStatus") as HTMLDivElement | null;
    const generatedPhotoUrlInput = document.getElementById("GeneratedPhotoUrl") as HTMLInputElement | null;

    const generateIntroductionButton = document.getElementById("generateIntroductionButton") as HTMLButtonElement | null;

    return {
        form,
        nameInput,
        introductionInput,
        generateImageButton,
        imagePreview,
        imageStatus,
        generatedPhotoUrlInput,
        generateIntroductionButton
    };
}

function setStatus(element: HTMLElement | null, message: string): void {
    if (element) {
        element.textContent = message;
    }
}

async function readJsonResponse<T>(response: Response): Promise<T | null> {
    try {
        return await response.json() as T;
    } catch {
        return null;
    }
}

async function generateProjectImage(): Promise<void> {
    const {
        form,
        nameInput,
        generateImageButton,
        imagePreview,
        imageStatus,
        generatedPhotoUrlInput
    } = getProjectInfoElements();

    if (!form || !nameInput || !generateImageButton) return;

    const subplatform = form.dataset.subplatform;
    const projectName = nameInput.value.trim();

    if (!subplatform || !projectName) {
        setStatus(imageStatus, "Geef eerst een projectnaam in.");
        return;
    }

    generateImageButton.disabled = true;
    generateImageButton.textContent = "Afbeelding genereren...";
    setStatus(imageStatus, "AI maakt een afbeelding op basis van de projectnaam.");

    try {
        const response = await fetch(`/${subplatform}/SubAdminProjects/GenerateProjectImage`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": getAntiForgeryToken()
            },
            body: JSON.stringify({ projectName })
        });

        const data = await readJsonResponse<GenerateImageResponse>(response);

        if (!response.ok || !data?.ok || !data.imageUrl) {
            setStatus(imageStatus, data?.message ?? "Afbeelding genereren is mislukt.");
            return;
        }

        if (imagePreview) {
            imagePreview.src = data.imageUrl;
            imagePreview.classList.remove("hidden");
        }

        if (generatedPhotoUrlInput) {
            generatedPhotoUrlInput.value = data.imageUrl;
        }

        setStatus(imageStatus, data.message ?? "Afbeelding gegenereerd.");
    } catch (error) {
        console.error("Fout bij genereren afbeelding:", error);
        setStatus(imageStatus, "Er ging iets mis bij het genereren van de afbeelding.");
    } finally {
        generateImageButton.disabled = false;
        generateImageButton.textContent = "Genereer met AI";
    }
}

async function generateIntroduction(): Promise<void> {
    const {
        form,
        nameInput,
        introductionInput,
        generateIntroductionButton
    } = getProjectInfoElements();

    if (!form || !nameInput || !introductionInput || !generateIntroductionButton) return;

    const subplatform = form.dataset.subplatform;
    const projectName = nameInput.value.trim();

    if (!subplatform || !projectName) {
        introductionInput.value = "Geef eerst een projectnaam in.";
        introductionInput.focus();
        return;
    }

    generateIntroductionButton.disabled = true;
    generateIntroductionButton.textContent = "AI schrijft...";

    try {
        const response = await fetch(`/${subplatform}/SubAdminProjects/GenerateIntroduction`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": getAntiForgeryToken()
            },
            body: JSON.stringify({ projectName })
        });

        const data = await readJsonResponse<GenerateIntroResponse>(response);

        if (!response.ok || !data?.ok || !data.introduction) {
            alert(data?.message ?? "Introductietekst genereren is mislukt.");
            return;
        }

        introductionInput.value = data.introduction;
        introductionInput.focus();
    } catch (error) {
        console.error("Fout bij genereren introductie:", error);
        alert("Er ging iets mis bij het genereren van de introductietekst.");
    } finally {
        generateIntroductionButton.disabled = false;
        generateIntroductionButton.textContent = "AI hulp";
    }
}

function initializeProjectInfo(): void {
    const { generateImageButton, generateIntroductionButton } = getProjectInfoElements();

    generateImageButton?.addEventListener("click", generateProjectImage);
    generateIntroductionButton?.addEventListener("click", generateIntroduction);
}

document.addEventListener("DOMContentLoaded", initializeProjectInfo);