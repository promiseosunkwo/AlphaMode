document.addEventListener("click", (event) => {
    const toggle = event.target.closest("[data-password-toggle]");
    if (!toggle) return;

    const input = document.getElementById(toggle.dataset.passwordToggle);
    if (!input) return;

    const isVisible = input.type === "text";
    input.type = isVisible ? "password" : "text";
    toggle.classList.toggle("is-visible", !isVisible);
    toggle.setAttribute("aria-pressed", String(!isVisible));
    toggle.setAttribute("aria-label", isVisible ? "Show password" : "Hide password");
});

