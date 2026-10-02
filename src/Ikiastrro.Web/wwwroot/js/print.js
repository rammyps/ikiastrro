// Print report (Components/Pages/PrintReport.razor): open the browser's print dialog once the
// modules have laid out — fonts loaded, then a beat for the chart grids' own sizing scripts.
(function () {
    window.ikiastrroPrint = {
        whenReady: async function () {
            if (document.fonts && document.fonts.ready) {
                await document.fonts.ready;
            }
            await new Promise(function (resolve) { setTimeout(resolve, 600); });
            window.print();
        }
    };
})();
