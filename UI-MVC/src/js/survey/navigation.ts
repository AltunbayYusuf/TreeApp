export class SurveyNavigation {
    private prevButton: HTMLButtonElement | null;
    private nextButton: HTMLButtonElement | null;
    private progressElement: HTMLDivElement | null;

    constructor() {
        this.prevButton = document.getElementById("prevQuestionBtn") as HTMLButtonElement | null;
        this.nextButton = document.getElementById("nextQuestionBtn") as HTMLButtonElement | null;
        this.progressElement = document.getElementById("questionProgress") as HTMLDivElement | null;
    }

    init(): void {
        if (!this.prevButton || !this.nextButton || !this.progressElement) {
            return;
        }

        this.prevButton.addEventListener("click", () => this.goToPreviousQuestion());
        this.nextButton.addEventListener("click", () => this.goToNextQuestion());

        window.addEventListener("scroll", () => this.updateNavigationState());
        window.addEventListener("resize", () => this.updateNavigationState());

        this.observeConditionalQuestions();
        this.updateNavigationState();
    }

    private getVisibleQuestions(): HTMLElement[] {
        return Array.from(document.querySelectorAll<HTMLElement>(".survey-question"))
            .filter(question =>
                !question.classList.contains("d-none") &&
                question.offsetParent !== null
            );
    }

    private getCurrentQuestionIndex(): number {
        const questions = this.getVisibleQuestions();

        if (questions.length === 0) {
            return 0;
        }

        const screenMiddle = window.innerHeight / 2;

        let closestIndex = 0;
        let closestDistance = Number.MAX_VALUE;

        questions.forEach((question, index) => {
            const rect = question.getBoundingClientRect();
            const questionMiddle = rect.top + rect.height / 2;
            const distance = Math.abs(questionMiddle - screenMiddle);

            if (distance < closestDistance) {
                closestDistance = distance;
                closestIndex = index;
            }
        });

        return closestIndex;
    }

    private scrollToQuestion(index: number): void {
        const questions = this.getVisibleQuestions();

        if (!questions[index]) {
            return;
        }

        questions[index].scrollIntoView({
            behavior: "smooth",
            block: "center"
        });

        window.setTimeout(() => this.updateNavigationState(), 400);
    }

    private goToPreviousQuestion(): void {
        const currentIndex = this.getCurrentQuestionIndex();

        if (currentIndex > 0) {
            this.scrollToQuestion(currentIndex - 1);
        }
    }

    private goToNextQuestion(): void {
        const questions = this.getVisibleQuestions();
        const currentIndex = this.getCurrentQuestionIndex();

        if (currentIndex < questions.length - 1) {
            this.scrollToQuestion(currentIndex + 1);
        }
    }

    private updateNavigationState(): void {
        if (!this.prevButton || !this.nextButton || !this.progressElement) {
            return;
        }

        const questions = this.getVisibleQuestions();

        if (questions.length === 0) {
            this.progressElement.textContent = "0/0";
            this.prevButton.disabled = true;
            this.nextButton.disabled = true;
            return;
        }

        const currentIndex = this.getCurrentQuestionIndex();

        this.progressElement.textContent = `${currentIndex + 1}/${questions.length}`;

        this.prevButton.disabled = currentIndex === 0;
        this.nextButton.disabled = currentIndex === questions.length - 1;
    }

    private observeConditionalQuestions(): void {
        const observer = new MutationObserver(() => this.updateNavigationState());

        document.querySelectorAll<HTMLElement>(".survey-question").forEach(question => {
            observer.observe(question, {
                attributes: true,
                attributeFilter: ["class"]
            });
        });
    }
}