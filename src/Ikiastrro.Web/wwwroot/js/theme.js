(function () {
    const storageKey = "ikiastrro-theme";

    function preferredTheme() {
        const saved = localStorage.getItem(storageKey);
        if (saved === "default-light" || saved === "nebula-light" || saved === "cosmic-light" || saved === "cosmic-dark") {
            return saved;
        }
        if (saved === "light" || saved === "dark") {
            return saved === "dark" ? "cosmic-dark" : "cosmic-light";
        }

        return "default-light";
    }

    function apply(theme, persist) {
        const resolved = ["default-light", "nebula-light", "cosmic-light", "cosmic-dark"].includes(theme) ? theme : "default-light";
        document.documentElement.dataset.theme = resolved;
        document.documentElement.style.colorScheme = resolved === "cosmic-dark" ? "dark" : "light";
        if (persist) {
            localStorage.setItem(storageKey, resolved);
        }
        return resolved;
    }

    window.ikiastrroTheme = {
        // The print report (/print/{id}) is always default-light, whatever the saved theme; not
        // persisted, so the rest of the app keeps the chosen one. MainLayout reads this result for
        // its MudBlazor palette too.
        initialize: function () {
            const printing = window.location.pathname.toLowerCase().startsWith("/print/");
            return apply(printing ? "default-light" : preferredTheme(), false);
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
