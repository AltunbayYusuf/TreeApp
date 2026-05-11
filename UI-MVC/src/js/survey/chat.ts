// ── Chatbot survey ────────────────────────────────────────────────────────

interface QuestionOption {
    text: string;
}

interface SurveyQuestion {
    id: number;
    description: string;
    questionType: 'SingleChoice' | 'MultipleChoice' | 'Range' | 'OpenQuestion';
    options: QuestionOption[];
    rangeMin: number | null;
    rangeMax: number | null;
    rangeMinLabel: string | null;
    rangeMaxLabel: string | null;
}

interface CollectedAnswer {
    questionId: number;
    value: string;
}

const chatMessages = document.getElementById('chatMessages') as HTMLDivElement | null;
const chatInputArea = document.getElementById('chatInputArea') as HTMLDivElement | null;
const chatWindow = document.getElementById('chatWindow') as HTMLDivElement | null;

if (!chatMessages || !chatInputArea || !chatWindow) {
    // Niet op de chatbot-pagina, niets te doen
} else {
    const dataEl = document.getElementById('survey-data');
    const questions: SurveyQuestion[] = dataEl
        ? (JSON.parse(dataEl.textContent ?? '[]') as SurveyQuestion[])
        : [];

    let currentIndex = 0;
    const answers: CollectedAnswer[] = [];
    sessionStorage.setItem(
        "surveyStartTime",
        Date.now().toString()
    );

    // ── helpers ──────────────────────────────────────────────────────────

    function scrollBottom(): void {
        if (chatWindow) chatWindow.scrollTop = chatWindow.scrollHeight;
    }

    function addBotMessage(text: string, delay = 0): void {
        setTimeout(() => {
            const typing = document.createElement('div');
            typing.className = 'chat-msg justify-content-start typing-indicator';
            typing.innerHTML = '<span></span><span></span><span></span>';
            chatMessages!.appendChild(typing);
            scrollBottom();

            setTimeout(() => {
                typing.remove();
                const div = document.createElement('div');
                div.className = 'chat-msg justify-content-start';
                div.innerHTML = `
                    <div class="chat-avatar">💬</div>
                    <div class="chat-bubble bot-bubble">${text}</div>`;
                chatMessages!.appendChild(div);
                scrollBottom();
            }, 800);
        }, delay);
    }

    function addUserMessage(text: string): void {
        const div = document.createElement('div');
        div.className = 'chat-msg justify-content-end';
        div.innerHTML = `<div class="chat-bubble user-bubble">${text}</div>`;
        chatMessages!.appendChild(div);
        scrollBottom();
    }

    function clearInput(): void {
        chatInputArea!.innerHTML = '';
    }

    // ── record + advance ──────────────────────────────────────────────────

    function recordAnswer(questionId: number, value: string, displayText: string): void {
        answers.push({questionId, value});
        clearInput();
        addUserMessage(displayText);
        currentIndex++;
        setTimeout(() => showQuestion(currentIndex), 600);
    }

    // ── question renderers ────────────────────────────────────────────────

    function renderSingleChoice(q: SurveyQuestion): void {
        const wrap = document.createElement('div');
        wrap.className = 'd-flex flex-wrap gap-2 w-100';
        q.options.forEach(opt => {
            const btn = document.createElement('button');
            btn.className = 'btn chat-opt-btn';
            btn.textContent = opt.text;
            btn.addEventListener('click', () => recordAnswer(q.id, opt.text, opt.text));
            wrap.appendChild(btn);
        });
        chatInputArea!.appendChild(wrap);
    }

    function renderMultiChoice(q: SurveyQuestion): void {
        const wrap = document.createElement('div');
        wrap.className = 'w-100 d-flex flex-column gap-2';

        const hint = document.createElement('p');
        hint.className = 'small text-muted m-0';
        hint.textContent = 'Meerdere antwoorden mogelijk – klik Bevestig als je klaar bent.';
        wrap.appendChild(hint);

        const optWrap = document.createElement('div');
        optWrap.className = 'd-flex flex-wrap gap-2';
        const selected: string[] = [];

        q.options.forEach(opt => {
            const btn = document.createElement('button');
            btn.className = 'btn chat-opt-btn';
            btn.textContent = opt.text;
            btn.addEventListener('click', () => {
                const idx = selected.indexOf(opt.text);
                if (idx === -1) {
                    selected.push(opt.text);
                    btn.classList.add('selected');
                } else {
                    selected.splice(idx, 1);
                    btn.classList.remove('selected');
                }
                confirmBtn.disabled = selected.length === 0;
            });
            optWrap.appendChild(btn);
        });
        wrap.appendChild(optWrap);

        const confirmBtn = document.createElement('button');
        confirmBtn.className = 'btn btn-primary chat-confirm-btn';
        confirmBtn.textContent = 'Bevestig';
        confirmBtn.disabled = true;
        confirmBtn.addEventListener('click', () => {
            const value = selected.join(', ');
            recordAnswer(q.id, value, value);
        });
        wrap.appendChild(confirmBtn);
        chatInputArea!.appendChild(wrap);
    }

    function renderRange(q: SurveyQuestion): void {
        const min = q.rangeMin ?? 1;
        const max = q.rangeMax ?? 5;

        const wrap = document.createElement('div');
        wrap.className = 'w-100 d-flex flex-column gap-1';

        if (q.rangeMinLabel || q.rangeMaxLabel) {
            const labels = document.createElement('div');
            labels.className = 'd-flex justify-content-between small text-muted px-1';
            labels.innerHTML =
                `<span>${q.rangeMinLabel ?? ''}</span><span>${q.rangeMaxLabel ?? ''}</span>`;
            wrap.appendChild(labels);
        }

        const row = document.createElement('div');
        row.className = 'd-flex flex-wrap gap-1';
        for (let i = min; i <= max; i++) {
            const btn = document.createElement('button');
            btn.className = 'btn chat-range-btn';
            btn.textContent = String(i);
            btn.addEventListener('click', () => recordAnswer(q.id, String(i), String(i)));
            row.appendChild(btn);
        }
        wrap.appendChild(row);
        chatInputArea!.appendChild(wrap);
    }

    function renderOpen(q: SurveyQuestion): void {
        const wrap = document.createElement('div');
        wrap.className = 'w-100 d-flex flex-column gap-2';

        const textarea = document.createElement('textarea');
        textarea.className = 'chat-textarea';
        textarea.placeholder = 'Typ je antwoord hier…';
        textarea.rows = 3;

        const sendBtn = document.createElement('button');
        sendBtn.className = 'btn btn-primary chat-send-btn';
        sendBtn.textContent = 'Verstuur ➤';

        const submit = () => {
            const value = textarea.value.trim();
            if (!value) return;
            recordAnswer(q.id, value, value);
        };

        sendBtn.addEventListener('click', submit);
        textarea.addEventListener('keydown', (e: KeyboardEvent) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                submit();
            }
        });

        wrap.appendChild(textarea);
        wrap.appendChild(sendBtn);
        chatInputArea!.appendChild(wrap);
        textarea.focus();
    }

    // ── main flow ─────────────────────────────────────────────────────────

    function showQuestion(index: number): void {
        if (index >= questions.length) {
            showFinish();
            return;
        }
        const q = questions[index];
        addBotMessage(q.description, 0);

        setTimeout(() => {
            switch (q.questionType) {
                case 'SingleChoice':
                    renderSingleChoice(q);
                    break;
                case 'MultipleChoice':
                    renderMultiChoice(q);
                    break;
                case 'Range':
                    renderRange(q);
                    break;
                case 'OpenQuestion':
                    renderOpen(q);
                    break;
            }
        }, 900);
    }

    function showFinish(): void {
        addBotMessage('Bedankt voor je antwoorden! Klik hieronder om de bevraging af te ronden.', 0);
        setTimeout(() => {
            const btn = document.createElement('button');
            btn.className = 'btn btn-primary chat-submit-btn';
            btn.textContent = '✓ Bevraging afronden';
            btn.addEventListener('click', submitSurvey);
            chatInputArea!.appendChild(btn);
        }, 900);
    }

    function submitSurvey(): void {
        const startTime = Number(
            sessionStorage.getItem("surveyStartTime")
        );

        const durationInSeconds = Math.floor(
            (Date.now() - startTime) / 1000
        );

        const params = new URLSearchParams(window.location.search);
        const projectId = Number(params.get("projectId"));

        const subplatformInput =
            document.getElementById(
                "subplatformSlug"
            ) as HTMLInputElement | null;

        const subplatform = subplatformInput?.value;

        const submitUrl =
            projectId && subplatform
                ? `/${subplatform}/Survey/Submit`
                : "/Survey/Submit";

        const payload = {
            projectId,
            durationInSeconds,
            answers
        };

        fetch(submitUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        })
            .then(res => {
                if (res.ok) return res.json();
                throw new Error("Netwerk fout");
            })
            .then((data: { redirectUrl?: string }) => {
                sessionStorage.removeItem("surveyStartTime");

                if (data.redirectUrl) {
                    window.location.href = data.redirectUrl;
                }
            })
            .catch(err =>
                console.error("Fout bij verzenden:", err)
            );
    }

    // ── boot ──────────────────────────────────────────────────────────────

    if (questions.length > 0) {
        addBotMessage('Hallo! Ik ga je een aantal vragen stellen. Neem rustig de tijd.', 300);
        setTimeout(() => showQuestion(0), 1600);
    }
}
