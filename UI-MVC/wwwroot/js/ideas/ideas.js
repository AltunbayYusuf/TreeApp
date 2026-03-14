const ideas = document.querySelectorAll(".idea-item");
const showMoreBtn = document.getElementById("show-more-btn");


function toonMeer() {


    let visibleCount = 2;

    function updateIdeas() {
        ideas.forEach((idea, index) => {
            idea.style.display = index < visibleCount ? "block" : "none";
        });

        if (showMoreBtn && visibleCount >= ideas.length) {
            showMoreBtn.style.display = "none";
        }
    }

    if (showMoreBtn) {
        showMoreBtn.addEventListener("click", () => {
            visibleCount += 3;
            updateIdeas();
        });
    }

    updateIdeas();
}



document.addEventListener("DOMContentLoaded", toonMeer);


