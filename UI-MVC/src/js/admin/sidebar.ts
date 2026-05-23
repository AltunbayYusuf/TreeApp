class AdminSidebar {
    private readonly sidebar = document.getElementById("adminSidebar");
    private readonly overlay = document.getElementById("adminOverlay");
    private readonly toggleButton = document.getElementById("adminSidebarToggle");
    private readonly mobileBreakpointPixels = 992;

    init(): void {
        if (!this.hasRequiredElements()) {
            return;
        }

        this.bindEvents();
    }

    private hasRequiredElements(): boolean {
        return this.sidebar instanceof HTMLElement &&
            this.overlay instanceof HTMLElement;
    }

    private bindEvents(): void {
        this.toggleButton?.addEventListener("click", () => {
            this.toggleSidebar();
        });

        this.overlay?.addEventListener("click", () => {
            this.closeSidebar();
        });

        document.addEventListener("keydown", (event) => {
            if (event.key === "Escape") {
                this.closeSidebar();
            }
        });

        this.sidebar?.querySelectorAll<HTMLAnchorElement>(".sidebar-link").forEach((link) => {
            link.addEventListener("click", () => {
                if (this.isMobileViewport()) {
                    this.closeSidebar();
                }
            });
        });

        window.addEventListener("resize", () => {
            if (!this.isMobileViewport()) {
                this.closeSidebar();
            }
        });
    }

    private toggleSidebar(): void {
        if (this.isSidebarOpen()) {
            this.closeSidebar();
            return;
        }

        this.openSidebar();
    }

    private openSidebar(): void {
        if (!(this.sidebar instanceof HTMLElement) || !(this.overlay instanceof HTMLElement)) {
            return;
        }

        this.sidebar.classList.add("is-open");
        this.overlay.classList.add("is-visible");
        document.body.style.overflow = "hidden";
    }

    private closeSidebar(): void {
        if (!(this.sidebar instanceof HTMLElement) || !(this.overlay instanceof HTMLElement)) {
            return;
        }

        this.sidebar.classList.remove("is-open");
        this.overlay.classList.remove("is-visible");
        document.body.style.overflow = "";
    }

    private isSidebarOpen(): boolean {
        return this.sidebar instanceof HTMLElement &&
            this.sidebar.classList.contains("is-open");
    }

    private isMobileViewport(): boolean {
        return window.innerWidth < this.mobileBreakpointPixels;
    }
}

class CssLoadedMarker {
    init(): void {
        window.addEventListener("load", () => {
            document.body.classList.add("css-loaded");
        });
    }
}

new AdminSidebar().init();
new CssLoadedMarker().init();