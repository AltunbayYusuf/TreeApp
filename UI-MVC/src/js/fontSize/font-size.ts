const fontSizeStorageKey = 'echo-font-size';
const fontSizeStep = 0.1;
const minFontSize = 0.9;
const maxFontSize = 2;
const defaultFontSize = 1;

const applyFontSize = (size: number) => {
    document.documentElement.style.setProperty('--font-scale', size.toFixed(1));
    localStorage.setItem(fontSizeStorageKey, size.toFixed(1));

    document.querySelectorAll<HTMLElement>('[data-font-size-value]').forEach((value) => {
        value.textContent = `${Math.round(size * 100)}%`;
    });
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
