// ideas/create.ts
import {DomUtils} from "../helpers/utils";

type IdeaResponse = {
    ok?: boolean;
    isToxic?: boolean;
    aiUnavailable?: boolean;
    warning?: string;
    explanation?: string;
    suggestedTitle?: string;
    suggestedText?: string;
    message?: string;
};

type ImproveIdeaResponse = {
    ok?: boolean;
    improvedTitle?: string;
    improvedText?: string;
    message?: string;
};

export class IdeaCreator {
    private topicId = "";
    private title = "";
    private text = "";
    
    private recognition: SpeechRecognition | null = null;

    // Toegevoegd: gekozen afbeelding bijhouden
    private imageFile: File | null = null;

    private aiAlternativeTitle = "";
    private aiAlternativeText = "";

    init(): void {
        document.getElementById("idea-contact-opt-in")?.addEventListener("change", this.toggleContactEmail.bind(this));
        document.getElementById("submit-idea-btn")?.addEventListener("click", this.handleSubmit.bind(this));
        document.getElementById("ai-improve-create-btn")?.addEventListener("click", this.handleImproveWithAi.bind(this));
        document.getElementById("use-ai-improvement-btn")?.addEventListener("click", this.useAiImprovement.bind(this));
        document.getElementById("speech-to-text-btn")?.addEventListener("click", this.startSpeechToText.bind(this));
        document.getElementById("stop-speech-btn")?.addEventListener("click", this.stopSpeechToText.bind(this));
        // Toegevoegd: afbeelding preview
        document.getElementById("idea-image")?.addEventListener("change", this.handleImagePreview.bind(this));

        this.toggleContactEmail();
    }

    private handleImagePreview(): void {
        const imageInput = document.getElementById("idea-image") as HTMLInputElement | null;
        const previewWrapper = document.getElementById("idea-image-preview-wrapper") as HTMLDivElement | null;
        const previewImage = document.getElementById("idea-image-preview") as HTMLImageElement | null;

        if (!imageInput) {
            return;
        }

        const file = imageInput.files?.[0] ?? null;
        this.imageFile = file;

        if (!file || !previewWrapper || !previewImage) {
            if (previewWrapper) {
                previewWrapper.style.display = "none";
            }
            return;
        }

        const allowedTypes = ["image/jpeg", "image/png", "image/webp"];

        if (!allowedTypes.includes(file.type)) {
            this.showError("Alleen jpg, jpeg, png of webp is toegestaan.");
            imageInput.value = "";
            this.imageFile = null;
            previewWrapper.style.display = "none";
            return;
        }

        previewImage.src = URL.createObjectURL(file);
        previewWrapper.style.display = "block";
    }
    private startSpeechToText(): void {
        const SpeechRecognitionConstructor =
            window.SpeechRecognition || window.webkitSpeechRecognition;

        const ideaText = document.getElementById("idea-text") as HTMLTextAreaElement | null;
        const startButton = document.getElementById("speech-to-text-btn") as HTMLButtonElement | null;
        const stopButton = document.getElementById("stop-speech-btn") as HTMLButtonElement | null;
        const speechStatus = document.getElementById("speech-status") as HTMLElement | null;

        if (!ideaText) {
            return;
        }

        if (!SpeechRecognitionConstructor) {
            this.showError("Spraak naar tekst wordt niet ondersteund door deze browser. Probeer Google Chrome.");
            return;
        }

        this.recognition = new SpeechRecognitionConstructor();
        this.recognition.lang = "nl-BE";
        this.recognition.continuous = true;
        this.recognition.interimResults = true;

        let finalTranscript = ideaText.value.trim();

        this.recognition.onstart = () => {
            if (startButton) startButton.style.display = "none";
            if (stopButton) stopButton.style.display = "inline-block";
            if (speechStatus) {
                speechStatus.style.display = "block";
                speechStatus.textContent = "🎤 Ik luister... spreek je idee rustig in.";
            }
        };

        this.recognition.onresult = (event: SpeechRecognitionEvent) => {
            let interimTranscript = "";

            for (let i = event.resultIndex; i < event.results.length; i++) {
                const transcript = event.results[i][0].transcript;

                if (event.results[i].isFinal) {
                    finalTranscript += finalTranscript ? ` ${transcript}` : transcript;
                } else {
                    interimTranscript += transcript;
                }
            }

            ideaText.value = `${finalTranscript} ${interimTranscript}`.trim();
        };

        this.recognition.onerror = () => {
            this.showError("Er ging iets mis met de spraakherkenning. Probeer opnieuw.");
            this.resetSpeechButtons();
        };

        this.recognition.onend = () => {
            this.resetSpeechButtons();
        };

        this.recognition.start();
    }

    private stopSpeechToText(): void {
        if (this.recognition) {
            this.recognition.stop();
            this.recognition = null;
        }

        this.resetSpeechButtons();
    }

