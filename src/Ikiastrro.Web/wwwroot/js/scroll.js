// Astro Facts jump links (Components/Pages/AstroFacts.razor): scroll a section heading into view
// and move focus to it, so keyboard and screen-reader users land where the eye does.
(function () {
    window.ikiastrroScroll = {
        toSection: function (id) {
            var el = document.getElementById(id);
            if (!el) return;
            el.scrollIntoView({ behavior: 'smooth', block: 'start' });
            el.focus({ preventScroll: true });
        }
    };
})();
