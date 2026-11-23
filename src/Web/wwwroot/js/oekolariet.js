// Small browser helpers called from Blazor via IJSRuntime.
window.oekolariet = {
    // Opens (or re-focuses) the kiosk window for a project.
    // Returns false when the browser's pop-up blocker stopped it.
    openKioskWindow(url, windowName) {
        const win = window.open(url, windowName, "popup=yes,width=1280,height=800");
        if (!win) return false;
        win.focus();
        return true;
    },

    // Requests full screen. Only works when called from a tap/click.
    async enterFullscreen() {
        const el = document.documentElement;
        if (!el.requestFullscreen || document.fullscreenElement) return !!document.fullscreenElement;
        try {
            await el.requestFullscreen({ navigationUI: "hide" });
            return true;
        } catch {
            return false;
        }
    }
};
