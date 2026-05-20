import {DomUtils} from "../helpers/utils";

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

function getProjectInfoElements() {
    return {
        form: document.getElementById("createProjectForm") as HTMLFormElement | null,
        nameInput: document.getElementById("Name") as HTMLInputElement | null,
        introductionInput: document.getElementById("Introduction") as HTMLTextAreaElement | null,
        generateImageButton: document.getElementById("generateProjectImageButton") as HTMLButtonElement | null,
        introMediaUpload: document.getElementById("IntroMediaUpload") as HTMLInputElement | null,
        introMediaType: document.getElementById("IntroMediaType") as HTMLSelectElement | null,
        introMediaUploadStatus: document.getElementById("introMediaUploadStatus") as HTMLElement | null,
        imagePreview: document.getElementById("projectImagePreview") as HTMLImageElement | null,
        imageStatus: document.getElementById("projectImageStatus") as HTMLDivElement | null,
        generatedPhotoUrlInput: document.getElementById("IntroMediaUri") as HTMLInputElement | null,
        generateIntroductionButton: document.getElementById("generateIntroductionButton") as HTMLButtonElement | null,
    };
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
        introductionInput,
        generateImageButton,
        imagePreview,
        imageStatus,
        generatedPhotoUrlInput
    } = getProjectInfoElements();
    if (!form || !nameInput || !generateImageButton) return;

    const projectName = nameInput.value.trim();
    const description = introductionInput?.value.trim() ?? "";

    if (!projectName) {
        if (imageStatus) imageStatus.textContent = "Geef eerst een projectnaam in.";
        return;
    }

    generateImageButton.disabled = true;
    generateImageButton.textContent = "Afbeelding genereren...";
    if (imageStatus) imageStatus.textContent = "AI maakt een afbeelding op basis van titel en beschrijving.";
    try {
        const response = await fetch(`/SubAdminProjects/GenerateProjectImage`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": DomUtils.getAntiForgeryToken()
            },
            body: JSON.stringify({
                projectName,
                introduction: description
            })
        });

        const data = await readJsonResponse<GenerateImageResponse>(response);

        if (!response.ok || !data?.ok || !data.imageUrl) {
            if (imageStatus) imageStatus.textContent = data?.message ?? "Afbeelding genereren is mislukt.";
            return;
        }

        if (imagePreview) {
            imagePreview.src = data.imageUrl;
            imagePreview.classList.remove("d-none");
        }

        if (generatedPhotoUrlInput) generatedPhotoUrlInput.value = data.imageUrl;
        if (imageStatus) imageStatus.textContent = data.message ?? "Afbeelding gegenereerd.";
    } catch (error) {
        console.error("Fout bij genereren afbeelding:", error);
        if (imageStatus) imageStatus.textContent = "Er ging iets mis bij het genereren van de afbeelding.";
    } finally {
        generateImageButton.disabled = false;
        generateImageButton.textContent = "Genereer met AI";
    }
}

async function generateIntroduction(): Promise<void> {
    const {form, nameInput, introductionInput, generateIntroductionButton} = getProjectInfoElements();

    if (!form || !nameInput || !introductionInput || !generateIntroductionButton) return;

    const projectName = nameInput.value.trim();

    if (!projectName) {
        introductionInput.value = "Geef eerst een projectnaam in.";
        introductionInput.focus();
        return;
    }

    generateIntroductionButton.disabled = true;
    generateIntroductionButton.textContent = "AI schrijft...";

    try {
        const response = await fetch(`/SubAdminProjects/GenerateIntroduction`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": DomUtils.getAntiForgeryToken()
            },
            body: JSON.stringify({projectName})
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

function updateIntroMediaUploadStatus(): void {
    const {
        introMediaUpload,
        introMediaType,
        introMediaUploadStatus,
        imagePreview,
        imageStatus,
        generatedPhotoUrlInput
    } = getProjectInfoElements();

    if (!introMediaUpload || !introMediaUploadStatus) return;

    const file = introMediaUpload.files?.[0] ?? null;

    if (!file) {
        introMediaUploadStatus.style.display = "none";
        introMediaUploadStatus.textContent = "";
        return;
    }

    const isVideo = file.type.startsWith("video/") || introMediaType?.value === "Video";
    introMediaUploadStatus.textContent = `${isVideo ? "Video" : "Foto"} gekozen: ${file.name}`;
    introMediaUploadStatus.style.display = "block";

    if (generatedPhotoUrlInput) generatedPhotoUrlInput.value = "";
    if (imageStatus) imageStatus.textContent = "Bestand gekozen. Opslaan om te uploaden.";

    if (!isVideo && imagePreview) {
        imagePreview.src = URL.createObjectURL(file);
        imagePreview.classList.remove("d-none");
    }
}

document.addEventListener("DOMContentLoaded", () => {
    const {generateImageButton, generateIntroductionButton, introMediaUpload} = getProjectInfoElements();
    generateImageButton?.addEventListener("click", generateProjectImage);
    generateIntroductionButton?.addEventListener("click", generateIntroduction);
    introMediaUpload?.addEventListener("change", updateIntroMediaUploadStatus);
});
