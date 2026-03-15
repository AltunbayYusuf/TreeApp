const btnGeven = document.getElementById("ideeGeven") as HTMLButtonElement | null;
const btnBekijken = document.getElementById("ideeenBekijken") as HTMLButtonElement | null;

const params = new URLSearchParams(window.location.search);
const projectId = params.get("projectId");

if (btnGeven) {
    btnGeven.addEventListener("click", (): void => {
        window.location.href = projectId
            ? `/Idea/Create?projectId=${projectId}`
            : "/Idea/Create";
    });
}

if (btnBekijken) {
    btnBekijken.addEventListener("click", (): void => {
        window.location.href = projectId
            ? `/Idea/Index?projectId=${projectId}`
            : "/Idea/Index";
    });
}