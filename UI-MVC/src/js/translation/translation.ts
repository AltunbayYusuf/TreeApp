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
const selectedLanguageStorageKey = 'selectedLanguage';
const maxLanguageChangeAttempts = 10;
const defaultLanguageLabel = 'NL';
const englishLanguageLabel = 'EN';

function ensureTranslateElement(): void {
    if (document.getElementById(translateElementId)) {
        return;
    }

    const translateElement = document.createElement('div');
    translateElement.id = translateElementId;
    translateElement.style.display = 'none';
    document.body.appendChild(translateElement);
}

function expireCookie(name: string, path = '/', domain?: string): void {
    const domainPart = domain ? `;domain=${domain}` : '';
    document.cookie = `${name}=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=${path}${domainPart}`;
}

function clearGoogleTranslateCookie(): void {
    expireCookie('googtrans');
    expireCookie('googtrans', window.location.pathname);

    if (window.location.hostname) {
        expireCookie('googtrans', '/', window.location.hostname);
        expireCookie('googtrans', '/', `.${window.location.hostname}`);
    }
}

function updateDropdownLabel(label: string): void {
    const dropdownButton = document.getElementById(dropdownButtonId);

    if (!dropdownButton) {
        return;
    }

    dropdownButton.textContent = label;
}

function getSelectedLanguage(): string {
    return localStorage.getItem(selectedLanguageStorageKey) ?? 'nl';
}

function setSelectedLanguage(langCode: string): void {
    localStorage.setItem(selectedLanguageStorageKey, langCode);
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

    if (getSelectedLanguage() === 'en') {
        applyLanguageChange('en', englishLanguageLabel);
    }
};

window.changeLanguage = function (langCode: string, displayLabel: string): void {
    const normalizedLangCode = langCode.toLowerCase();

    if (normalizedLangCode === 'nl') {
        resetToDutch();
        return;
    }

    applyLanguageChange(normalizedLangCode, displayLabel);
};

function resetToDutch(): void {
    setSelectedLanguage('nl');
    clearGoogleTranslateCookie();
    updateDropdownLabel(defaultLanguageLabel);

    const selectField = document.querySelector<HTMLSelectElement>('.goog-te-combo');
    if (selectField) {
        selectField.value = '';
    }

    window.location.reload();
}

function applyLanguageChange(langCode: string, displayLabel: string, attempt = 1): void {
    const selectField = document.querySelector<HTMLSelectElement>('.goog-te-combo');

    if (!selectField) {
        retryLanguageChange(langCode, displayLabel, attempt);
        return;
    }

    selectField.value = langCode;
    dispatchLanguageChange(selectField);
    setSelectedLanguage(langCode);
    updateDropdownLabel(displayLabel);

    window.setTimeout(() => updateDropdownLabel(displayLabel), 800);
}

function retryLanguageChange(langCode: string, displayLabel: string, attempt: number): void {
    if (attempt >= maxLanguageChangeAttempts) {
        updateDropdownLabel(displayLabel);
        return;
    }

    window.setTimeout(() => applyLanguageChange(langCode, displayLabel, attempt + 1), 300);
}

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
    const selectedLanguage = getSelectedLanguage();

    if (selectedLanguage === 'nl') {
        clearGoogleTranslateCookie();
    }

    ensureTranslateElement();
    updateDropdownLabel(selectedLanguage === 'en' ? englishLanguageLabel : defaultLanguageLabel);
    setupLanguageOptions();
    loadGoogleTranslateScript();
});

export {};
