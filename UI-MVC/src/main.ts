import 'vite/modulepreload-polyfill';

import 'bootstrap';
import 'bootstrap/dist/css/bootstrap.min.css';

import './main.scss';

import './js/survey/index';
import './js/survey/results';
import './js/survey/chat';
import './js/reaction/index';
import './js/idea/create';
import './js/idea/ideas';
import './js/createSurvey/createSurvey.ts';
import './js/ideation/create-project-ideation'
import './js/subadmin/ideas.ts'
import './js/createSurvey/project-info';
import './js/translation/translation';
import './js/fontSize/font-size';
import './js/admin/newSubplatform.ts';

const fontSizeStorageKey = 'echo-font-size';
const fontSizeStep = 0.1;
const minFontSize = 0.9;
const maxFontSize = 2;
const defaultFontSize = 1;

const applyFontSize = (size: number) => {
    document.documentElement.style.setProperty('--font-scale', size.toFixed(1));
    localStorage.setItem(fontSizeStorageKey, size.toFixed(1));
};

const getStoredFontSize = () => {
    const storedValue = Number(localStorage.getItem(fontSizeStorageKey));

    if (Number.isNaN(storedValue) || storedValue < minFontSize || storedValue > maxFontSize) {
        return defaultFontSize;
    }

    return storedValue;
};

const setupFontSizeControls = () => {
    const fontSizeButtons = document.querySelectorAll<HTMLElement>('[data-font-size]');

    if (fontSizeButtons.length === 0) {
        applyFontSize(defaultFontSize);
        return;
    }

    applyFontSize(getStoredFontSize());

    fontSizeButtons.forEach((button) => {
        button.addEventListener('click', () => {
            const direction = button.dataset.fontSize;
            const currentSize = getStoredFontSize();
            const nextSize = direction === 'up'
                ? Math.min(currentSize + fontSizeStep, maxFontSize)
                : Math.max(currentSize - fontSizeStep, minFontSize);

            applyFontSize(nextSize);
        });
    });
};

setupFontSizeControls();
