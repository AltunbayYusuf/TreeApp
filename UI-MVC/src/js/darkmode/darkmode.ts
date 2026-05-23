const storageKey = 'theme';
const lightTheme = 'light';
const darkTheme = 'dark';

type Theme = typeof lightTheme | typeof darkTheme;

function getStoredTheme(): Theme {
    return localStorage.getItem(storageKey) === darkTheme ? darkTheme : lightTheme;
}

function applyTheme(theme: Theme): void {
    document.documentElement.setAttribute('data-bs-theme', theme);
    localStorage.setItem(storageKey, theme);
}

function updateThemeIcon(themeIcon: HTMLElement, theme: Theme): void {
    themeIcon.textContent = theme === darkTheme ? '☀️' : '🌙';
}

function getNextTheme(): Theme {
    return document.documentElement.getAttribute('data-bs-theme') === darkTheme
        ? lightTheme
        : darkTheme;
}

export function initDarkMode(): void {
    const themeToggleBtn = document.getElementById('themeToggleBtn');
    const themeIcon = document.getElementById('themeIcon');

    const storedTheme = getStoredTheme();
    applyTheme(storedTheme);

    if (!(themeToggleBtn instanceof HTMLButtonElement) || !(themeIcon instanceof HTMLElement)) {
        return;
    }

    updateThemeIcon(themeIcon, storedTheme);

    themeToggleBtn.addEventListener('click', () => {
        const newTheme = getNextTheme();

        applyTheme(newTheme);
        updateThemeIcon(themeIcon, newTheme);
    });
}

initDarkMode();