    private resetSpeechButtons(): void {
        const startButton = document.getElementById("speech-to-text-btn") as HTMLButtonElement | null;
        const stopButton = document.getElementById("stop-speech-btn") as HTMLButtonElement | null;
        const speechStatus = document.getElementById("speech-status") as HTMLElement | null;

        if (startButton) startButton.style.display = "inline-block";
        if (stopButton) stopButton.style.display = "none";

        if (speechStatus) {
            speechStatus.style.display = "none";
            speechStatus.textContent = "";
        }
    }

    private async handleImproveWithAi(): Promise<void> {
        const ideaTitle = document.getElementById("idea-title") as HTMLInputElement | null;
        const ideaText = document.getElementById("idea-text") as HTMLTextAreaElement | null;
        const improveButton = document.getElementById("ai-improve-create-btn") as HTMLButtonElement | null;
        const useButton = document.getElementById("use-ai-improvement-btn") as HTMLButtonElement | null;
        const resultBox = document.getElementById("ai-improve-result") as HTMLDivElement | null;
        const resultText = document.getElementById("ai-improve-text") as HTMLParagraphElement | null;

        if (!ideaText || !ideaTitle || !improveButton || !useButton || !resultBox || !resultText) return;

        if (!ideaText.value.trim()) {
            resultBox.style.display = "block";
            resultText.textContent = "Schrijf eerst een idee voordat je AI gebruikt.";
            useButton.style.display = "none";
            return;
        }

        improveButton.disabled = true;
        improveButton.textContent = "AI denkt na...";

        try {
            const response = await fetch("/api/ideas/improve", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
                body: JSON.stringify({
                    title: ideaTitle.value,
                    text: ideaText.value,
                    language: localStorage.getItem("selectedLanguage") ?? "nl"
                })
            });

            const data = await response.json() as ImproveIdeaResponse;

            resultBox.style.display = "block";

            if (!response.ok || !data.ok) {
                resultText.textContent = data.message ?? "AI kon geen alternatief maken.";
                useButton.style.display = "none";
                return;
            }

            this.aiAlternativeTitle = data.improvedTitle ?? "";
            this.aiAlternativeText = data.improvedText ?? "";

            resultText.innerHTML = `
                <strong>Titel:</strong>
                ${DomUtils.escapeHtml(this.aiAlternativeTitle)}
                
                <strong>Inhoud:</strong>
                ${DomUtils.escapeHtml(this.aiAlternativeText)}
            `;
            useButton.style.display = "inline-block";
        } catch {
            resultBox.style.display = "block";
            resultText.textContent = "AI-assistentie is tijdelijk niet beschikbaar.";
            useButton.style.display = "none";
        } finally {
            improveButton.disabled = false;
            improveButton.textContent = "✨ Herschrijf met AI";
        }
    }

    private useAiImprovement(): void {
        const ideaTitle = document.getElementById("idea-title") as HTMLInputElement | null;
        const ideaText = document.getElementById("idea-text") as HTMLTextAreaElement | null;
        const resultBox = document.getElementById("ai-improve-result") as HTMLDivElement | null;

        if (!ideaTitle || !ideaText) return;

        ideaTitle.value = this.aiAlternativeTitle;
        ideaText.value = this.aiAlternativeText;

        if (resultBox) {
            resultBox.style.display = "none";
        }

        this.clearAiMessage();
    }

    private toggleContactEmail(): void {
        const contactOptIn = document.getElementById("idea-contact-opt-in") as HTMLInputElement | null;
        const contactEmailWrapper = document.getElementById("idea-contact-email-wrapper") as HTMLDivElement | null;
        const contactEmail = document.getElementById("idea-contact-email") as HTMLInputElement | null;

        const isChecked = contactOptIn?.checked ?? false;

        if (contactEmailWrapper) {
            contactEmailWrapper.style.display = isChecked ? "block" : "none";
        }

        if (contactEmail) {
            contactEmail.required = isChecked;
        }

        if (!isChecked && contactEmail) {
            contactEmail.value = "";
        }
    }

    private clearAiMessage(): void {
        const aiMessage = document.getElementById("idea-ai-message") as HTMLDivElement | null;
        if (!aiMessage) return;

        aiMessage.style.display = "none";
        aiMessage.innerHTML = "";
    }

    private showError(message: string): void {
        const aiMessage = document.getElementById("idea-ai-message") as HTMLDivElement | null;
        if (!aiMessage) return;

        aiMessage.style.display = "block";
        aiMessage.className = "mb-3 idea-message idea-message-danger";
        aiMessage.textContent = message;
    }

    private async handleSubmit(e: Event): Promise<void> {
        e.preventDefault();

        const ideaTopic = document.getElementById("idea-topic") as HTMLSelectElement | null;
        const ideaTitle = document.getElementById("idea-title") as HTMLInputElement | null;
        const ideaText = document.getElementById("idea-text") as HTMLTextAreaElement | null;
        const contactOptIn = document.getElementById("idea-contact-opt-in") as HTMLInputElement | null;
        const contactEmail = document.getElementById("idea-contact-email") as HTMLInputElement | null;

        this.topicId = ideaTopic?.value ?? "";
        this.title = ideaTitle?.value.trim() ?? "";
        this.text = ideaText?.value.trim() ?? "";

        this.clearAiMessage();

        if (!this.topicId || this.topicId === "0") {
            this.showError("Kies eerst een topic.");
            return;
        }

        if (!this.title) {
            this.showError("Geef eerst een titel in.");
            return;
        }

        if (!this.text) {
            this.showError("Beschrijf eerst je idee.");
            return;
        }

        if (contactOptIn?.checked && !contactEmail?.value.trim()) {
            this.showError("Geef je e-mailadres in als je gecontacteerd wil worden.");
            contactEmail?.focus();
            return;
        }

        if (contactOptIn?.checked && contactEmail && !contactEmail.checkValidity()) {
            this.showError("Geef een geldig e-mailadres in.");
            contactEmail.focus();
            return;
        }

        const data = await this.postIdea("/api/ideas", false);

        if (!data.ok) {
            this.showError(data.message || "Er ging iets mis.");
            return;
        }

        if (data.isToxic) {
            this.showAiWarning(data);
            return;
        }

        if (data.aiUnavailable) {
            alert(data.message || "Je idee werd doorgestuurd voor moderatie.");
            window.location.href = DomUtils.getProjectRedirectUrl("Idea");
            return;
        }

        alert("Idee succesvol verstuurd.");
        window.location.href = DomUtils.getProjectRedirectUrl("Idea");
    }

    private async postIdea(url: string, skipAiModeration: boolean): Promise<IdeaResponse> {
        const contactOptIn = document.getElementById("idea-contact-opt-in") as HTMLInputElement | null;
        const contactEmail = document.getElementById("idea-contact-email") as HTMLInputElement | null;
        const subplatformSlug = document.getElementById("idea-subplatform-slug") as HTMLInputElement | null;

        const formData = new FormData();

        formData.append("topicId", String(Number(this.topicId)));
        formData.append("title", this.title);
        formData.append("text", this.text);
        formData.append("contactOptIn", String(contactOptIn?.checked ?? false));
        formData.append("email", contactEmail?.value.trim() ?? "");
        formData.append("subplatformSlug", subplatformSlug?.value ?? "");
        formData.append("skipAiModeration", String(skipAiModeration));

        if (this.imageFile) {
            formData.append("imageUpload", this.imageFile);
        }

        const response = await fetch(url, {
            method: "POST",
            body: formData
        });

        const data = await response.json() as IdeaResponse;

        if (!response.ok) {
            return {
                ok: false,
                aiUnavailable: data.aiUnavailable,
                message: data.message || "Er ging iets mis."
            };
        }

        return data;
    }

    private showAiWarning(data: IdeaResponse): void {
        const aiMessage = document.getElementById("idea-ai-message") as HTMLDivElement | null;
        if (!aiMessage) return;

        const suggestedTitle = data.suggestedTitle || this.title;
        const suggestedText = data.suggestedText || "";

        aiMessage.style.display = "block";
        aiMessage.className = "mb-3 idea-message idea-message-warning";
        aiMessage.innerHTML = `
            <strong>${DomUtils.escapeHtml(data.warning || "AI: je tekst bevat mogelijk toxische inhoud.")}</strong>
            ${data.explanation ? `<div class="mt-2"><em>${DomUtils.escapeHtml(data.explanation)}</em></div>` : ""}
            <div class="mt-2">
                <strong>Alternatief:</strong><br>
                <strong>Titel:</strong><br>
                ${DomUtils.escapeHtml(suggestedTitle)}<br><br>
                <strong>Inhoud:</strong><br>
                ${DomUtils.escapeHtml(suggestedText)}
            </div>
            <div class="mt-3 d-flex gap-2 flex-wrap">
                <button type="button" id="force-submit-btn" class="btn btn-outline-danger btn-sm">Toch versturen</button>
                <button type="button" id="use-alternative-btn" class="btn btn-outline-primary btn-sm">Alternatief gebruiken</button>
            </div>
        `;

        document.getElementById("force-submit-btn")?.addEventListener("click", this.handleForceSubmit.bind(this));

        document.getElementById("use-alternative-btn")?.addEventListener("click", () => {
            const ideaTitle = document.getElementById("idea-title") as HTMLInputElement | null;
            const ideaText = document.getElementById("idea-text") as HTMLTextAreaElement | null;

            this.aiAlternativeTitle = suggestedTitle;
            this.aiAlternativeText = suggestedText;

            if (ideaTitle) {
                ideaTitle.value = this.aiAlternativeTitle;
            }

            if (ideaText) {
                ideaText.value = this.aiAlternativeText;
                ideaText.focus();
            }

            this.clearAiMessage();
        });
    }

    private async handleForceSubmit(): Promise<void> {
        const forceResult = await this.postIdea("/api/ideas/force", false);

        if (!forceResult.ok) {
            this.showError(forceResult.message || "Er ging iets mis.");
            return;
        }

        this.clearAiMessage();
        alert("Idee doorgestuurd voor moderatie.");
        window.location.href = DomUtils.getProjectRedirectUrl("Idea");
    }
}

document.addEventListener("DOMContentLoaded", () => new IdeaCreator().init());
