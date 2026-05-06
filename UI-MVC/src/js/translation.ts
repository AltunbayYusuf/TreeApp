declare global {
    interface Window {
        google?: {
            translate?: {
                TranslateElement: new (
                    options: {
                        pageLanguage: string;
                        includedLanguages: string;
                        autoDisplay: boolean;
                    },
                    element: string
                ) => void;
            };
        };
        googleTranslateElementInit?: () => void;
        changeLanguage?: (langCode: string, displayLabel: string) => void;
    }
}

const translateElementId = 'google_translate_element';
const translateScriptId = 'google-translate-script';
const dropdownButtonId = 'languageDropdown';

function ensureTranslateElement(): void {
    if (document.getElementById(translateElementId)) {
        return;
    }

    const translateElement = document.createElement('div');
    translateElement.id = translateElementId;
    translateElement.style.display = 'none';
    document.body.appendChild(translateElement);
}

function getCookie(name: string): string | null {
    const match = document.cookie.match(new RegExp(`(^| )${name}=([^;]+)`));
    return match ? match[2] : null;
}

function updateDropdownLabel(): void {
    const googtransCookie = getCookie('googtrans');
    const dropdownButton = document.getElementById(dropdownButtonId);

    if (!dropdownButton) {
        return;
    }

    dropdownButton.innerHTML = googtransCookie?.endsWith('/en') ? 'EN' : 'NL';
}

function dispatchLanguageChange(selectField: HTMLSelectElement): void {
    selectField.dispatchEvent(new Event('change', { bubbles: true, cancelable: true }));

    window.setTimeout(() => {
        selectField.dispatchEvent(new Event('change', { bubbles: true, cancelable: true }));
    }, 500);
}

window.googleTranslateElementInit = function (): void {
    ensureTranslateElement();

    if (!window.google?.translate?.TranslateElement) {
        return;
    }

    new window.google.translate.TranslateElement({
        pageLanguage: 'nl',
        includedLanguages: 'nl,en',
        autoDisplay: false
    }, translateElementId);
};

window.changeLanguage = function (langCode: string, displayLabel: string): void {
    const dropdownButton = document.getElementById(dropdownButtonId);

    if (dropdownButton && dropdownButton.innerHTML.trim() === displayLabel) {
        return;
    }

    const selectField = document.querySelector<HTMLSelectElement>('.goog-te-combo');

    if (selectField) {
        selectField.value = langCode;
        dispatchLanguageChange(selectField);
    }

    if (dropdownButton) {
        dropdownButton.innerHTML = displayLabel;
    }
};

function loadGoogleTranslateScript(): void {
    if (document.getElementById(translateScriptId)) {
        return;
    }

    const script = document.createElement('script');
    script.id = translateScriptId;
    script.src = '//translate.google.com/translate_a/element.js?cb=googleTranslateElementInit';
    script.async = true;
    document.body.appendChild(script);
}

function setupLanguageOptions(): void {
    document.querySelectorAll<HTMLElement>('[data-language-option]').forEach((option) => {
        option.addEventListener('click', () => {
            const langCode = option.dataset.languageOption;
            const displayLabel = option.dataset.languageLabel ?? langCode?.toUpperCase();

            if (!langCode || !displayLabel) {
                return;
            }

            window.changeLanguage?.(langCode, displayLabel);
        });
    });
}

document.addEventListener('DOMContentLoaded', () => {
    ensureTranslateElement();
    updateDropdownLabel();
    setupLanguageOptions();
    loadGoogleTranslateScript();
});

export {};
