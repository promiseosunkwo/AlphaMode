(() => {
    const progress = () => document.getElementById("navigation-progress");
    const start = () => { const bar = progress(); if (bar) { bar.classList.add("is-active"); bar.setAttribute("aria-hidden", "false"); } };
    const stop = () => { const bar = progress(); if (bar) { bar.classList.remove("is-active"); bar.setAttribute("aria-hidden", "true"); } };

    // Start feedback immediately when a user follows an internal application link.
    document.addEventListener("click", event => {
        const link = event.target.closest("a[href]");
        if (!link || link.target || link.hasAttribute("download")) return;
        const destination = new URL(link.href, window.location.href);
        if (destination.origin === window.location.origin && destination.href !== window.location.href) start();
    });

    // Mark submitted forms once, disable their action, and expose the busy label.
    document.addEventListener("submit", event => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) || form.classList.contains("is-submitting")) return;
        form.classList.add("is-submitting");
        form.setAttribute("aria-busy", "true");
        form.querySelectorAll("[data-loading-submit]").forEach(button => button.disabled = true);
        start();
    });

    // Blazor raises enhancedload after an enhanced navigation has finished rendering.
    document.addEventListener("enhancedload", stop);
    window.addEventListener("pageshow", stop);
    window.addEventListener("popstate", start);
})();

