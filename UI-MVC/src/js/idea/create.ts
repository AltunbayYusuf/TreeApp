const submitIdeaBtn = document.getElementById("submit-idea-btn") as HTMLButtonElement | null;
const ideaTitle = document.getElementById("idea-title") as HTMLInputElement | null;
const ideaTopic = document.getElementById("idea-topic") as HTMLSelectElement | null;
const ideaText = document.getElementById("idea-text") as HTMLTextAreaElement | null;
const aiMessage = document.getElementById("idea-ai-message") as HTMLDivElement | null;

type IdeaResponse = {
    ok?: boolean;
    isToxic?: boolean;
    aiUnavailable?: boolean;
    warning?: string;
    explanation?: string;
    suggestedText?: string;
    message?: string;
};

function escapeHtml(str: string): string {
    return (str || "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

function getProjectRedirectUrl(): string {
    const params = new URLSearchParams(window.location.search);
    const projectId = params.get("projectId");
    const pathSegments = window.location.pathname.split("/").filter(Boolean);
    const subplatform = pathSegments[0];

    return projectId
        ? `/${subplatform}/Idea?projectId=${projectId}`
        : `/${subplatform}/Idea`;

}

async function postIdea(url: string, topicId: string, title: string, text: string): Promise<IdeaResponse> {
    const response = await fetch(url, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            topicId: Number(topicId),
            title: title,
            text: text
        })
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

function clearAiMessage(): void {
    if (!aiMessage) return;
    aiMessage.style.display = "none";
    aiMessage.innerHTML = "";
}

function showAiWarning(data: IdeaResponse, topicId: string, title: string, text: string): void {
    if (!aiMessage || !ideaText) return;

    aiMessage.style.display = "block";
    aiMessage.innerHTML = `
        <div class="alert alert-warning mb-0">
            <strong>${escapeHtml(data.warning || "AI: je tekst bevat mogelijk toxische inhoud.")}</strong>
            ${data.explanation ? `<div class="mt-2"><em>${escapeHtml(data.explanation)}</em></div>` : ""}
            ${data.suggestedText ? `<div class="mt-2"><strong>Alternatief:</strong><br>${escapeHtml(data.suggestedText)}</div>` : ""}
            <div class="mt-3 d-flex gap-2 flex-wrap">
                <button type="button" id="force-submit-btn" class="btn btn-outline-danger btn-sm">Toch versturen</button>
                <button type="button" id="use-alternative-btn" class="btn btn-outline-primary btn-sm">Alternatief gebruiken</button>
            </div>
        </div>
    `;

    const forceSubmitBtn = document.getElementById("force-submit-btn") as HTMLButtonElement | null;
    const useAlternativeBtn = document.getElementById("use-alternative-btn") as HTMLButtonElement | null;

    forceSubmitBtn?.addEventListener("click", async (): Promise<void> => {
        const forceResult = await postIdea("/api/ideas/force", topicId, title, text);

        if (!forceResult.ok) {
            aiMessage.innerHTML = `<div class="alert alert-danger mb-0">${escapeHtml(forceResult.message || "Er ging iets mis.")}</div>`;
            return;
        }

        clearAiMessage();
        alert("Idee doorgestuurd voor moderatie.");
        window.location.href = getProjectRedirectUrl();
    });

    useAlternativeBtn?.addEventListener("click", (): void => {
        ideaText.value = data.suggestedText || "";
        clearAiMessage();
        ideaText.focus();
    });
}

if (submitIdeaBtn && ideaTitle && ideaTopic && ideaText && aiMessage) {
    submitIdeaBtn.addEventListener("click", async (): Promise<void> => {
        const topicId = ideaTopic.value;
        const title = ideaTitle.value.trim();
        const text = ideaText.value.trim();

        clearAiMessage();

        if (!topicId || topicId === "0") {
            aiMessage.style.display = "block";
            aiMessage.innerHTML = `<div class="alert alert-danger mb-0">Kies eerst een topic.</div>`;
            return;
        }

        if (!text) {
            aiMessage.style.display = "block";
            aiMessage.innerHTML = `<div class="alert alert-danger mb-0">Beschrijf eerst je idee.</div>`;
            return;
        }

        const data = await postIdea("/api/ideas", topicId, title, text);

        if (!data.ok ) {
            aiMessage.style.display = "block";
            aiMessage.innerHTML = `<div class="alert alert-danger mb-0">${escapeHtml(data.message || "Er ging iets mis.")}</div>`;
            return;
        }
        if (data.isToxic) {
            showAiWarning(data, topicId, title, text);
            return;
        }
        
        if (data.aiUnavailable) {
            alert(data.message || "Je idee werd doorgestuurd voor moderatie.");
            window.location.href = getProjectRedirectUrl();
            return;
        }

        clearAiMessage();
        alert("Idee succesvol verstuurd.");
        window.location.href = getProjectRedirectUrl();
    });
}