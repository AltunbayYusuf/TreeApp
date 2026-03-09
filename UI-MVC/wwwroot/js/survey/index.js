const button = document.getElementById("verzendbtn");

button.addEventListener("click", (e) => {
    e.preventDefault();
    const formData = new URLSearchParams();
    const questions = document.querySelectorAll(".survey-question");
    let allFilled = true;

    questions.forEach((block, index) => {
        const questionId = block.getAttribute("data-question-id");
        const type = block.getAttribute("data-type");
        let resultValue = "";
        let questionAnswered = false;

        if (type === "SingleChoice") {
            const selected = block.querySelector('input[type="radio"]:checked');
            if (selected) {
                resultValue = selected.value;
                questionAnswered = true;
            }
        } else if (type === "MultipleChoice") {
            const selected = Array.from(block.querySelectorAll('input[type="checkbox"]:checked'));
            if (selected.length > 0) {
                resultValue = selected.map(cb => cb.value).join(", ");
                questionAnswered = true;
            }
        } else if (type === "OpenQuestion") {
            resultValue = block.querySelector(".question-text").value.trim();
            if (resultValue !== "") {
                questionAnswered = true;
            }
        }
         else if (type === "Range") {
        //     const activeBtn = block.querySelector(".range-box.active");
        //     if (activeBtn) {
        //         resultValue = activeBtn.getAttribute("data-val");
                questionAnswered = true;
        //     }
        }
        
        if (!questionAnswered) {
            block.style.border = "2px solid red";
            allFilled = false;
        } else {
            block.style.border = "";
        }

        formData.append(`answers[${index}].QuestionId`, questionId);
        formData.append(`answers[${index}].Value`, resultValue);
    });

    if (!allFilled) {
        alert("Niet alle vragen zijn beantwoord!");
        return;
    }
    
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

});