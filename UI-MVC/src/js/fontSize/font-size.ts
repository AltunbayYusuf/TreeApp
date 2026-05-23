const fontSizeStorageKey = 'echo-font-size';
const fontSizeStep = 0.1;
const minFontSize = 0.9;
const maxFontSize = 2;
const defaultFontSize = 1;

function applyFontSize(size: number): void {
    document.documentElement.style.setProperty('--font-scale', size.toFixed(1));
    localStorage.setItem(fontSizeStorageKey, size.toFixed(1));

    document.querySelectorAll<HTMLElement>('[data-font-size-value]').forEach((value) => {
        value.textContent = `${Math.round(size * 100)}%`;
    });
}

function getStoredFontSize(): number {
    const storedValue = Number(localStorage.getItem(fontSizeStorageKey));

    if (Number.isNaN(storedValue) || storedValue < minFontSize || storedValue > maxFontSize) {
        return defaultFontSize;
    }

    return storedValue;
}

function handleFontSizeClick(button: HTMLElement): void {
    const direction = button.dataset.fontSize;
    const currentSize = getStoredFontSize();

    const nextSize = direction === 'up'
        ? Math.min(currentSize + fontSizeStep, maxFontSize)
        : Math.max(currentSize - fontSizeStep, minFontSize);

    applyFontSize(nextSize);
}

export function initFontSize(): void {
    const fontSizeButtons = document.querySelectorAll<HTMLElement>('[data-font-size]');

    if (fontSizeButtons.length === 0) {
        applyFontSize(defaultFontSize);
        return;
    }

    applyFontSize(getStoredFontSize());

    fontSizeButtons.forEach((button) => {
        button.addEventListener('click', () => handleFontSizeClick(button));
    });
}

initFontSize();