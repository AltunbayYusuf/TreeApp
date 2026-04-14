const btnGeven = document.getElementById("ideeGeven") as HTMLButtonElement | null;
const btnBekijken = document.getElementById("ideeenBekijken") as HTMLButtonElement | null;

const params = new URLSearchParams(window.location.search);
const projectId = params.get("projectId");

const subplatformInput = document.getElementById("subplatformSlug") as HTMLInputElement | null;
const subplatform = subplatformInput?.value;

if (btnGeven) {
    btnGeven.addEventListener("click", (): void => {
        window.location.href = projectId && subplatform
            ? `/${subplatform}/Idea/Create?projectId=${projectId}`
            : "/Idea/Create";
    });
}

if (btnBekijken) {
    btnBekijken.addEventListener("click", (): void => {
        window.location.href = projectId && subplatform
            ? `/${subplatform}/Idea/Index?projectId=${projectId}`
            : "/Idea/Index";
    });
}
