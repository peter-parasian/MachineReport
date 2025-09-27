(() => {
    "use strict";

    const getStoredTheme = () => localStorage.getItem("theme");
    const setStoredTheme = (theme) => localStorage.setItem("theme", theme);

    const getPreferredTheme = () => {
        const storedTheme = getStoredTheme();
        if (storedTheme) {
            return storedTheme;
        }

        return window.matchMedia("(prefers-color-scheme: dark)").matches
            ? "dark"
            : "light";
    };

    const setTheme = (theme) => {
        if (theme === "auto") {
            document.documentElement.setAttribute(
                "data-bs-theme",
                window.matchMedia("(prefers-color-scheme: dark)").matches
                    ? "dark"
                    : "light"
            );
        } else {
            document.documentElement.setAttribute("data-bs-theme", theme);
        }
    };

    setTheme(getPreferredTheme());

    const showActiveTheme = (theme, focus = false) => {
        const themeSwitcher = document.querySelector("#bd-theme");

        if (!themeSwitcher) {
            return;
        }

        const themeSwitcherText = document.querySelector("#bd-theme-text");
        const activeThemeIcon = document.querySelector(".theme-icon-active use");
        const btnToActive = document.querySelector(
            `[data-bs-theme-value="${theme}"]`
        );
        const svgOfActiveBtn = btnToActive
            .querySelector("svg use")
            .getAttribute("href");

        document.querySelectorAll("[data-bs-theme-value]").forEach((element) => {
            element.classList.remove("active");
            element.setAttribute("aria-pressed", "false");
        });

        btnToActive.classList.add("active");
        btnToActive.setAttribute("aria-pressed", "true");
        activeThemeIcon.setAttribute("href", svgOfActiveBtn);
        const themeSwitcherLabel = themeSwitcherText
            ? `${themeSwitcherText.textContent} (${btnToActive.dataset.bsThemeValue})`
            : `Toggle tema (${btnToActive.dataset.bsThemeValue})`;
        themeSwitcher.setAttribute("aria-label", themeSwitcherLabel);

        if (focus) {
            themeSwitcher.focus();
        }
    };

    window
        .matchMedia("(prefers-color-scheme: dark)")
        .addEventListener("change", () => {
            const storedTheme = getStoredTheme();
            if (storedTheme !== "light" && storedTheme !== "dark") {
                setTheme(getPreferredTheme());
            }
        });

    window.addEventListener("DOMContentLoaded", () => {
        showActiveTheme(getPreferredTheme());

        document.querySelectorAll("[data-bs-theme-value]").forEach((toggle) => {
            toggle.addEventListener("click", () => {
                const theme = toggle.getAttribute("data-bs-theme-value");
                setStoredTheme(theme);
                setTheme(theme);
                showActiveTheme(theme, true);
            });
        });
    });
})();

// Carousel Custom Controls
document.addEventListener('DOMContentLoaded', function () {
    const carousel = document.querySelector('#panasonicCarousel');
    const indicators = document.querySelectorAll('.indicator-dot');

    if (carousel && indicators.length > 0) {
        const bsCarousel = new bootstrap.Carousel(carousel, {
            interval: 4000,
            wrap: true,
            keyboard: true,
            pause: 'hover'
        });

        // Custom indicators click handler
        indicators.forEach((indicator, index) => {
            indicator.addEventListener('click', () => {
                bsCarousel.to(index);
                updateActiveIndicator(index);
            });
        });

        // Update active indicator when carousel slides
        carousel.addEventListener('slid.bs.carousel', (event) => {
            updateActiveIndicator(event.to);
        });

        // Function to update active indicator
        function updateActiveIndicator(activeIndex) {
            indicators.forEach((indicator, index) => {
                indicator.classList.toggle('active', index === activeIndex);
            });
        }

        // Pause carousel on hover
        carousel.addEventListener('mouseenter', () => {
            bsCarousel.pause();
        });

        carousel.addEventListener('mouseleave', () => {
            bsCarousel.cycle();
        });

        // Touch/swipe support for mobile
        let startX = null;
        let currentX = null;

        carousel.addEventListener('touchstart', (e) => {
            startX = e.touches[0].clientX;
        }, { passive: true });

        carousel.addEventListener('touchmove', (e) => {
            if (startX !== null) {
                currentX = e.touches[0].clientX;
            }
        }, { passive: true });

        carousel.addEventListener('touchend', () => {
            if (startX !== null && currentX !== null) {
                const diffX = startX - currentX;
                if (Math.abs(diffX) > 50) {
                    if (diffX > 0) {
                        bsCarousel.next();
                    } else {
                        bsCarousel.prev();
                    }
                }
            }
            startX = null;
            currentX = null;
        }, { passive: true });
    }
});
