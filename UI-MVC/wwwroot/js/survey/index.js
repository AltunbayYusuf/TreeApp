const button = document.getElementById("verzendbtn");

document.querySelectorAll('.range-box').forEach(btn => {
    btn.addEventListener('click', function() {
        // Verwijder 'active' van broertjes in dezelfde vraag
        this.parentElement.querySelectorAll('.range-box').forEach(b => b.classList.remove('active'));
        // Voeg toe aan deze
        this.classList.add('active');
    });
});
button.addEventListener("click", (e) => {
    const allAnswers = [];
    const questions = document.getElementsByClassName(".survey-question");

    questions.forEach(block => {
        const questionId = block.getAttribute("data-question-id");
        const type = block.getAttribute("data-type");
        let answer = null;

        if (type === "SingleChoice"){
            const selected = block.querySelector('input[type="radio"]:checked');
            answer = selected ? selected.value : null;
        }
        else if(type === "MultipleChoice"){
            const selected = Array.from(block.querySelectorAll('input[type="checkbox"]:checked'));
            answer = selected.map(cb => cb.value);
        }
        else if (type === "OpenQuestion") {
            answer = block.querySelector(".question-text").value;
        }
        else if (type === "Range") {
            const activeBtn = block.querySelector(".range-box.active");
            answer = activeBtn ? activeBtn.getAttribute("data-val") : null;
        }
        allAnswers.push({
            QuestionId: questionId,
            Value: answer
        });
    });


})