const btnGeven = document.getElementById("ideeGeven");
const btnBekijken = document.getElementById("ideeenBekijken");

if (btnGeven) {
    btnGeven.addEventListener("click", () => {
        window.location.href = "/Chat/Index";
    });
}

if (btnBekijken) {
    btnBekijken.addEventListener("click", () => {
        window.location.href = "/Ideas/Index";
    });
}