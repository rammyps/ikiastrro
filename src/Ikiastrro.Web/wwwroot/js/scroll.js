// Astro Facts jump links (Components/Pages/AstroFacts.razor): scroll a section heading into view
// and move focus to it, so keyboard and screen-reader users land where the eye does.
(function () {
    window.ikiastrroScroll = {
        toSection: function (id) {
            var el = document.getElementById(id);
            if (!el) return;
            el.scrollIntoView({ behavior: 'smooth', block: 'start' });
            el.focus({ preventScroll: true });
        },
        // Compatibility page: open or close every <details> matching the selector.
        setDetails: function (selector, open) {
            document.querySelectorAll(selector).forEach(function (d) { d.open = open; });
        },
        // Compatibility page: mark the nav button of the section nearest the top of the viewport
        // with aria-current, so the sticky section bar shows where the reader is.
        spy: function (navId, ids) {
            var nav = document.getElementById(navId);
            if (!nav || !('IntersectionObserver' in window)) return;
            if (window.ikiastrroSpy) window.ikiastrroSpy.disconnect();
            var seen = {};
            var obs = new IntersectionObserver(function (entries) {
                entries.forEach(function (e) { seen[e.target.id] = e.isIntersecting; });
                var current = ids.filter(function (id) { return seen[id]; })[0];
                if (!current) return;
                nav.querySelectorAll('[data-target]').forEach(function (b) {
                    if (b.getAttribute('data-target') === current) b.setAttribute('aria-current', 'true');
                    else b.removeAttribute('aria-current');
                });
            }, { rootMargin: '-110px 0px -70% 0px' });
            ids.forEach(function (id) { var el = document.getElementById(id); if (el) obs.observe(el); });
            window.ikiastrroSpy = obs;
        }
    };
})();
