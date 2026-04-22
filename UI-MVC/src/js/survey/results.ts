// survey/results.ts
import {DomUtils} from '../helpers/utils';

export class SurveyResultsHandler {
    init(): void {
        document.getElementById("ideeGeven")?.addEventListener("click", () => {
            window.location.href = DomUtils.getProjectRedirectUrl("Idea/Create");
        });

        document.getElementById("ideeenBekijken")?.addEventListener("click", () => {
            window.location.href = DomUtils.getProjectRedirectUrl("Idea/Index");
        });
    }
}

document.addEventListener("DOMContentLoaded", () => new SurveyResultsHandler().init());