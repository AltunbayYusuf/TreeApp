const overviewBtn = document.getElementById("overviewBtn") as HTMLButtonElement | null;
const overviewSection = document.getElementById("overviewSection") as HTMLElement | null;

if (overviewBtn && overviewSection) {
    overviewBtn.addEventListener("click", () => {
        const isHidden = overviewSection.style.display === "none" || overviewSection.style.display === "";

        overviewSection.style.display = isHidden ? "block" : "none";
    });
}