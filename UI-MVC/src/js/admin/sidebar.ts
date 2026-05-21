(function (): void {
    const toggle = document.getElementById('adminSidebarToggle');
    const sidebarElement = document.getElementById('adminSidebar');
    const overlayElement = document.getElementById('adminOverlay');

    if (!(sidebarElement instanceof HTMLElement) || !(overlayElement instanceof HTMLElement)) {
        return;
    }

    const sidebar: HTMLElement = sidebarElement;
    const overlay: HTMLElement = overlayElement;

    function openSidebar(): void {
        sidebar.classList.add('is-open');
        overlay.classList.add('is-visible');
        document.body.style.overflow = 'hidden';
    }

    function closeSidebar(): void {
        sidebar.classList.remove('is-open');
        overlay.classList.remove('is-visible');
        document.body.style.overflow = '';
    }

    if (toggle instanceof HTMLElement) {
        toggle.addEventListener('click', function (): void {
            if (sidebar.classList.contains('is-open')) {
                closeSidebar();
            } else {
                openSidebar();
            }
        });
    }

    overlay.addEventListener('click', closeSidebar);
})();

window.addEventListener('load', function (): void {
    document.body.classList.add('css-loaded');
});