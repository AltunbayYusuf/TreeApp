const btnGeven = document.getElementById("ideeGeven");
const btnBekijken = document.getElementById("ideeenBekijken");

if (btnGeven) {
    btnGeven.addEventListener("click", () => {
        window.location.href = "/Idea/Create";
    });
}

if (btnBekijken) {
    btnBekijken.addEventListener("click", () => {
        window.location.href = "/Idea/Index";
    });
}