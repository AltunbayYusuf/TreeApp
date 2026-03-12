const btnGeven = document.getElementById("ideeGeven") as HTMLButtonElement | null;
const btnBekijken = document.getElementById("ideeenBekijken") as HTMLButtonElement | null;

if (btnGeven) {
    btnGeven.addEventListener("click", (): void => {
        window.location.href = "/Idea/Create";
    });
}

if (btnBekijken) {
    btnBekijken.addEventListener("click", (): void => {
        window.location.href = "/Idea/Index";
    });
}