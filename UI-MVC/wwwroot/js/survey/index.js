const button = document.getElementById("verzendbtn");

button.addEventListener("click", (e) => {
    const formData = new URLSearchParams();
    const questions = document.querySelectorAll(".survey-question");

    questions.forEach((block, index) => {
        const questionId = block.getAttribute("data-question-id");
        const type = block.getAttribute("data-type");
        let resultValue = ""; 
        if (type === "SingleChoice") {
            const selected = block.querySelector('input[type="radio"]:checked');
            resultValue = selected ? selected.value : "";
        }
        else if (type === "MultipleChoice") {
            const selected = Array.from(block.querySelectorAll('input[type="checkbox"]:checked'));
           
            resultValue = selected.map(cb => cb.value).join(", ");
        }
        else if (type === "OpenQuestion") {
            resultValue = block.querySelector(".question-text").value;
        }
        else if (type === "Range") {
            const activeBtn = block.querySelector(".range-box.active");
            resultValue = activeBtn ? activeBtn.getAttribute("data-val") : "";
        }

        formData.append(`answers[${index}].QuestionId`, questionId);
        formData.append(`answers[${index}].Value`, resultValue);
    });

    fetch('/Survey/Submit', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: formData.toString()
    })
        .then(response => {
            if (response.ok) return response.json();
            throw new Error('Netwerk response was niet ok');
        })
        .then(data => {
            if (data.redirectUrl) window.location.href = data.redirectUrl;
        })
        .catch(error => console.error('Fout bij verzenden:', error));
    
    //Deze redirect moet weg het is gewoon om te testen dat de antwoorden opgeslagen werden
});