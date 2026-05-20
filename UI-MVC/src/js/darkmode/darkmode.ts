const storedTheme = localStorage.getItem('theme') || 'light';
document.documentElement.setAttribute('data-bs-theme', storedTheme);

document.addEventListener('DOMContentLoaded', () => {
    const themeToggleBtn = document.getElementById('themeToggleBtn') as HTMLButtonElement | null;
    const themeIcon = document.getElementById('themeIcon') as HTMLElement | null;

    if (!themeToggleBtn || !themeIcon) return;

    themeIcon.textContent = storedTheme === 'dark' ? '☀️' : '🌙';

    themeToggleBtn.addEventListener('click', () => {
        const htmlElement = document.documentElement;
        const newTheme = htmlElement.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';

        htmlElement.setAttribute('data-bs-theme', newTheme);

        localStorage.setItem('theme', newTheme);

        themeIcon.textContent = newTheme === 'dark' ? '☀️' : '🌙';
    });
});