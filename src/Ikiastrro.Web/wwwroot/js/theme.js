(function () {
    const storageKey = "ikiastrro-theme";

    function preferredTheme() {
        const saved = localStorage.getItem(storageKey);
        if (saved === "default-light" || saved === "cosmic-light" || saved === "cosmic-dark") {
            return saved;
        }
        if (saved === "light" || saved === "dark") {
            return saved === "dark" ? "cosmic-dark" : "cosmic-light";
        }

        return "default-light";
    }

    function apply(theme, persist) {
        const resolved = ["default-light", "cosmic-light", "cosmic-dark"].includes(theme) ? theme : "default-light";
        document.documentElement.dataset.theme = resolved;
        document.documentElement.style.colorScheme = resolved === "cosmic-dark" ? "dark" : "light";
        if (persist) {
            localStorage.setItem(storageKey, resolved);
        }
        return resolved;
    }

    window.ikiastrroTheme = {
        initialize: function () {
            return apply(preferredTheme(), false);
        },
        get: function () {
            return preferredTheme();
        },
        set: function (theme) {
            return apply(theme, true);
        },
        setAndReload: function (theme) {
            apply(theme, true);
            window.location.reload();
        }
    };

    window.ikiastrroTheme.initialize();
})();
