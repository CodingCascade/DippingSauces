"use strict";

// The toggle changes only the input display. It never logs or stores passwords.
document.addEventListener("click", function (event) {
    if (!(event.target instanceof Element)) {
        return;
    }

    const button = event.target.closest("[data-password-toggle]");
    if (button === null) {
        return;
    }

    const input = document.getElementById(button.dataset.passwordToggle);
    if (!(input instanceof HTMLInputElement)) {
        return;
    }

    const showPassword = input.type === "password";
    input.type = showPassword ? "text" : "password";

    const label = showPassword ? "Hide password" : "Show password";
    button.setAttribute("aria-label", label);
    button.setAttribute("aria-pressed", String(showPassword));
    button.title = label;

    const icon = button.querySelector("i");
    if (icon !== null) {
        icon.classList.toggle("bi-eye", !showPassword);
        icon.classList.toggle("bi-eye-slash", showPassword);
    }
});